namespace Akka.Jobs.Models;

public sealed class JobCreatedCommandResult(bool success, string result, string jobId)
{
    public string JobId { get; set; } = jobId;
    public bool Success { get; set; } = success;
    public string Result { get; set; } = result;
}