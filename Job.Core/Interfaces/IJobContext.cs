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
    Guid CreateJob(TIn input, 
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null);
    
    /// <summary>
    /// Waiting for a response about the completion of the job
    /// </summary>
    Task<JobCommandResult> DoJobAsync(TIn input,
        int? maxNrOfRetries = null, TimeSpan? minBackoff = null, TimeSpan? maxBackoff = null,  Guid? jobId = null);
    
    Task<StopJobCommandResult> StopJobAsync(Guid jobId);
    
    Task<IDictionary<Guid, ReplyWorkerInfo<TOut>>> GetAllWorkersCurrentStateAsync(long requestId);
}