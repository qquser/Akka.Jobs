namespace Job.Core.Theater.Workers.Messages;

public sealed class StopJobCommand
{
    public StopJobCommand(Guid jobId, string groupName)
    {
        JobId = jobId;
        GroupName = groupName;
    }
    public Guid JobId { get; }
    public string GroupName { get; }
}