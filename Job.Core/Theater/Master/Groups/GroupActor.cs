using Akka.Actor;
using Akka.DependencyInjection;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Theater.ActorQueries.Messages;
using Job.Core.Theater.Master.Groups.Workers;
using Job.Core.Theater.Master.Groups.Workers.Messages;

namespace Job.Core.Theater.Master.Groups;

internal class GroupActor<TIn, TOut> : ReceiveActor
    where TIn : IJobInput
    where TOut : IJobResult
{
    private string? _groupId;

    private readonly Dictionary<Guid, IActorRef> _idToManagerActor = new();
    private readonly Dictionary<IActorRef, Guid> _managerActorToId = new();

    public GroupActor()
    {
        //Commands
        Receive<DoJobCommand<TIn>>(StartJobCommandHandler);
        Receive<StopJobCommand>(StopJobCommandHandler);

        //Queries
        Receive<RequestAllWorkersInfo>(RequestAllWorkersInfoQueryHandler);

        //Internal
        Receive<Terminated>(ManagerActorTerminatedHandler);
    }

    private void RequestAllWorkersInfoQueryHandler(RequestAllWorkersInfo obj)
    {
        throw new NotImplementedException();
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

    private void StartJobCommandHandler(DoJobCommand<TIn> doJobCommand)
    {
        if (_groupId != null && doJobCommand.GroupName != _groupId)
        {
            Sender.Tell(new JobCommandResult(false, "Ignoring Create Worker Actor", doJobCommand.JobId));
            return;
        }

        if (_idToManagerActor.ContainsKey(doJobCommand.JobId))
        {
            Sender.Tell(new JobCommandResult(false, $"{doJobCommand.JobId} Actor Exists.", doJobCommand.JobId));
            return;
        }

        _groupId ??= doJobCommand.GroupName;

        var managerActorProps = DependencyResolver
            .For(Context.System)
            .Props(typeof(ManagerActor<,>)
                .MakeGenericType(typeof(TIn), typeof(TOut)));
        
        var managerActor = Context.ActorOf(managerActorProps, $"manager-{doJobCommand.JobId}");

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
}