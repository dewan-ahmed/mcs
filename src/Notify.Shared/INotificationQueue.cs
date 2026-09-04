namespace Notify.Shared;

/// <summary>
/// Hands work to a consumer. In production this would be a message broker.
/// </summary>
public interface INotificationQueue
{
    void Enqueue(Notification notification);

    ValueTask<Notification> DequeueAsync(CancellationToken cancellationToken);
}
