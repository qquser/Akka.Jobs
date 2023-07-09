using Akka.Jobs.Interfaces;

namespace Job.Tests.JobTests;

public class TestForEachJobInput : IJobInput
{
    public int Count { get; set; }
}