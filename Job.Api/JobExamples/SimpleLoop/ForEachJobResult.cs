using Akka.Jobs.Interfaces;

namespace Job.Api.JobExamples.SimpleLoop;

public class ForEachJobResult : IJobResult
{
    public string Id { get; set; }
    public int Data { get; set; }
}