namespace Notify.Shared;

/// <summary>
/// No-op sender that logs to the console and returns a fake provider identifier.
/// </summary>
public sealed class InMemoryNotificationSender : INotificationSender
{
    public string Send(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        var providerId = $"mem-{Guid.NewGuid():N}";
        Console.WriteLine(
            $"[InMemoryNotificationSender] Delivered {notification.Id} to {notification.Recipient} (provider {providerId})");
        return providerId;
    }
}
