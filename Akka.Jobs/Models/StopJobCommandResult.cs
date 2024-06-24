namespace Akka.Jobs.Models;

public sealed class StopJobCommandResult(bool success, string result)
{
    public StopJobCommandResult() : this(true, "")
    {
    }
    
    public bool Success { get; set; } = success;
    public string Result { get; set; } = result;
}