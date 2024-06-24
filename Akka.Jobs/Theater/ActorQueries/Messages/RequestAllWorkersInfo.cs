namespace Akka.Jobs.Theater.ActorQueries.Messages;

internal sealed class RequestAllWorkersInfo(long requestId, string groupId, TimeSpan timeout)
{
    public long RequestId { get; } = requestId;
    public string GroupId { get; } = groupId;
    public TimeSpan Timeout { get; } = timeout;
}