namespace Akka.Jobs.Theater.ActorQueries.Messages;

internal sealed class CollectionTimeout
{
    public static CollectionTimeout Instance { get; } = new ();

    private CollectionTimeout()
    {
    }
}