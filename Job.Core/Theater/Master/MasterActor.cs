using Akka.Actor;
using Akka.DependencyInjection;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Theater.ActorQueries.Messages;
using Job.Core.Theater.Master.Groups;
using Job.Core.Theater.Master.Groups.Workers.Messages;

namespace Job.Core.Theater.Master;

internal class MasterActor<TIn, TOut> : ReceiveActor
    where TIn : IJobInput
    where TOut : IJobResult
{
    private readonly Dictionary<string, IActorRef> _groupIdToActor = new();
    private readonly Dictionary<IActorRef, string> _actorToGroupId = new();
    
    public MasterActor()
    {
        //Commands
        Receive<DoJobCommand<TIn>>(StartJobCommandHandler);
        Receive<StopJobCommand>(StopJobCommandHandler);
        
        //Queries
        Receive<RequestAllWorkersInfo>(RequestAllWorkersInfoQueryHandler);
        
        //Internal
        Receive<Terminated>(GroupActorTerminatedHandler);
    }

    private void RequestAllWorkersInfoQueryHandler(RequestAllWorkersInfo msg)
    {
        if (_groupIdToActor.TryGetValue(msg.GroupId, out var groupActor))
        {
            groupActor.Forward(msg);
            return;
        }

        Sender.Tell(new RespondAllWorkersInfo<TOut>(msg.RequestId));
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
    
    private void StartJobCommandHandler(DoJobCommand<TIn> command)
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
        
        var groupActorProps = DependencyResolver
            .For(Context.System)
            .Props(typeof(GroupActor<,>)
                .MakeGenericType(typeof(TIn), typeof(TOut)));
        
        var groupActor = Context.ActorOf(groupActorProps, $"group-{command.GroupName}");
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