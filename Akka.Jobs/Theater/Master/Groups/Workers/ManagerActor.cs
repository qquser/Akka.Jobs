using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Jobs.Interfaces;
using Akka.Jobs.Models;
using Akka.Jobs.Theater.ActorQueries.Messages.States;
using Akka.Jobs.Theater.Master.Groups.Workers.Messages;
using Akka.Pattern;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Akka.Jobs.Theater.Master.Groups.Workers;

internal sealed class ManagerActor<TIn, TOut> : ReceiveActor
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
    
    private string _jobId;
    private bool _startedFlag;
    private readonly CancellationTokenSource _cancellationTokenSource = new ();
    
    private readonly IServiceScope _scope;
    private ILogger<ManagerActor<TIn, TOut>> _logger;
    
    public ManagerActor(IServiceProvider serviceProvider)
    {
        _scope = serviceProvider.CreateScope();
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
            if (groupActor is LocalActorRef localActorRef && localActorRef.IsTerminated)
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
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            Sender.Tell(new StopJobCommandResult(false, "Cancellation Requested Already."));
            return;
        }
     
        if(_doJobCommand?.IsCreateCommand != true)
            _doJobCommandSender.Tell(new JobDoneCommandResult(false, "Job was cancelled.", _jobId));
        
        _cancellationTokenSource.Cancel();
        Sender.Tell(new StopJobCommandResult(true, "Ok"));
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
                    var text = $"BackoffSupervisor: jobId: {_jobId} " +
                               $"IsCancellationRequested {_cancellationTokenSource.IsCancellationRequested} " +
                               $"_currentNrOfRetries {_currentNrOfRetries} " +
                               $"Message: {exception?.Message} " +
                               $"InnerException: {exception?.InnerException?.Message} ";
                    _logger.LogError(text);
                    
                    if (_cancellationTokenSource.IsCancellationRequested 
                        || exception is TaskCanceledException 
                        || exception?.InnerException is TaskCanceledException
                        || _currentNrOfRetries >= _maxNrOfRetries)
                    {
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
    protected override void PreStart()
    {
        _logger = _scope.ServiceProvider.GetService<ILogger<ManagerActor<TIn, TOut>>>();
    }
    
    protected override void PostStop()
    {
        _scope.Dispose();
        _cancellationTokenSource.Dispose();
    }
    
}