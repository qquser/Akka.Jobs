namespace Job.Core.Models;

public sealed class JobCreatedCommandResult
{
    public JobCreatedCommandResult(bool success, string result, Guid jobId)
    {
        Success = success;
        Result = result;
        JobId = jobId;
    }
    
    public Guid JobId { get; set; }
    public bool Success { get; set; }
    public string Result { get; set; }
}