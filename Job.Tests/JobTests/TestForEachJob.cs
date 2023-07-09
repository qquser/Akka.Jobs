using Akka.Jobs.Interfaces;
// ReSharper disable ClassNeverInstantiated.Global

namespace Job.Tests.JobTests;

public class TestForEachJob : IJob<TestForEachJobInput, TestForEachJobResult>
{
    private int _currentState;
    
    public async Task<bool> DoAsync(TestForEachJobInput input, CancellationToken token)
    {
        foreach (var item in Enumerable.Range(0, input.Count))
        {
            if (token.IsCancellationRequested)
                return false;
            _currentState = item;
            await Task.Delay(1000, token);
        }

        return true;
    }

    public TestForEachJobResult GetCurrentState(string jobId)
    {
        return new TestForEachJobResult(jobId, _currentState);
    }
}