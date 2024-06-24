using Akka.Actor;
using Akka.Jobs.Interfaces;

namespace Akka.Jobs.Theater.Master.Groups.Workers.Messages;

internal sealed class WorkerDoJobCommand<TIn>(
    TIn jobInput,
    IActorRef doJobCommandSender,
    string jobId,
    CancellationTokenSource cancellationTokenSource,
    bool isCreateCommand)
    where TIn : IJobInput
{
    public bool IsCreateCommand { get; } = isCreateCommand;
    public TIn JobInput { get; } = jobInput;
    public IActorRef DoJobCommandSender { get; } = doJobCommandSender;
    public string JobId { get; } = jobId;
    public CancellationTokenSource CancellationTokenSource { get; } = cancellationTokenSource;
}