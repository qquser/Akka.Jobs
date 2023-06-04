namespace Job.Core.Theater.Workers.Messages;

internal sealed class DoJobCommand
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