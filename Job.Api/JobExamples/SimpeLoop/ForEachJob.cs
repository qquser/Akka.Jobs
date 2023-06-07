using Job.Core.Interfaces;

namespace Job.Api.JobExamples.SimpeLoop;

public class ForEachJob : IJob<ForEachJobInput, ForEachJobResult>, IDisposable
{
    private int _currentState;

    private readonly ILogger<ForEachJob> _logger;
    public ForEachJob(ILogger<ForEachJob> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> DoAsync(ForEachJobInput input, CancellationToken token)
    {
        foreach (var item in Enumerable.Range(0, input.Count))
        {
            if (token.IsCancellationRequested)
                return false;
            _currentState = item;
            _logger.LogInformation(item.ToString());
            await Task.Delay(1000, token);
        }

        return true;
    }

    public ForEachJobResult GetCurrentState(Guid jobId)
    {
        return new ForEachJobResult
        {
            Id = jobId,
            Data = _currentState
        };
    }

    public void Dispose()
    {
        _logger.LogInformation("Dispose.");
    }
}