namespace Job.Core.Theater.Workers.Messages;

public sealed class DoJobCommand
{
    public Guid JobId { get; set; }
    public Type GroupType { get; set; }
}