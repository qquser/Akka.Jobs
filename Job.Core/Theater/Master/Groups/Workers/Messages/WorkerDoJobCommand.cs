using Akka.Actor;
using Job.Core.Interfaces;

namespace Job.Core.Theater.Master.Groups.Workers.Messages;

internal sealed class WorkerDoJobCommand<TIn> 
    where  TIn : IJobInput
{
    public WorkerDoJobCommand(
        TIn jobInput,
        IActorRef doJobCommandSender, 
        Guid jobId,
        CancellationTokenSource cancellationTokenSource,
        bool isCreateCommand)
    {
        IsCreateCommand = isCreateCommand;
        JobId = jobId;
        CancellationTokenSource = cancellationTokenSource;
        DoJobCommandSender = doJobCommandSender;
        JobInput = jobInput;
    }
    public bool IsCreateCommand { get; }
    public TIn JobInput { get; }
    public IActorRef DoJobCommandSender { get; }
    public Guid JobId { get; }
    public CancellationTokenSource CancellationTokenSource { get; }

}