using Akka.Jobs.Interfaces;

namespace Job.Tests.JobTests;

public class TestExceptionForEachJobInput : IJobInput
{
    public int Count { get; set; }
}