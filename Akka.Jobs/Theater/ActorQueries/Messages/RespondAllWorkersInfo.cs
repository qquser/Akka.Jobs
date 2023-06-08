using Akka.Jobs.Interfaces;
using Akka.Jobs.Theater.ActorQueries.Messages.States;

namespace Akka.Jobs.Theater.ActorQueries.Messages;

internal sealed class RespondAllWorkersInfo<TOut> 
    where TOut : IJobResult
{
    public RespondAllWorkersInfo(long requestId)
    {
        RequestId = requestId;
        WorkersData = new Dictionary<Guid, ReplyWorkerInfo<TOut>>();
    }
    
    public RespondAllWorkersInfo(long requestId, Dictionary<Guid, ReplyWorkerInfo<TOut>> workersData)
    {
        RequestId = requestId;
        WorkersData = workersData;
    }

    public long RequestId { get; }
    public Dictionary<Guid, ReplyWorkerInfo<TOut>> WorkersData { get; }
}