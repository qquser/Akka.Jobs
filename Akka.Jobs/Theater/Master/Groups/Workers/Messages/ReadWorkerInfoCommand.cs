namespace Akka.Jobs.Theater.Master.Groups.Workers.Messages;

internal sealed class ReadWorkerInfoCommand
{
    public ReadWorkerInfoCommand(long requestId, string jobId, string groupId)
    {
        RequestId = requestId;
        JobId = jobId;
        GroupId = groupId;
    }

    public long RequestId { get; }
    public string JobId { get; }
    public string GroupId { get; }
}