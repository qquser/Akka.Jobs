namespace Job.Core.Theater.ActorQueries.Messages;

internal sealed class RequestAllWorkersInfo
{
    public RequestAllWorkersInfo(long requestId, string groupId)
    {
        RequestId = requestId;
        GroupId = groupId;
    }

    public long RequestId { get; }
    public string GroupId { get; }
}