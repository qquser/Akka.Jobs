using Akka.Actor;
using Akka.DependencyInjection;
using Job.Core.Models;
using Job.Core.Theater.Workers;
using Job.Core.Theater.Workers.Messages;

namespace Job.Core.Theater.Groups;

internal class GroupActor : ReceiveActor
{
    private readonly string _groupId;
    
    private readonly Dictionary<Guid, IActorRef> _idToJobActor = new ();
    private readonly Dictionary<IActorRef, Guid> _jobActorToId = new ();
    public GroupActor(string groupId)
    {
        _groupId = groupId;
        
        Receive<DoJobCommand>(StartJobCommandHandler);

        //Receive<Terminated>(DownloadActorTerminatedHandler);
    }
    
    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: -1,
            withinTimeRange: TimeSpan.FromMilliseconds(-1),
            localOnlyDecider: ex => Directive.Stop);
    }

    private void StartJobCommandHandler(DoJobCommand createMsg)
    {
        if (!createMsg.GroupId.Equals(_groupId))
        {
            Sender.Tell(new JobCommandResult(false, "IgnoringCreateDownload_Info"));
            return;
        }

        if (_idToJobActor.ContainsKey(createMsg.JobId))
        {
            Sender.Tell(new JobCommandResult(false, "ActorExists_Info"));
            return;
        }
        
        var dependencyResolver = DependencyResolver.For(Context.System);
        var workerActorProps = dependencyResolver
            .Props<WorkerActor>(createMsg.GroupId, createMsg.JobId);
        
        var workerActor = Context.ActorOf(workerActorProps);
        // var workerActor = Context.ActorOf(
        //     WorkerActor.Props(createMsg.GroupId, createMsg.JobId),
        //     $"worker-{createMsg.JobId}");
        //
        Context.Watch(workerActor);
        
        _idToJobActor.Add(createMsg.JobId, workerActor);
        _jobActorToId.Add(workerActor, createMsg.JobId);
        workerActor.Forward(createMsg);
    }

    public static Props Props(string myGroupId) =>
        Akka.Actor.Props.Create(() => new GroupActor(myGroupId));
}