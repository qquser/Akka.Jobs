using Akka.Jobs.Interfaces;

namespace Job.Tests.JobTests;

public class TestForEachJobResult : IJobResult
{
    public TestForEachJobResult(string id, int data )
    {
        Id = id;
        Data = data;
    }

    public string Id { get; set; }
    public int Data { get; set; }
}