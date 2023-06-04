using Akka.Actor;
using Job.Core.Models;
using Job.Core.Theater.Groups;
using Job.Core.Theater.Workers.Messages;

namespace Job.Core.Theater.Master;

internal class MasterActor : ReceiveActor
{
    private readonly Dictionary<string, IActorRef> _groupIdToActor = new();
    private readonly Dictionary<IActorRef, string> _actorToGroupId = new();
    
    public MasterActor()
    {
        Receive<DoJobCommand>(StartJobCommandHandler);
        Receive<StopJobCommand>(StopJobCommandHandler);
        
        Receive<Terminated>(GroupActorTerminatedHandler);
    }

    private void StopJobCommandHandler(StopJobCommand command)
    {
        if (!_groupIdToActor.ContainsKey(command.GroupName))
        {
            Sender.Tell(new StopJobCommandResult(false, $"_groupIdToActor does not contain {command.GroupName}"));
            return;
        }

        _groupIdToActor[command.GroupName].Forward(command);
    }

    private void GroupActorTerminatedHandler(Terminated t)
    {
        var groupId = _actorToGroupId[t.ActorRef];
        _actorToGroupId.Remove(t.ActorRef);
        _groupIdToActor.Remove(groupId);
        
        // var text = $"Group actor for {groupId} has been terminated";
        // LogActorState(text);
    }
    
    private void StartJobCommandHandler(DoJobCommand command)
    {
        if (_groupIdToActor.TryGetValue(command.GroupName, out var actorRef))
        {
            if ((actorRef is LocalActorRef localActorRef) && localActorRef.IsTerminated)
            {
                Sender.Tell(new JobCommandResult(false, 
                    "IsTerminated == true. Group Actor has been terminated.", command.JobId));
                return;
            }

            actorRef.Forward(command);
            return;
        }
        
        var groupActor = Context.ActorOf(GroupActor.Props(command.JobResultType), $"group-{command.JobResultType}");
        Context.Watch(groupActor);
        groupActor.Forward(command);
        _groupIdToActor.Add(command.GroupName, groupActor);
        _actorToGroupId.Add(groupActor, command.GroupName);
    }
    
    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: -1,
            withinTimeRange: TimeSpan.FromMilliseconds(-1),
            localOnlyDecider: ex => Directive.Stop);
    }
}