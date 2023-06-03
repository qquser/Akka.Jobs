namespace Job.Core.Models;

public sealed class StopJobCommandResult
{
    public StopJobCommandResult(bool success, string result)
    {
        Success = success;
        Result = result;
    }
    
    public StopJobCommandResult()
    {
        Success = true;
        Result = "";
    }
    
    public bool Success { get; set; }
    public string Result { get; set; }
}