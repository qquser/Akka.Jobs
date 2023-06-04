using Akka.Actor;

namespace Job.Core.Theater.Master.Groups.Workers.Messages;

internal sealed class WorkerDoJobCommand
{
    public WorkerDoJobCommand(
        object jobInput, 
        Type jobInputType, 
        IActorRef doJobCommandSender, 
        Guid jobId, 
        Type jobResultType,
        CancellationTokenSource cancellationTokenSource)
    {
        JobId = jobId;
        JobResultType = jobResultType;
        CancellationTokenSource = cancellationTokenSource;
        DoJobCommandSender = doJobCommandSender;
        JobInput = jobInput;
        JobInputType = jobInputType;
    }
    public object JobInput { get; }
    public Type JobInputType { get; }
    public IActorRef DoJobCommandSender { get; }
    public Guid JobId { get; }
    public Type JobResultType { get; }
    public CancellationTokenSource CancellationTokenSource { get; }

}