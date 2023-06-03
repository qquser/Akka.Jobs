namespace Job.Core.Models;

public sealed class StartJobCommandResult
{
    public StartJobCommandResult(bool success, string result)
    {
        Success = success;
        Result = result;
    }
    
    public StartJobCommandResult()
    {
        Success = true;
        Result = "";
    }
    
    public bool Success { get; set; }
    public string Result { get; set; }
}