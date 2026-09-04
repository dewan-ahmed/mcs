using System;
using System.Threading;
using System.Threading.Tasks;
using Notify.Shared;

internal sealed class NotificationWorker
{
    private static readonly TimeSpan SyntheticInterval = TimeSpan.FromSeconds(30);

    private readonly INotificationStore _store;
    private readonly INotificationQueue _queue;
    private readonly INotificationSender _sender;

    public NotificationWorker(
        INotificationStore store,
        INotificationQueue queue,
        INotificationSender sender)
    {
        _store = store;
        _queue = queue;
        _sender = sender;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Worker running. SIGTERM finishes the in-flight item, then exits.");

        var producer = ProduceSyntheticAsync(stoppingToken);
        var consumer = ConsumeAsync(stoppingToken);
        await Task.WhenAll(producer, consumer);
    }

    private async Task ProduceSyntheticAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SyntheticInterval);
        try
        {
            do
            {
                var notification = new Notification(
                    Guid.NewGuid(),
                    "worker@local",
                    "synthetic heartbeat",
                    "queued",
                    DateTimeOffset.UtcNow);

                _store.Add(notification);
                _queue.Enqueue(notification);
                Console.WriteLine($"Enqueued synthetic notification {notification.Id}");
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is shutting down (SIGTERM). Stop producing new work.
        }
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Notification item;
            try
            {
                item = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var providerId = _sender.Send(item);
                _store.UpdateStatus(item.Id, "sent");
                Console.WriteLine(
                    $"Sent notification {item.Id} to {item.Recipient}; provider={providerId}; status=sent");
            }
            catch (Exception ex)
            {
                _store.UpdateStatus(item.Id, "failed");
                Console.Error.WriteLine($"Failed to send notification {item.Id}: {ex}");
            }
        }
    }
}
