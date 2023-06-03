using Akka.Actor;
using Job.Core.Models;
using Job.Core.Theater.Workers;
using Job.Core.Theater.Workers.Messages;

namespace Job.Core.Theater.Groups;

internal class GroupActor : ReceiveActor
{
    private readonly Type _groupId;
    
    private readonly Dictionary<Guid, IActorRef> _idToManagerActor = new ();
    private readonly Dictionary<IActorRef, Guid> _managerActorToId = new ();
    
    public GroupActor(Type groupId)
    {
        _groupId = groupId;
        
        Receive<DoJobCommand>(StartJobCommandHandler);
        Receive<StopJobCommand>(StopJobCommandHandler);

        Receive<Terminated>(ManagerActorTerminatedHandler);
    }

    private void ManagerActorTerminatedHandler(Terminated t)
    {
        var workerId = _managerActorToId[t.ActorRef];
        _managerActorToId.Remove(t.ActorRef);
        _idToManagerActor.Remove(workerId);
    }

    private void StopJobCommandHandler(StopJobCommand command)
    {
        if (!_idToManagerActor.ContainsKey(command.JobId))
        {
            Sender.Tell(new StopJobCommandResult(false, $"_idToWorkerActor does not contain {command.JobId}"));
            return;
        }

        _idToManagerActor[command.JobId].Forward(command);
    }

    private void StartJobCommandHandler(DoJobCommand doJobCommand)
    {
        if (doJobCommand.GroupType != _groupId)
        {
            Sender.Tell(new JobCommandResult(false, "Ignoring Create Worker Actor", doJobCommand.JobId));
            return;
        }

        if (_idToManagerActor.ContainsKey(doJobCommand.JobId))
        {
            Sender.Tell(new JobCommandResult(false, $"{doJobCommand.JobId} Actor Exists.", doJobCommand.JobId));
            return;
        }
        
        var managerActor = Context.ActorOf(ManagerActor.Props(doJobCommand.JobId), $"manager-{doJobCommand.JobId}");

        Context.Watch(managerActor);
        
        _idToManagerActor.Add(doJobCommand.JobId, managerActor);
        _managerActorToId.Add(managerActor, doJobCommand.JobId);
        managerActor.Forward(doJobCommand);
    }

    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: -1,
            withinTimeRange: TimeSpan.FromMilliseconds(-1),
            localOnlyDecider: ex => Directive.Stop);
    }
    
    public static Props Props(Type myGroupId) =>
        Akka.Actor.Props.Create(() => new GroupActor(myGroupId));
}