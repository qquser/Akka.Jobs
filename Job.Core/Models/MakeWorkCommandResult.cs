namespace Job.Core.Models;

public sealed class MakeWorkCommandResult
{
    public MakeWorkCommandResult(bool success, string result)
    {
        Success = success;
        Result = result;
    }
    
    public MakeWorkCommandResult()
    {
        Success = true;
        Result = "";
    }
    
    public bool Success { get; set; }
    public string Result { get; set; }
}