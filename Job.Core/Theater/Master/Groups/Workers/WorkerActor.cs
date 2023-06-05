using Akka.Actor;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Theater.ActorQueries.Messages.States;
using Job.Core.Theater.Master.Groups.Workers.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Job.Core.Theater.Master.Groups.Workers;

internal class WorkerActor<TIn, TOut> : ReceiveActor
    where TIn : IJobInput
    where TOut : IJobResult
{
    private Guid _jobId;

    private readonly IServiceScope _scope;
    
    private IJob<TIn, TOut> _job;

    public WorkerActor(IServiceProvider serviceProvider)
    {
        _scope = serviceProvider.CreateScope();
 
        Receive<Status.Failure>(Failed);
        Receive<WorkerDoJobCommand<TIn>>((msg) =>
        {
            DoJobCommandHandlerAsync(msg).PipeTo(Self);
        });
        
        Receive<ReadWorkerInfoCommand>(ReadWorkerInfoCommandHandler);
        
        Context.Parent.Tell(new GiveMeWorkerDoJobCommand());
    }
    
    private void Failed(Status.Failure msg)
    {
        throw msg?.Cause ?? throw new Exception("Unknown error, msg?.Cause == null");
    }
    
    private void ReadWorkerInfoCommandHandler(ReadWorkerInfoCommand command)
    {
        var currentState = _job.GetCurrentState(_jobId);
        var result = new RespondWorkerInfo<TOut>(command.RequestId, currentState);
        Sender.Tell(result);
    }
    
    private async Task DoJobCommandHandlerAsync(WorkerDoJobCommand<TIn> command)
    {
        _jobId = command.JobId;
        Context.Parent.Tell(new TrySaveWorkerActorRefCommand(Self, _jobId, command.DoJobCommandSender));
        
        var token = command.CancellationTokenSource.Token;
        var jobResult = await _job.DoAsync(command.JobInput, token);

        command.DoJobCommandSender.Tell(token.IsCancellationRequested
            ? new JobCommandResult(false, "Job was cancelled.", command.JobId)
            : new JobCommandResult(jobResult, "Ok", command.JobId));
        
        Self.Tell(PoisonPill.Instance);
    }

    protected override void PreStart()
    {
        _job = _scope.ServiceProvider.GetService<IJob<TIn, TOut>>();
    }
    
    protected override void PostStop()
    {
        _scope.Dispose();
    }
}