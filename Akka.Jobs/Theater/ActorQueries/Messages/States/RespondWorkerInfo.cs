using Akka.Jobs.Interfaces;

namespace Akka.Jobs.Theater.ActorQueries.Messages.States;

internal sealed class RespondWorkerInfo<TOut> 
    where TOut : IJobResult
{
    public RespondWorkerInfo(long requestId,  TOut result)
    {
        RequestId = requestId;
        Success = true;
        RequestId = requestId;
        ErrorMessage = "";
        Result = result;
    }
    public RespondWorkerInfo(bool success, long requestId, string errorMessage)
    {
        RequestId = requestId;
        Success = success;
        ErrorMessage = errorMessage;
    }
    public long RequestId { get; }
    public string ErrorMessage { get; }
    public bool Success { get; }
    public TOut? Result { get; }
}