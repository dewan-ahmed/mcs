namespace Notify.Shared;

/// <summary>
/// Delivers a notification to a recipient. In production this would be an SMS/email provider.
/// </summary>
public interface INotificationSender
{
    string Send(Notification notification);
}
