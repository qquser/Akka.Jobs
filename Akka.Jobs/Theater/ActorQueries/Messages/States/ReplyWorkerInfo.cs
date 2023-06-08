using Akka.Jobs.Interfaces;

namespace Akka.Jobs.Theater.ActorQueries.Messages.States;

public sealed class ReplyWorkerInfo<TOut> 
    where TOut : IJobResult
{
    public ReplyWorkerInfo(TOut result)
    {
        Success = true;
        ErrorMessage = "";
        Result = result;
    }
    public ReplyWorkerInfo(bool success, string errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }
    
    public string ErrorMessage { get; }
    public bool Success { get; }
    public TOut? Result { get; }
}