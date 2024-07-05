Job.Library

Nuget - https://www.nuget.org/packages/Job.Library
> dotnet add package Job.Library --version 2.0.0




### How to use.

Runs background Jobs.

- Run the Jobs in the background and get its ID.
- Get the current state of a running Job by JobId.
- Stop a running Job.
- The DI scope for the Job.
- Has the ability to set the number of restarts in case of an error.

In the Program.cs file, register the Job interface and the Job Context for your ...Job class.

```csharp
//Job library registration
builder.Services.AddScoped<IJob<ForEachJobInput, ForEachJobResult>, ForEachJob>();
builder.Services.AddJobContext();
```

Next, for the job class, you need to implement the IJob interface.

```csharp
public interface IJob<in TIn, out TOut>
    where TIn : IJobInput
    where TOut : IJobResult 
{
    Task<bool> DoAsync(TIn input, CancellationToken token);
    TOut GetCurrentState(Guid jobId);
}
```

For example, the implementation of IJob for ForEachJob, which simply iterates through the values from 0 to Count in a loop.

```csharp
public class ForEachJob : IJob<ForEachJobInput, ForEachJobResult>
{
    private int _currentState;

    private readonly ILogger<ForEachJob> _logger;
    //It is possible to inject any registered interface
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

    //Describes the state of the fields of the IJob class that are changed by the DoAsync method.
    public ForEachJobResult GetCurrentState(Guid jobId)
    {
        return new ForEachJobResult
        {
            Id = jobId,
            Data = _currentState
        };
    }
}

public class ForEachJobInput : IJobInput
{
    public int Count { get; set; }
}

public class ForEachJobResult : IJobResult
{
    public Guid Id { get; set; }
    public int Data { get; set; }
}
```

Next, IJobContext is injected for the corresponding input and output parameters of the job.

```csharp
public interface IJobContext<in TIn, TOut>
    where TIn : IJobInput
    where TOut : IJobResult
{
    /// <summary>
    /// Create a background job
    /// </summary>
    /// <returns>Job Id</returns>
    Guid CreateJob(TIn input, 
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null, TimeSpan? timeout = null);
    
    /// <summary>
    /// Waiting for a response about the completion of the job
    /// </summary>
    Task<JobCommandResult> DoJobAsync(TIn input,
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null, TimeSpan? timeout = null);
    
    Task<StopJobCommandResult> StopJobAsync(Guid jobId, TimeSpan? timeout = null);
    
    Task<IDictionary<Guid, ReplyWorkerInfo<TOut>>> GetAllJobsCurrentStatesAsync(long requestId, TimeSpan? timeout = null);
}
```

Controller

```csharp
    private readonly IJobContext<ForEachJobInput, ForEachJobResult> _jobContext;
    
    public ForEachJobController(IJobContext<ForEachJobInput, ForEachJobResult> jobContext)
    {
        _jobContext = jobContext;
    }
    
    [HttpPost]
    [Route(nameof(CreateJob))]
    public Guid CreateJob([FromBody] ForEachJobInput input)
    {
        return _jobContext.CreateJob(input); //Immediately returns the ID of a background task
    }
    
    [HttpGet]
    [Route(nameof(GetAllJobs))]
    public async Task<ICollection<ForEachJobResult?>> GetAllJobs([FromQuery] int requestId)
    {
        var result = await _jobContext
            .GetAllJobsCurrentStatesAsync(requestId);
        return result.Values.Select(x => x.Result).ToList();
    }
````
