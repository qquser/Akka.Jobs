namespace Akka.Jobs.Models;

public sealed class JobDoneCommandResult
{
    public JobDoneCommandResult(bool success, string result, string jobId)
    {
        Success = success;
        Result = result;
        JobId = jobId;
    }
    
    public string JobId { get; set; }
    public bool Success { get; set; }
    public string Result { get; set; }
}