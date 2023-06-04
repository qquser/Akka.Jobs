using Job.Core.Models;

namespace Job.Core.Interfaces;

public interface IJobContext<in TIn, out TOut> where TOut : IJobResult
{
    /// <summary>
    /// Create a background job
    /// </summary>
    /// <returns>Job Id</returns>
    Guid CreateJob(TIn input, Guid? jobId = null);
    
    /// <summary>
    /// Waiting for a response about the completion of the job
    /// </summary>
    Task<JobCommandResult> DoJobAsync(TIn input, Guid? jobId = null);
    
    Task<StopJobCommandResult> StopJobAsync(Guid jobId);
}