using Akka.Jobs.Interfaces;

namespace Akka.Jobs.Theater.Master.Groups.Workers.Messages;

internal sealed class DoJobCommand<TIn>(
    TIn jobInput,
    string jobId,
    string groupName,
    TimeSpan minBackoff,
    TimeSpan maxBackoff,
    int maxNrOfRetries,
    bool isCreateCommand)
    where TIn : IJobInput
{
    public bool IsCreateCommand { get; } = isCreateCommand;
    public string GroupName { get; } = groupName;
    public TIn JobInput { get; } = jobInput;
    public string JobId { get; } = jobId;
    public TimeSpan MinBackoff { get; } = minBackoff;
    public TimeSpan MaxBackoff { get; } = maxBackoff;
    public int MaxNrOfRetries{ get; } = maxNrOfRetries;
}