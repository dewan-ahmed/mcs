namespace Notify.Shared;

/// <summary>
/// Persists notifications. In production this would be a SQL database.
/// </summary>
public interface INotificationStore
{
    void Add(Notification notification);

    Notification? GetById(Guid id);

    bool UpdateStatus(Guid id, string status);
}
