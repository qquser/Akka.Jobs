using Akka.Jobs.Interfaces;

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

    public TestForEachJobResult GetCurrentState(Guid jobId)
    {
        return new TestForEachJobResult
        {
            Id = jobId,
            Data = _currentState
        };
    }
}

public class TestForEachJobInput : IJobInput
{
    public int Count { get; set; }
}

public class TestForEachJobResult : IJobResult
{
    public Guid Id { get; set; }
    public int Data { get; set; }
}