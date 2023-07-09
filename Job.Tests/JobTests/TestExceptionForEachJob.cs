using Akka.Jobs.Interfaces;
// ReSharper disable ClassNeverInstantiated.Global

namespace Job.Tests.JobTests;

public class TestExceptionForEachJob : IJob<TestExceptionForEachJobInput, TestForEachJobResult>
{

    public Task<bool> DoAsync(TestExceptionForEachJobInput input, CancellationToken token)
    {
        throw new Exception();
    }

    public TestForEachJobResult GetCurrentState(string jobId)
    {
        return new TestForEachJobResult(jobId, 0);
    }
}