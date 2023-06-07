using Job.Core.Models;
using Job.Core.Theater.ActorQueries.Messages.States;

namespace Job.Core.Interfaces;

public interface IJobContext<in TIn, TOut>
    where TIn : IJobInput
    where TOut : IJobResult
{
    /// <summary>
    /// Create a background job
    /// </summary>
    /// <returns>Job Id</returns>
    Task<JobCreatedCommandResult> CreateJobAsync(TIn input,
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null, TimeSpan? timeout = null);
    
    /// <summary>
    /// Waiting for a response about the completion of the job
    /// </summary>
    Task<JobDoneCommandResult> DoJobAsync(TIn input,
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null, TimeSpan? timeout = null);
    
    Task<StopJobCommandResult> StopJobAsync(Guid jobId, TimeSpan? timeout = null);
    
    Task<IDictionary<Guid, ReplyWorkerInfo<TOut>>> GetAllJobsCurrentStatesAsync(long requestId, TimeSpan? timeout = null);
}