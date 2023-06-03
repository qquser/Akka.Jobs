namespace Job.Core.Theater.Workers.Messages;

public sealed class StopJobCommand
{
    public StopJobCommand(Guid jobId, Type groupType)
    {
        JobId = jobId;
        GroupType = groupType;
    }
    public Guid JobId { get; }
    public Type GroupType { get; }
}