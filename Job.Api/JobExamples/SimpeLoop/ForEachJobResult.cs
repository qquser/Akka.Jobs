using Job.Core.Interfaces;

namespace Job.Api.JobExamples.SimpeLoop;

public class ForEachJobResult : IJobResult
{
    public Guid Id { get; set; }
    public int Data { get; set; }
}