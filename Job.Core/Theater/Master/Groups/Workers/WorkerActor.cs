using Akka.Actor;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Theater.Master.Groups.Workers.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Job.Core.Theater.Master.Groups.Workers;

internal class WorkerActor<TIn, TOut> : ReceiveActor
    where TIn : IJobInput
    where TOut : IJobResult
{
    private Guid _actorId;
    private Type _groupId;
    
    private readonly IServiceScope _scope;
    
    private IJob<TIn, TOut> _job;

    private CancellationTokenSource _cancelTokenSource;
    
    public WorkerActor(IServiceProvider serviceProvider)
    {
        _scope = serviceProvider.CreateScope();
 
        Receive<Status.Failure>(Failed);
        Receive<WorkerDoJobCommand>((msg) =>
        {
            DoJobCommandHandlerAsync(msg).PipeTo(Self);
        });
    }

    private void Failed(Status.Failure msg)
    {
        throw msg?.Cause ?? throw new Exception("Unknown error, msg?.Cause == null");
    }
    
    private async Task DoJobCommandHandlerAsync(WorkerDoJobCommand command)
    {
        _actorId = command.JobId;
        _groupId = command.GroupType;
        _cancelTokenSource = command.CancellationTokenSource;
        
        var jobResult = await _job.DoAsync((TIn)command.JobInput, _cancelTokenSource.Token);

        command.DoJobCommandSender.Tell(_cancelTokenSource.Token.IsCancellationRequested
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
        _cancelTokenSource?.Dispose();
    }
}