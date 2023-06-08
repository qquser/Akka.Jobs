using Akka.Actor;

namespace Akka.Jobs.Theater.Master.Groups.Workers.Messages;

internal sealed class TrySaveWorkerActorRefCommand
{
    public TrySaveWorkerActorRefCommand(
        IActorRef actorRef, 
        Guid slaveActorId,
        IActorRef downloadQuerySender)
    {
        ActorRef = actorRef;
        SlaveActorId = slaveActorId;
        DownloadQuerySender = downloadQuerySender;
    }

    public IActorRef DownloadQuerySender { get; }
    public IActorRef ActorRef { get; }
    public Guid SlaveActorId { get; }
}