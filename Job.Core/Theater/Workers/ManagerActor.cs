using Akka.Actor;
using Akka.DependencyInjection;
using Job.Core.Models;
using Job.Core.Theater.Workers.Messages;

namespace Job.Core.Theater.Workers;

internal class ManagerActor : ReceiveActor
{
    private IActorRef? _workerActor;
    private readonly Guid _jobId;
    private readonly CancellationTokenSource _cancellationTokenSource = new ();
    
    public ManagerActor(Guid jobId)
    {
        _jobId = jobId;
        
        Receive<DoJobCommand>(StartJobCommandHandler);
        Receive<StopJobCommand>(StopJobCommandHandler);

        Receive<Terminated>(WorkerActorTerminatedHandler);
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
        if (doJobCommand.JobId != _jobId || _workerActor != null)
        {
            Sender.Tell(new JobCommandResult(false, "Ignoring Create Worker Actor", doJobCommand.JobId));
            return;
        }

        var dependencyResolver = DependencyResolver.For(Context.System);
        var workerActorType = typeof(WorkerActor<,>)
            .MakeGenericType(doJobCommand.JobInputType, doJobCommand.JobResultType);
        var workerActorProps = dependencyResolver
            .Props(workerActorType);
        
        _workerActor = Context.ActorOf(workerActorProps, $"worker-{doJobCommand.JobId}");
  
        Context.Watch(_workerActor);
        
        _workerActor.Forward(new WorkerDoJobCommand(
            doJobCommand.JobInput,
            doJobCommand.JobInputType,
            Sender, 
            doJobCommand.JobId, 
            doJobCommand.JobResultType,
            _cancellationTokenSource));
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