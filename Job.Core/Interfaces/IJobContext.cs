using Job.Core.Models;

namespace Job.Core.Interfaces;

public interface IJobContext<TData> where TData : IJobData
{
    /// <summary>
    /// Create a background job
    /// </summary>
    /// <returns>Job Id</returns>
    Guid CreateJob();
    
    /// <summary>
    /// Waiting for a response about the completion of the job
    /// </summary>
    Task<JobCommandResult> DoJobAsync();
    
    Task<StopJobCommandResult> StopJobAsync(Guid jobId);
}