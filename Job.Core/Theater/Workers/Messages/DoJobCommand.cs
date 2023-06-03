using Akka.Actor;

namespace Job.Core.Theater.Workers.Messages;

public sealed class DoJobCommand
{
    public DoJobCommand(Guid jobId, Type groupType)
    {
        JobId = jobId;
        GroupType = groupType;
    }
    public Guid JobId { get; }
    public Type GroupType { get; }
}

public sealed class WorkerDoJobCommand
{
    public WorkerDoJobCommand(IActorRef doJobCommandSender, Guid jobId, Type groupType, CancellationTokenSource cancellationTokenSource)
    {
        JobId = jobId;
        GroupType = groupType;
        CancellationTokenSource = cancellationTokenSource;
        DoJobCommandSender = doJobCommandSender;
    }
    public IActorRef DoJobCommandSender { get; }
    public Guid JobId { get; }
    public Type GroupType { get; }
    public CancellationTokenSource CancellationTokenSource { get; }
}