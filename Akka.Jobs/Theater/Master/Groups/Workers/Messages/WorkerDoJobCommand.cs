using Akka.Actor;
using Akka.Jobs.Interfaces;

namespace Akka.Jobs.Theater.Master.Groups.Workers.Messages;

internal sealed class WorkerDoJobCommand<TIn> 
    where  TIn : IJobInput
{
    public WorkerDoJobCommand(
        TIn jobInput,
        IActorRef doJobCommandSender, 
        string jobId,
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
    public string JobId { get; }
    public CancellationTokenSource CancellationTokenSource { get; }

}