using Akka.Actor;
using Job.Core.Interfaces;
using Job.Core.Models;
using Job.Core.Theater.Workers.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Job.Core.Theater.Workers;

internal class WorkerActor<TData> : ReceiveActor 
    where TData : IJobData
{
    private Guid _actorId;
    private Type _groupId;
    
    private readonly IServiceScope _scope;
    
    private IJob<TData> _job;

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
        
        var jobResult = await _job.DoJobAsync(_cancelTokenSource.Token);

        command.DoJobCommandSender.Tell(_cancelTokenSource.Token.IsCancellationRequested
            ? new JobCommandResult(false, "Job was cancelled.", command.JobId)
            : new JobCommandResult(jobResult, "Ok", command.JobId));
        
        Self.Tell(PoisonPill.Instance);
    }

    protected override void PreStart()
    {
        _job = _scope.ServiceProvider.GetService<IJob<TData>>();
    }
    
    protected override void PostStop()
    {
        _scope.Dispose();
        _cancelTokenSource?.Dispose();
    }
}