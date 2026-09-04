using Notify.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<INotificationStore, InMemoryNotificationStore>();

var app = builder.Build();

var appEnvironment = app.Configuration["AppEnvironment"] ?? "unknown";
app.Logger.LogInformation("Notify.Public.Api starting in AppEnvironment={AppEnvironment}", appEnvironment);

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/health"))
    {
        await next();
        return;
    }

    var configuredKey = context.RequestServices.GetRequiredService<IConfiguration>()["ApiKey"];
    if (string.IsNullOrEmpty(configuredKey))
    {
        await next();
        return;
    }

    if (!context.Request.Headers.TryGetValue("X-Api-Key", out var provided) || provided != configuredKey)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    await next();
});

app.MapPost("/api/callbacks/status", (StatusCallbackRequest request, INotificationStore store) =>
{
    if (request.Id == Guid.Empty || string.IsNullOrWhiteSpace(request.Status))
    {
        return Results.BadRequest();
    }

    return store.UpdateStatus(request.Id, request.Status.Trim())
        ? Results.NoContent()
        : Results.NotFound();
});

app.MapGet("/api/notifications/{id:guid}/status", (Guid id, INotificationStore store) =>
{
    var notification = store.GetById(id);
    return notification is null
        ? Results.NotFound()
        : Results.Ok(new { notification.Id, notification.Status });
});

app.MapGet("/health", () => Results.Ok());
app.MapGet("/health/ready", () => Results.Ok());

app.Run();

internal sealed record StatusCallbackRequest(Guid Id, string Status);
