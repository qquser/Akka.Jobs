using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Pattern;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Theater.ActorQueries.Messages.States;
using Job.Core.Theater.Master.Groups.Workers.Messages;
using Microsoft.Extensions.Logging;

namespace Job.Core.Theater.Master.Groups.Workers;

internal class ManagerActor<TIn, TOut> : ReceiveActor
    where TIn : IJobInput
    where TOut : IJobResult
{
    private int _currentNrOfRetries;
    private int _maxNrOfRetries;
    private TimeSpan _minBackoff;
    private TimeSpan _maxBackoff;
    private IActorRef? _doJobCommandSender;
    private IActorRef? _workerSupervisorActor;
    private WorkerDoJobCommand<TIn>? _doJobCommand;
    
    private Guid _jobId;
    private bool _startedFlag = false;
    private readonly CancellationTokenSource _cancellationTokenSource = new ();
    
    private readonly ILogger<ManagerActor<TIn, TOut>> _logger;
    
    public ManagerActor(ILogger<ManagerActor<TIn, TOut>> logger)
    {
        _logger = logger;
        //Commands
        Receive<DoJobCommand<TIn>>(DoJobCommandHandler);
        Receive<StopJobCommand>(StopJobCommandHandler);
        
        //Queries
        Receive<ReadWorkerInfoCommand>(ReadWorkerInfoCommandHandler);
        
        //Internal
        Receive<TrySaveWorkerActorRefCommand>(TrySaveWorkerActorRefCommandHandler);
        Receive<GiveMeWorkerDoJobCommand>(GiveMeWorkerDoJobCommandHandler);
        Receive<Terminated>(WorkerActorTerminatedHandler);
    }

    private void TrySaveWorkerActorRefCommandHandler(TrySaveWorkerActorRefCommand msg)
    {
        if (!_startedFlag)
        {
            var groupActor = Context.Parent;
            if ((groupActor is LocalActorRef localActorRef) && localActorRef.IsTerminated)
            {
                _doJobCommandSender.Tell(new JobDoneCommandResult(false, 
                    "TrySaveWorkerActorRefCommand Failed. IsTerminated == true. Group Actor has been terminated.",
                    _jobId));
                return;
            }
    
            groupActor.Tell(msg);
            _startedFlag = true;
        }
    }

    private void ReadWorkerInfoCommandHandler(ReadWorkerInfoCommand command)
    {
        if (_workerSupervisorActor == null)
        {
            Sender.Tell(new RespondWorkerInfo<TOut>(false,
                command.RequestId, 
                "_workerSupervisorActor == null") );
            return;
        }
        
        _workerSupervisorActor.Forward(command);
    }

    private void GiveMeWorkerDoJobCommandHandler(GiveMeWorkerDoJobCommand _)
    {
        _workerSupervisorActor.Tell(_doJobCommand);
    }

    private void StopJobCommandHandler(StopJobCommand _)
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
            _workerSupervisorActor.Tell(PoisonPill.Instance);
            Sender.Tell(new StopJobCommandResult(true, "Ok"));
            return;
        }
        Sender.Tell(new StopJobCommandResult(false, "Cancellation Requested Already."));
    }
    
    private void WorkerActorTerminatedHandler(Terminated t)
    {
        Self.Tell(PoisonPill.Instance);
    }

    private void DoJobCommandHandler(DoJobCommand<TIn> doJobCommand)
    {
        if (_workerSupervisorActor != null)
        {
            var message = "Ignoring Create Worker Actor";
            Sender.Tell(doJobCommand.IsCreateCommand
                ? new JobCreatedCommandResult(false, message, doJobCommand.JobId)
                : new JobDoneCommandResult(false, message, doJobCommand.JobId));
            return;
        }

        _jobId = doJobCommand.JobId;
        _maxNrOfRetries = doJobCommand.MaxNrOfRetries;
        _minBackoff = doJobCommand.MinBackoff;
        _maxBackoff = doJobCommand.MaxBackoff;
        _doJobCommandSender = Sender;

        var workerActorProps = DependencyResolver
            .For(Context.System)
            .Props<WorkerActor<TIn,TOut>>();

        var supervisorOfWorkerActorProps = BackoffSupervisor.Props(
            Backoff.OnFailure(
                    workerActorProps,
                    childName: $"worker-{doJobCommand.JobId}",
                    minBackoff: _minBackoff,
                    maxBackoff: _maxBackoff,
                    randomFactor: 0.2,
                    maxNrOfRetries: _maxNrOfRetries)
                .WithSupervisorStrategy(new OneForOneStrategy(exception =>
                {
                    if (exception is TaskCanceledException 
                        || exception.InnerException is TaskCanceledException
                        || _currentNrOfRetries >= _maxNrOfRetries)
                    {
                        var text = $"BackoffSupervisor: jobId: {_jobId}" +
                                   $" {exception?.Message}" +
                                   $" InnerException: {exception?.InnerException?.Message}";
                        _logger.LogError(text);
                        _doJobCommandSender.Tell(new JobDoneCommandResult(false, text, _jobId));
                        return Directive.Stop;
                    }

                    _currentNrOfRetries += 1;
                    return Directive.Restart;
                })));
        _workerSupervisorActor = Context
            .ActorOf(supervisorOfWorkerActorProps, $"supervisor-of-worker-{doJobCommand.JobId}");
  
        Context.Watch(_workerSupervisorActor);
        
        _doJobCommand = new WorkerDoJobCommand<TIn>(
            doJobCommand.JobInput,
            _doJobCommandSender, 
            doJobCommand.JobId,
            _cancellationTokenSource,
            doJobCommand.IsCreateCommand);
    }
    
    protected override void PostStop()
    {
        _cancellationTokenSource?.Dispose();
    }

    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: -1,
            withinTimeRange: TimeSpan.FromMilliseconds(-1),
            localOnlyDecider: ex => Directive.Stop);
    }
}