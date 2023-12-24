using Akka.Actor;
using Akka.Jobs.Interfaces;
using Akka.Jobs.Theater.ActorQueries.Messages;
using Akka.Jobs.Theater.ActorQueries.Messages.States;
using Akka.Jobs.Theater.Master.Groups.Workers.Messages;

namespace Akka.Jobs.Theater.ActorQueries;

internal sealed class WorkerGroupQuery<TOut> : UntypedActor
    where TOut : IJobResult
{
    private readonly ICancelable _queryTimeoutTimer;
    private readonly Dictionary<IActorRef, string> _actorToWorkerId;
    private readonly long _requestId;
    private readonly IActorRef _requester;


    public WorkerGroupQuery(Dictionary<IActorRef, string> actorToWorkerId, long requestId,
        IActorRef requester, TimeSpan timeout)
    {
        _actorToWorkerId = actorToWorkerId;
        _requestId = requestId;
        _requester = requester;
        _queryTimeoutTimer =
            Context.System.Scheduler.ScheduleTellOnceCancelable(timeout, Self, CollectionTimeout.Instance, Self);

        Become(WaitingForReplies(new Dictionary<string, ReplyWorkerInfo<TOut>>(),
            new HashSet<IActorRef>(_actorToWorkerId.Keys)));
    }

    protected override void PreStart()
    {
        foreach (var actorRef in _actorToWorkerId.Keys)
        {
            Context.Watch(actorRef);
            actorRef.Tell(new ReadWorkerInfoCommand(0, string.Empty, string.Empty));
        }
    }

    protected override void PostStop()
    {
        _queryTimeoutTimer.Cancel();
    }
    
    private UntypedReceive WaitingForReplies(
        Dictionary<string, ReplyWorkerInfo<TOut>> repliesSoFar,
        HashSet<IActorRef> stillWaiting)
    {
        return message =>
        {
            switch (message)
            {
                case RespondWorkerInfo<TOut> response when response.RequestId == 0:
                    var workerActor = Sender;
                    ReplyWorkerInfo<TOut> reading;
                    if (response?.Success == true && response.Result != null)
                    {
                        reading = new ReplyWorkerInfo<TOut>(response.Result);
                    }
                    else
                    {
                        reading = new ReplyWorkerInfo<TOut>(false, "Worker Data Not Available");
                    }
                    ReceivedResponse(workerActor, reading, stillWaiting, repliesSoFar);
                    break;
                case Terminated t:
                    ReceivedResponse(t.ActorRef, new ReplyWorkerInfo<TOut>(false, "Worker Actor Not Available"),
                        stillWaiting,
                        repliesSoFar);
                    break;
                case CollectionTimeout _:
                    var replies = new Dictionary<string, ReplyWorkerInfo<TOut>>(repliesSoFar);
                    foreach (var actor in stillWaiting)
                    {
                        if (_actorToWorkerId.TryGetValue(actor, out var workerId))
                        {
                            replies.Add(workerId, new ReplyWorkerInfo<TOut>(false, "Worker Actor Timed Out"));
                        }
                    }
                    _requester.Tell(new RespondAllWorkersInfo<TOut>(_requestId, replies));
                    Context.Stop(Self);
                    break;
            }
        };
    }


    private void ReceivedResponse(
        IActorRef workerActor,
        ReplyWorkerInfo<TOut> reading,
        HashSet<IActorRef> stillWaiting,
        Dictionary<string, ReplyWorkerInfo<TOut>> repliesSoFar)
    {
        if (!_actorToWorkerId.TryGetValue(workerActor, out var workerId)) 
            return;
        
        Context.Unwatch(workerActor);
        stillWaiting.Remove(workerActor);
        repliesSoFar.Add(workerId, reading);
        
        if (stillWaiting.Count == 0)
        {
            _requester.Tell(new RespondAllWorkersInfo<TOut>(_requestId, repliesSoFar));
            Context.Stop(Self);
        }
        else
        {
            Context.Become(WaitingForReplies(repliesSoFar, stillWaiting));
        }
    }

    protected override void OnReceive(object message)
    {
    }

    public static Props Props(Dictionary<IActorRef, string> actorToWorkerId, long requestId,
        IActorRef requester, TimeSpan timeout) =>
        Akka.Actor.Props.Create(() => new WorkerGroupQuery<TOut>(actorToWorkerId, requestId, requester, timeout));
}