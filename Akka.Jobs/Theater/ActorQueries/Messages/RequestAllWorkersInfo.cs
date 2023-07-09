namespace Akka.Jobs.Theater.ActorQueries.Messages;

internal sealed class RequestAllWorkersInfo
{
    public RequestAllWorkersInfo(long requestId, string groupId, TimeSpan timeout)
    {
        RequestId = requestId;
        GroupId = groupId;
        Timeout = timeout;
    }

    public long RequestId { get; }
    public string GroupId { get; }
    public TimeSpan Timeout { get; }
}