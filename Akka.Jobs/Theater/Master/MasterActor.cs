using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Jobs.Interfaces;
using Akka.Jobs.Models;
using Akka.Jobs.Theater.ActorQueries.Messages;
using Akka.Jobs.Theater.Master.Groups;
using Akka.Jobs.Theater.Master.Groups.Workers.Messages;

namespace Akka.Jobs.Theater.Master;

internal class MasterActor<TIn, TOut> : ReceiveActor
    where TIn : IJobInput
    where TOut : IJobResult
{
    private readonly Dictionary<string, IActorRef> _groupIdToActor = new();
    private readonly Dictionary<IActorRef, string> _actorToGroupId = new();
    
    public MasterActor()
    {
        //Commands
        Receive<DoJobCommand<TIn>>(DoJobCommandHandler);
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
    }
    
    private void DoJobCommandHandler(DoJobCommand<TIn> doJobCommand)
    {
        if (_groupIdToActor.TryGetValue(doJobCommand.GroupName, out var actorRef))
        {
            if ((actorRef is LocalActorRef localActorRef) && localActorRef.IsTerminated)
            {
                var message = "Group Actor has been terminated.";
                Sender.Tell(doJobCommand.IsCreateCommand
                    ? new JobCreatedCommandResult(false, message, doJobCommand.JobId)
                    : new JobDoneCommandResult(false, message, doJobCommand.JobId));

                return;
            }

            actorRef.Forward(doJobCommand);
            return;
        }
        
        var groupActorProps = DependencyResolver
            .For(Context.System)
            .Props<GroupActor<TIn,TOut>>();
        
        var groupActor = Context.ActorOf(groupActorProps, $"group-{doJobCommand.GroupName}");
        Context.Watch(groupActor);
        groupActor.Forward(doJobCommand);
        _groupIdToActor.Add(doJobCommand.GroupName, groupActor);
        _actorToGroupId.Add(groupActor, doJobCommand.GroupName);
    }
    
    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: -1,
            withinTimeRange: TimeSpan.FromMilliseconds(-1),
            localOnlyDecider: ex => Directive.Stop);
    }
}