namespace Job.Core.Theater.Master.Groups.Workers.Messages;

internal sealed class DoJobCommand
{
    public DoJobCommand(
        object jobInput, 
        Type jobInputType,
        Type jobResultType,
        Guid jobId,
        string groupName,
        TimeSpan minBackoff,
        TimeSpan maxBackoff,
        int maxNrOfRetries)
    {
        JobInput = jobInput;
        JobInputType = jobInputType;
        JobResultType = jobResultType;
        JobId = jobId;
        GroupName = groupName;
        MinBackoff = minBackoff;
        MaxBackoff = maxBackoff;
        MaxNrOfRetries = maxNrOfRetries;
    }
    public string GroupName { get; }
    public object JobInput { get; }
    public Type JobInputType { get; }
    public Guid JobId { get; }
    public Type JobResultType { get; }
    public TimeSpan MinBackoff { get; }
    public TimeSpan MaxBackoff { get; }
    public int MaxNrOfRetries{ get; }
}