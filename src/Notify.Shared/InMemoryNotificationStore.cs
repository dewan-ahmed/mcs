using System.Collections.Concurrent;

namespace Notify.Shared;

public sealed class InMemoryNotificationStore : INotificationStore
{
    private readonly ConcurrentDictionary<Guid, Notification> _items = new();

    public void Add(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (!_items.TryAdd(notification.Id, notification))
        {
            throw new InvalidOperationException($"Notification '{notification.Id}' already exists.");
        }
    }

    public Notification? GetById(Guid id) =>
        _items.TryGetValue(id, out var notification) ? notification : null;

    public bool UpdateStatus(Guid id, string status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);

        while (_items.TryGetValue(id, out var current))
        {
            var updated = current with { Status = status };
            if (_items.TryUpdate(id, updated, current))
            {
                return true;
            }
        }

        return false;
    }
}
