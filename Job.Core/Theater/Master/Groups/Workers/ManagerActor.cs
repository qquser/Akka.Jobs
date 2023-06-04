using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Pattern;
using Job.Core.Models;
using Job.Core.Theater.Master.Groups.Workers.Messages;

namespace Job.Core.Theater.Master.Groups.Workers;

internal class ManagerActor : ReceiveActor
{
    private int _currentNrOfRetries;
    private int _maxNrOfRetries;
    private TimeSpan _minBackoff;
    private TimeSpan _maxBackoff;
    private IActorRef? _doJobCommandSender;
    private IActorRef? _workerSupervisorActor;
    private WorkerDoJobCommand? _doJobCommand;
    
    private readonly Guid _jobId;
    private readonly CancellationTokenSource _cancellationTokenSource = new ();
    
    public ManagerActor(Guid jobId)
    {
        _jobId = jobId;
        
        Receive<DoJobCommand>(StartJobCommandHandler);
        Receive<StopJobCommand>(StopJobCommandHandler);
        
        Receive<GiveMeWorkerDoJobCommand>(GiveMeWorkerDoJobCommandHandler);
        
        Receive<Terminated>(WorkerActorTerminatedHandler);
    }

    private void GiveMeWorkerDoJobCommandHandler(GiveMeWorkerDoJobCommand _)
    {
        _workerSupervisorActor.Tell(_doJobCommand);
    }

    private void StopJobCommandHandler(StopJobCommand _)
    {
        _cancellationTokenSource.Cancel();
        Sender.Tell(new StopJobCommandResult(true, "Ok"));
        //Self.Tell(PoisonPill.Instance) is not called here
            //because the death of the actor does not stop the thread _job.DoJob
    }
    
    private void WorkerActorTerminatedHandler(Terminated t)
    {
        Self.Tell(PoisonPill.Instance);
    }

    private void StartJobCommandHandler(DoJobCommand doJobCommand)
    {
        if (doJobCommand.JobId != _jobId || _workerSupervisorActor != null)
        {
            Sender.Tell(new JobCommandResult(false, "Ignoring Create Worker Actor", doJobCommand.JobId));
            return;
        }

        _maxNrOfRetries = doJobCommand.MaxNrOfRetries;
        _minBackoff = doJobCommand.MinBackoff;
        _maxBackoff = doJobCommand.MaxBackoff;
        _doJobCommandSender = Sender;

        var dependencyResolver = DependencyResolver.For(Context.System);
        var workerActorType = typeof(WorkerActor<,>)
            .MakeGenericType(doJobCommand.JobInputType, doJobCommand.JobResultType);
        var workerActorProps = dependencyResolver
            .Props(workerActorType);
        
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
                    if (_currentNrOfRetries < _maxNrOfRetries)
                    {
                        _currentNrOfRetries += 1;
                        return Directive.Restart;
                    }

                    var text = $"BackoffSupervisor: jobId: {_jobId}" +
                               $" {exception?.Message}" +
                               $" InnerException: {exception?.InnerException?.Message}";
                    _doJobCommandSender.Tell(new JobCommandResult(false, text, _jobId));
                    return Directive.Stop;
                })));
        _workerSupervisorActor = Context
            .ActorOf(supervisorOfWorkerActorProps, $"supervisor-of-worker-{doJobCommand.JobId}");
  
        Context.Watch(_workerSupervisorActor);
        
        _doJobCommand = new WorkerDoJobCommand(
            doJobCommand.JobInput,
            doJobCommand.JobInputType,
            _doJobCommandSender, 
            doJobCommand.JobId, 
            doJobCommand.JobResultType,
            _cancellationTokenSource);
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
    
    public static Props Props(Guid jobId) =>
        Akka.Actor.Props.Create(() => new ManagerActor(jobId));
    
}