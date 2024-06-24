using Akka.Jobs.Interfaces;

namespace Job.Api.JobExamples.SimpleLoop;

public class ForEachJob(ILogger<ForEachJob> logger) : IJob<ForEachJobInput, ForEachJobResult>, IDisposable
{
    private int _currentState;

    public async Task<bool> DoAsync(ForEachJobInput input, CancellationToken token)
    {
        foreach (var item in Enumerable.Range(0, input.Count))
        {
            if (token.IsCancellationRequested)
                return false;
            _currentState = item;
            logger.LogInformation(item.ToString());
            await Task.Delay(1000, token);
        }

        return true;
    }

    public ForEachJobResult GetCurrentState(string jobId)
    {
        return new ForEachJobResult
        {
            Id = jobId,
            Data = _currentState
        };
    }

    public void Dispose()
    {
        logger.LogInformation("Dispose.");
    }
}