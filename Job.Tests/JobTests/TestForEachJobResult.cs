using Akka.Jobs.Interfaces;

namespace Job.Tests.JobTests;

public class TestForEachJobResult(string id, int data) : IJobResult
{
    public string Id { get; set; } = id;
    public int Data { get; set; } = data;
}