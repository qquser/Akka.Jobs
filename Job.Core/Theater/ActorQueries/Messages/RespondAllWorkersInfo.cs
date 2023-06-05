using Job.Core.Interfaces;
using Job.Core.Theater.ActorQueries.Messages.States;

namespace Job.Core.Theater.ActorQueries.Messages;

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