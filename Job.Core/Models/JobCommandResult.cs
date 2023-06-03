namespace Job.Core.Models;

public sealed class JobCommandResult
{
    public JobCommandResult(bool success, string result, Guid jobId)
    {
        Success = success;
        Result = result;
    }
    
    public JobCommandResult()
    {
        Success = true;
        Result = "";
    }
    
    public bool Success { get; set; }
    public string Result { get; set; }
}