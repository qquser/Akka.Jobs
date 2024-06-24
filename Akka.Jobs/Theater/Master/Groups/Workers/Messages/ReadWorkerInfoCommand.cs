namespace Akka.Jobs.Theater.Master.Groups.Workers.Messages;

internal sealed class ReadWorkerInfoCommand(long requestId, string jobId, string groupId)
{
    public long RequestId { get; } = requestId;
    public string JobId { get; } = jobId;
    public string GroupId { get; } = groupId;
}