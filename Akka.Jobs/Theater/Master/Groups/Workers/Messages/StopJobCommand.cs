namespace Akka.Jobs.Theater.Master.Groups.Workers.Messages;

internal sealed class StopJobCommand
{
    public StopJobCommand(string jobId, string groupName)
    {
        JobId = jobId;
        GroupName = groupName;
    }
    public string JobId { get; }
    public string GroupName { get; }
}