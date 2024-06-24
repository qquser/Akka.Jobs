using Akka.Jobs.Interfaces;

namespace Akka.Jobs.Theater.ActorQueries.Messages.States;

public sealed class ReplyWorkerInfo<TOut>(bool success, string errorMessage)
    where TOut : IJobResult
{
    public ReplyWorkerInfo(TOut result) : this(true, "")
    {
        Result = result;
    }

    public string ErrorMessage { get; } = errorMessage;
    public bool Success { get; } = success;
    public TOut? Result { get; }
}