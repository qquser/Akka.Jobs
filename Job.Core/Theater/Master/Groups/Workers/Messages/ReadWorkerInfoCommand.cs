namespace Job.Core.Theater.Master.Groups.Workers.Messages;

internal sealed class ReadWorkerInfoCommand
{
    public ReadWorkerInfoCommand(long requestId, Guid jobId, string groupId)
    {
        RequestId = requestId;
        JobId = jobId;
        GroupId = groupId;
    }

    public long RequestId { get; }
    public Guid JobId { get; }
    public string GroupId { get; }
}