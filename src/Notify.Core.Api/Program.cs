using Notify.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<INotificationStore, InMemoryNotificationStore>();
builder.Services.AddSingleton<INotificationQueue, InMemoryNotificationQueue>();

var app = builder.Build();

var appEnvironment = app.Configuration["AppEnvironment"] ?? "unknown";
app.Logger.LogInformation("Notify.Core.Api starting in AppEnvironment={AppEnvironment}", appEnvironment);

app.MapPost("/api/notifications", (CreateNotificationRequest request, INotificationStore store, INotificationQueue queue) =>
{
    if (string.IsNullOrWhiteSpace(request.Recipient) || string.IsNullOrWhiteSpace(request.Body))
    {
        return Results.BadRequest();
    }

    var notification = new Notification(
        Guid.NewGuid(),
        request.Recipient.Trim(),
        request.Body.Trim(),
        "queued",
        DateTimeOffset.UtcNow);

    store.Add(notification);
    queue.Enqueue(notification);
    return Results.Created($"/api/notifications/{notification.Id}", notification);
});

app.MapGet("/api/notifications/{id:guid}", (Guid id, INotificationStore store) =>
{
    var notification = store.GetById(id);
    return notification is null ? Results.NotFound() : Results.Ok(notification);
});

app.MapGet("/health", () => Results.Ok());
app.MapGet("/health/ready", () => Results.Ok());

app.Run();

internal sealed record CreateNotificationRequest(string Recipient, string Body);
