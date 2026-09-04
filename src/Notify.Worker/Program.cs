using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Notify.Shared;

var appEnvironment = Environment.GetEnvironmentVariable("AppEnvironment")
    ?? ReadAppSetting("AppEnvironment")
    ?? "unknown";

Console.WriteLine($"Notify.Worker starting in AppEnvironment={appEnvironment}");

using var shutdown = new CancellationTokenSource();
using var sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
{
    context.Cancel = true;
    shutdown.Cancel();
});
using var sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, context =>
{
    context.Cancel = true;
    shutdown.Cancel();
});

var store = new InMemoryNotificationStore();
var queue = new InMemoryNotificationQueue();
var sender = new InMemoryNotificationSender();
var worker = new NotificationWorker(store, queue, sender);

try
{
    await worker.RunAsync(shutdown.Token);
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
}

Console.WriteLine("Notify.Worker stopped.");

static string? ReadAppSetting(string key)
{
    var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    if (!File.Exists(path))
    {
        return null;
    }

    using var document = JsonDocument.Parse(File.ReadAllText(path));
    return document.RootElement.TryGetProperty(key, out var value) ? value.GetString() : null;
}
