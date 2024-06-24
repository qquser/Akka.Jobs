using Akka.Actor;

namespace Akka.Jobs.Theater.Master.Groups.Workers.Messages;

internal sealed class TrySaveWorkerActorRefCommand(
    IActorRef actorRef,
    string slaveActorId,
    IActorRef downloadQuerySender)
{
    public IActorRef DownloadQuerySender { get; } = downloadQuerySender;
    public IActorRef ActorRef { get; } = actorRef;
    public string SlaveActorId { get; } = slaveActorId;
}