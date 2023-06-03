using Job.Core.Models;

namespace Job.Core.Interfaces;

public interface IJobContext
{
    /// <summary>
    /// Create a background task
    /// </summary>
    /// <returns>Job Id</returns>
    Guid CreateJob();
    
    /// <summary>
    /// Waiting for a response about the completion of the task
    /// </summary>
    Task<MakeWorkCommandResult> MakeWorkAsync();
    
    Task<StopJobCommandResult> StopJobAsync(Guid jobId);
}