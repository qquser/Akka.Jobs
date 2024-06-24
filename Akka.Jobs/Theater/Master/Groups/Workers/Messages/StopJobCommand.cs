namespace Akka.Jobs.Theater.Master.Groups.Workers.Messages;

internal sealed class StopJobCommand(string jobId, string groupName)
{
    public string JobId { get; } = jobId;
    public string GroupName { get; } = groupName;
}