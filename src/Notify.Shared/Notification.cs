namespace Notify.Shared;

public record Notification(
    Guid Id,
    string Recipient,
    string Body,
    string Status,
    DateTimeOffset CreatedAt);
