using System.Threading.Channels;

namespace Notify.Shared;

public sealed class InMemoryNotificationQueue : INotificationQueue
{
    private readonly Channel<Notification> _channel = Channel.CreateUnbounded<Notification>(
        new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });

    public void Enqueue(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (!_channel.Writer.TryWrite(notification))
        {
            throw new InvalidOperationException("Queue is no longer accepting notifications.");
        }
    }

    public ValueTask<Notification> DequeueAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAsync(cancellationToken);
}
