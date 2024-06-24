using Akka.Jobs.Interfaces;
using Akka.Jobs.Theater.ActorQueries.Messages.States;

namespace Akka.Jobs.Theater.ActorQueries.Messages;

internal sealed class RespondAllWorkersInfo<TOut>(long requestId, Dictionary<string, ReplyWorkerInfo<TOut>> workersData)
    where TOut : IJobResult
{
    public RespondAllWorkersInfo(long requestId) : this(requestId, new Dictionary<string, ReplyWorkerInfo<TOut>>())
    {
    }

    public long RequestId { get; } = requestId;
    public Dictionary<string, ReplyWorkerInfo<TOut>> WorkersData { get; } = workersData;
}