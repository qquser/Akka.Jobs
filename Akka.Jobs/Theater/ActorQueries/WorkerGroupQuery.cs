using Akka.Actor;
using Akka.Jobs.Interfaces;
using Akka.Jobs.Theater.ActorQueries.Messages;
using Akka.Jobs.Theater.ActorQueries.Messages.States;
using Akka.Jobs.Theater.Master.Groups.Workers.Messages;

namespace Akka.Jobs.Theater.ActorQueries;

internal sealed class CollectionTimeout
{
    public static CollectionTimeout Instance { get; } = new ();

    private CollectionTimeout()
    {
    }
}

internal class WorkerGroupQuery<TOut> : UntypedActor
    where TOut : IJobResult
{
    private readonly ICancelable _queryTimeoutTimer;

    public WorkerGroupQuery(Dictionary<IActorRef, Guid> actorToWorkerId, long requestId,
        IActorRef requester, TimeSpan timeout)
    {
        ActorToWorkerId = actorToWorkerId;
        RequestId = requestId;
        Requester = requester;
        Timeout = timeout;
        _queryTimeoutTimer =
            Context.System.Scheduler.ScheduleTellOnceCancelable(timeout, Self, CollectionTimeout.Instance, Self);

        Become(WaitingForReplies(new Dictionary<Guid, ReplyWorkerInfo<TOut>>(),
            new HashSet<IActorRef>(ActorToWorkerId.Keys)));
    }

    protected override void PreStart()
    {
        foreach (var actorRef in ActorToWorkerId.Keys)
        {
            Context.Watch(actorRef);
            actorRef.Tell(new ReadWorkerInfoCommand(0, Guid.Empty, ""));
        }
    }

    protected override void PostStop()
    {
        _queryTimeoutTimer.Cancel();
    }

    public Dictionary<IActorRef, Guid> ActorToWorkerId { get; }
    public long RequestId { get; }
    public IActorRef Requester { get; }
    public TimeSpan Timeout { get; }
        
        
    public UntypedReceive WaitingForReplies(
        Dictionary<Guid, ReplyWorkerInfo<TOut>> repliesSoFar,
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
                    var replies = new Dictionary<Guid, ReplyWorkerInfo<TOut>>(repliesSoFar);
                    foreach (var actor in stillWaiting)
                    {
                        var workerId = ActorToWorkerId[actor];
                        replies.Add(workerId, new ReplyWorkerInfo<TOut>(false, "Worker Actor Timed Out"));
                    }
                    Requester.Tell(new RespondAllWorkersInfo<TOut>(RequestId, replies));
                    Context.Stop(Self);
                    break;
            }
        };
    }


    public void ReceivedResponse(
        IActorRef workerActor,
        ReplyWorkerInfo<TOut> reading,
        HashSet<IActorRef> stillWaiting,
        Dictionary<Guid, ReplyWorkerInfo<TOut>> repliesSoFar)
    {
        Context.Unwatch(workerActor);
        var workerId = ActorToWorkerId[workerActor];
        stillWaiting.Remove(workerActor);

        repliesSoFar.Add(workerId, reading);

        if (stillWaiting.Count == 0)
        {
            Requester.Tell(new RespondAllWorkersInfo<TOut>(RequestId, repliesSoFar));
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

    public static Props Props(Dictionary<IActorRef, Guid> actorToWorkerId, long requestId,
        IActorRef requester, TimeSpan timeout) =>
        Akka.Actor.Props.Create(() => new WorkerGroupQuery<TOut>(actorToWorkerId, requestId, requester, timeout));
}