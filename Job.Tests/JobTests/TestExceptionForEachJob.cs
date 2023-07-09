using Akka.Jobs.Interfaces;

namespace Job.Tests.JobTests;

public class TestExceptionForEachJob : IJob<TestExceptionForEachJobInput, TestForEachJobResult>
{
    private int _currentState;
    
    public Task<bool> DoAsync(TestExceptionForEachJobInput input, CancellationToken token)
    {
        throw new Exception();
    }

    public TestForEachJobResult GetCurrentState(string jobId)
    {
        return new TestForEachJobResult(jobId, _currentState);
    }
}