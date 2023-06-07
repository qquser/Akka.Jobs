using Job.Core.Interfaces;

namespace Job.Api.JobExamples.SimpeLoop;

public class ForEachJobInput : IJobInput
{
    public int Count { get; set; }
}