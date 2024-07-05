using Akka.Jobs.Models;
using Akka.Jobs.Theater.ActorQueries.Messages.States;

namespace Akka.Jobs.Interfaces;

/// <summary>
/// Interface for accessing tasks that are described by the IJob&lt;TIn, Tout&gt; type.
/// The interface IJobContext does not require an implementation,
/// and it is registered by calling builder.Services.AddJobContext().
/// A specific task type IJob&lt;TIn, Tout&gt; is accessed through a unique combination of input and
/// output generalized parameters TIn TOut. 
///The job must be registered in DI as in the example below:
///builder.Services.AddScoped&lt;IJob&lt;ForEachJobInput, ForEachJobResult&gt;, ForEachJob&gt;();
/// </summary>
/// <typeparam name="TIn"> Input Parameter Data: This type parameter represents the type of data that will be
/// provided as input to the background task.
/// </typeparam>
/// <typeparam name="TOut"> Current State Data: This type parameter represents the type of data that represents
/// the current state of the task.
/// </typeparam>
public interface IJobContext<in TIn, TOut>
    where TIn : IJobInput
    where TOut : IJobResult
{
    /// <summary>
    /// Create a background job
    /// </summary>
    /// <returns>Job Id</returns>
    Task<JobCreatedCommandResult> CreateJobAsync(TIn input,
        int? maxNrOfRetries = null,
        TimeSpan? minBackoff = null,
        TimeSpan? maxBackoff = null,
        string? jobId = null,
        TimeSpan? timeout = null);
    
    /// <summary>
    /// Waiting for a response about the completion of the job
    /// </summary>
    Task<JobDoneCommandResult> DoJobAsync(TIn input,
        int? maxNrOfRetries = null,
        TimeSpan? minBackoff = null,
        TimeSpan? maxBackoff = null,
        string? jobId = null,
        TimeSpan? timeout = null);
    
    Task<StopJobCommandResult> StopJobAsync(string jobId, TimeSpan? timeout = null);
    
    Task<IDictionary<string, ReplyWorkerInfo<TOut>>> GetAllJobsCurrentStatesAsync(long requestId,
        TimeSpan? timeout = null);
}