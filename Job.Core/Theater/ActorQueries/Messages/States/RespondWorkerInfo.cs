using Job.Core.Interfaces;

namespace Job.Core.Theater.ActorQueries.Messages.States;

internal sealed class RespondWorkerInfo<TOut> 
    where TOut : IJobResult
{
    public RespondWorkerInfo(bool success, string errorMessage, TOut result)
    {
        Success = success;
        ErrorMessage = errorMessage;
        Result = result;
    }
    public RespondWorkerInfo(bool success, string errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }
    
    public string ErrorMessage { get; }
    public bool Success { get; }
    public TOut? Result { get; }
}