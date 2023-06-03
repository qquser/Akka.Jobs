namespace Job.Core.Theater.Workers.Messages;

public sealed class MakeWorkCommand
{
    public Guid JobId { get; set; }
    public string GroupId { get; set; }
}