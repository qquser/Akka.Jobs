using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Jobs.Interfaces;
using Akka.Jobs.Models;
using Akka.Jobs.Theater.ActorQueries;
using Akka.Jobs.Theater.ActorQueries.Messages;
using Akka.Jobs.Theater.Master.Groups.Workers;
using Akka.Jobs.Theater.Master.Groups.Workers.Messages;

namespace Akka.Jobs.Theater.Master.Groups;

internal sealed class GroupActor<TIn, TOut> : ReceiveActor
    where TIn : IJobInput
    where TOut : IJobResult
{
    private string? _groupId;

    private readonly Dictionary<string, IActorRef> _idToManagerActor = new();
    private readonly Dictionary<IActorRef, string> _managerActorToId = new();
    
    private readonly Dictionary<IActorRef, string> _workerActorToId = new ();
    private readonly Dictionary<string, IActorRef> _idToWorkerActor = new ();

    public GroupActor()
    {
        //Commands
        Receive<DoJobCommand<TIn>>(DoJobCommandHandler);
        Receive<StopJobCommand>(StopJobCommandHandler);

        //Queries
        Receive<RequestAllWorkersInfo>(RequestAllWorkersInfoQueryHandler);

        //Internal
        Receive<TrySaveWorkerActorRefCommand>(TrySaveWorkerActorRefCommandHandler);
        Receive<Terminated>(ManagerActorTerminatedHandler);
    }
    
    private void ManagerActorTerminatedHandler(Terminated t)
    {
        var workerId = _managerActorToId[t.ActorRef];
        _managerActorToId.Remove(t.ActorRef);
        _idToManagerActor.Remove(workerId);

        if (!_idToWorkerActor.TryGetValue(workerId, out var workerActorRef)) return;
        _workerActorToId.Remove(workerActorRef);
        _idToWorkerActor.Remove(workerId);
    }
    
    private void TrySaveWorkerActorRefCommandHandler(TrySaveWorkerActorRefCommand msg)
    {
        _workerActorToId.TryAdd(msg.ActorRef, msg.SlaveActorId);
        _idToWorkerActor.TryAdd(msg.SlaveActorId, msg.ActorRef);
    }

    private void RequestAllWorkersInfoQueryHandler(RequestAllWorkersInfo msg)
    {
        if (_workerActorToId?.Any() != true)
        {
            Sender.Tell(new RespondAllWorkersInfo<TOut>(msg.RequestId));
            return;
        }

        Context.ActorOf(
            WorkerGroupQuery<TOut>.Props(_workerActorToId, msg.RequestId, Sender, msg.Timeout));
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

    private void DoJobCommandHandler(DoJobCommand<TIn> doJobCommand)
    {
        if (_groupId != null && doJobCommand.GroupName != _groupId)
        {
            var message = "Ignoring Create Worker Actor";
            Sender.Tell(doJobCommand.IsCreateCommand
                    ? new JobCreatedCommandResult(false, message, doJobCommand.JobId)
                    : new JobDoneCommandResult(false, message, doJobCommand.JobId));
            return;
        }

        if (_idToManagerActor.ContainsKey(doJobCommand.JobId))
        {
            var message = $"{doJobCommand.JobId} Actor Exists.";
            Sender.Tell(doJobCommand.IsCreateCommand
                ? new JobCreatedCommandResult(false, message, doJobCommand.JobId)
                : new JobDoneCommandResult(false, message, doJobCommand.JobId));
            return;
        }

        _groupId ??= doJobCommand.GroupName;

        var managerActorProps = DependencyResolver
            .For(Context.System)
            .Props<ManagerActor<TIn,TOut>>();
        
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