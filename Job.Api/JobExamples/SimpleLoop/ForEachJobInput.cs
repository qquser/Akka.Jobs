using Akka.Jobs.Interfaces;

namespace Job.Api.JobExamples.SimpleLoop;

public class ForEachJobInput : IJobInput
{
    public int Count { get; set; }
}