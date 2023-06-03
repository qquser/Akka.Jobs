using Akka.Actor;
using Job.Core.Models;
using Job.Core.Theater.Groups;
using Job.Core.Theater.Workers.Messages;

namespace Job.Core.Theater.Master;

public class MasterActor : ReceiveActor
{
    private readonly Dictionary<string, IActorRef> _groupIdToActor = new();
    private readonly Dictionary<IActorRef, string> _actorToGroupId = new();
    
    public MasterActor()
    {
        Receive<StartJobCommand>(StartJobCommandHandler);
        //Receive<StopJobCommand>(StopJobCommandHandler);
        
        Receive<Terminated>(GroupActorTerminatedHandler);
    }
    
    private void GroupActorTerminatedHandler(Terminated t)
    {
        var groupId = _actorToGroupId[t.ActorRef];
        _actorToGroupId.Remove(t.ActorRef);
        _groupIdToActor.Remove(groupId);
        
        // var text = $"Group actor for {groupId} has been terminated";
        // LogActorState(text);
    }
    
    private void StartJobCommandHandler(StartJobCommand command)
    {
        if (_groupIdToActor.TryGetValue(command.GroupId, out var actorRef))
        {
            if ((actorRef is LocalActorRef localActorRef) && localActorRef.IsTerminated)
            {
                Sender.Tell(new StartJobCommandResult(false, "IsTerminated == true. Group Actor has been terminated."));
                return;
            }

            actorRef.Forward(command);
            return;
        }
        
        var groupActor = Context.ActorOf(GroupActor.Props(command.GroupId), $"group-{command.GroupId}");
        Context.Watch(groupActor);
        groupActor.Forward(command);
        _groupIdToActor.Add(command.GroupId, groupActor);
        _actorToGroupId.Add(groupActor, command.GroupId);
    }
    
    public static Props Props() => Akka.Actor.Props.Create<MasterActor>();
}