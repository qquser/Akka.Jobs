using Akka.Actor;

namespace Job.Core.Theater.Workers.Messages;

public sealed class DoJobCommand
{
    public DoJobCommand(
        object jobInput, 
        Type jobInputType,
        Type jobResultType,
        Guid jobId,
        string groupName)
    {
        JobInput = jobInput;
        JobInputType = jobInputType;
        JobResultType = jobResultType;
        JobId = jobId;
        GroupName = groupName;
    }
    public string GroupName { get; }
    public object JobInput { get; }
    public Type JobInputType { get; }
    public Guid JobId { get; }
    public Type JobResultType { get; }
}

public sealed class WorkerDoJobCommand
{
    public WorkerDoJobCommand(
        object jobInput, 
        Type jobInputType, 
        IActorRef doJobCommandSender, 
        Guid jobId, 
        Type groupType,
        CancellationTokenSource cancellationTokenSource)
    {
        JobId = jobId;
        GroupType = groupType;
        CancellationTokenSource = cancellationTokenSource;
        DoJobCommandSender = doJobCommandSender;
        JobInput = jobInput;
        JobInputType = jobInputType;
    }
    public object JobInput { get; }
    public Type JobInputType { get; }
    public IActorRef DoJobCommandSender { get; }
    public Guid JobId { get; }
    public Type GroupType { get; }
    public CancellationTokenSource CancellationTokenSource { get; }

}