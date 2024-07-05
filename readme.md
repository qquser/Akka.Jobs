Job.Library

Nuget - https://www.nuget.org/packages/Job.Library
> dotnet add package Job.Library --version 2.0.0

### Key Features.

This description details a library designed for running background jobs with a strong emphasis on idempotency and preventing duplicate job execution. Here's a detailed breakdown:

• Run Jobs in the Background and Get Its Idemopotent Key JobId:
  * This allows you to initiate a job and receive a unique JobId that acts as an idempotent key. This key guarantees that only one instance of a job with that specific key can exist. 
  * This approach ensures that a task is completed once and prevents duplicate jobs from being created, which is crucial for data integrity and efficiency.
   
• Idemopotent Key Enforcement:
  * If you attempt to create a job with an existing JobId, the system will prevent the creation, informing you that the key already exists. This behavior safeguards against accidental duplicate jobs and ensures that only one instance of a job with a particular key is running.

• Job Management:
  * Get the Current State of a Running Job by Idemopotent Key JobId: You can query the system to check the status of a job using its unique JobId. 
  * Stop a Running Job by JobId: You can terminate a running job using its JobId, enabling control over job execution.
    
• Dependency Injection and Error Handling:
  * The DI scope for the Job: The system supports dependency injection (DI), allowing you to inject required services or dependencies into your background jobs. The DI scope defines the lifetime of these dependencies within the context of the job.
  * Ability to set the number of restarts in case of an error: The system allows configuring the number of automatic retries for failed jobs, enhancing job reliability by handling temporary failures.

Benefits of This Approach:

• Data Integrity: The idempotent key mechanism prevents duplicate jobs from executing, ensuring data consistency and preventing unintended side effects.
• Scalability: By allowing only one job per key, the system can be scaled efficiently, avoiding unnecessary resource consumption.
• Reliability: The combination of idempotency, automatic restarts, and job monitoring contributes to a more reliable background job execution system.
• Simplified Development: The system's features make it easier to develop and manage background jobs by providing a clear structure and mechanisms to handle common scenarios.

### How to use.

In the Program.cs file, register the Job interface and the Job Context for your ...Job class.

```csharp
//Job library registration
builder.Services.AddScoped<IJob<ForEachJobInput, ForEachJobResult>, ForEachJob>();
builder.Services.AddJobContext();
```

Next, for the job class, you need to implement the IJob interface.

```csharp
/// <summary>
/// This IJob interface defines the interface for objects that represent background tasks.
/// These tasks are designed to run asynchronously in the background, without blocking the main application flow.
/// There is no need to create IJob instances manually.
/// This suggests that the library is responsible for creating and managing IJob,
/// providing a mechanism for registering and scheduling IJob tasks.
/// Management through IJobContext:
/// management of background tasks (starting, stopping, and querying their state) should be done exclusively through
/// the IJobContext interface.
/// </summary>
/// <typeparam name="TIn"> Input Parameter Data: This type parameter represents the type of data that will be
/// provided as input to the background task.</typeparam>
/// <typeparam name="TOut"> Current State Data: This type parameter represents the type of data that represents
/// the current state of the task.</typeparam>
public interface IJob<in TIn, out TOut>
    where TIn : IJobInput
    where TOut : IJobResult 
{
    /// <summary>
    /// Method describing the basic logic of the background task.
    /// </summary>
    /// <param name="input">Input data for the background task</param>
    /// <param name="token">The CancellationToken is managed by the library and is disabled when the Stop module is
    /// called from the IJobContext.</param>
    /// <returns>If IJobContext.DoJobAsync is waiting for the entire task to complete using the method,
    /// then the bool result of IJob.DoAsync will be added to JobDoneCommandResult</returns>
    Task<bool> DoAsync(TIn input, CancellationToken token);
    
    /// <summary>
    /// Describes the state of the fields of the IJob class that are changed by the DoAsync method.
    /// </summary>
    TOut GetCurrentState(string jobId);
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
