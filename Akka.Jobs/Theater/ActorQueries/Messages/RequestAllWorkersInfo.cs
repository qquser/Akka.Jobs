namespace Akka.Jobs.Theater.ActorQueries.Messages;

internal sealed class RequestAllWorkersInfo
{
    public RequestAllWorkersInfo(long requestId, string groupName, TimeSpan timeout)
    {
        RequestId = requestId;
        GroupName = groupName;
        Timeout = timeout;
    }

    public long RequestId { get; }
    public string GroupName { get; }
    public TimeSpan Timeout { get; }
}