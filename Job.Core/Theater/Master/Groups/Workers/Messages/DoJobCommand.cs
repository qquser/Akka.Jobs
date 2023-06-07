using Job.Core.Interfaces;

namespace Job.Core.Theater.Master.Groups.Workers.Messages;

internal sealed class DoJobCommand<TIn> where TIn : IJobInput
{
    public DoJobCommand(
        TIn jobInput,
        Guid jobId,
        string groupName,
        TimeSpan minBackoff,
        TimeSpan maxBackoff,
        int maxNrOfRetries,
        bool isCreateCommand)
    {
        JobInput = jobInput;
        JobId = jobId;
        GroupName = groupName;
        MinBackoff = minBackoff;
        MaxBackoff = maxBackoff;
        MaxNrOfRetries = maxNrOfRetries;
        IsCreateCommand = isCreateCommand;
    }
    public bool IsCreateCommand { get; }
    public string GroupName { get; }
    public TIn JobInput { get; }
    public Guid JobId { get; }
    public TimeSpan MinBackoff { get; }
    public TimeSpan MaxBackoff { get; }
    public int MaxNrOfRetries{ get; }
}