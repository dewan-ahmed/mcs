using Notify.Shared;

namespace Notify.UnitTests;

public class InMemoryNotificationStoreTests
{
    [Fact]
    public void Add_ThenGetById_ReturnsTheSameNotification()
    {
        var store = new InMemoryNotificationStore();
        var notification = NewNotification("queued");

        store.Add(notification);

        var found = store.GetById(notification.Id);
        Assert.NotNull(found);
        Assert.Equal(notification, found);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        var store = new InMemoryNotificationStore();
        Assert.Null(store.GetById(Guid.NewGuid()));
    }

    [Fact]
    public void UpdateStatus_ReplacesStatus_WhenPresent()
    {
        var store = new InMemoryNotificationStore();
        var notification = NewNotification("queued");
        store.Add(notification);

        var updated = store.UpdateStatus(notification.Id, "sent");

        Assert.True(updated);
        Assert.Equal("sent", store.GetById(notification.Id)!.Status);
    }

    [Fact]
    public void UpdateStatus_UnknownId_ReturnsFalse()
    {
        var store = new InMemoryNotificationStore();
        Assert.False(store.UpdateStatus(Guid.NewGuid(), "sent"));
    }

    private static Notification NewNotification(string status) =>
        new(Guid.NewGuid(), "user@example.com", "hello", status, DateTimeOffset.UnixEpoch);
}

public class InMemoryNotificationQueueTests
{
    [Fact]
    public async Task DequeueAsync_ReturnsEnqueuedItem()
    {
        var queue = new InMemoryNotificationQueue();
        var notification = new Notification(
            Guid.NewGuid(),
            "user@example.com",
            "hello",
            "queued",
            DateTimeOffset.UnixEpoch);

        queue.Enqueue(notification);
        var dequeued = await queue.DequeueAsync(CancellationToken.None);

        Assert.Equal(notification, dequeued);
    }

    [Fact]
    public async Task DequeueAsync_WaitsUntilCancelled_WhenEmpty()
    {
        var queue = new InMemoryNotificationQueue();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await queue.DequeueAsync(cts.Token).AsTask());
    }
}

public class InMemoryNotificationSenderTests
{
    [Fact]
    public void Send_ReturnsProviderId_WithoutThrowing()
    {
        var sender = new InMemoryNotificationSender();
        var notification = new Notification(
            Guid.NewGuid(),
            "user@example.com",
            "hello",
            "queued",
            DateTimeOffset.UnixEpoch);

        var providerId = sender.Send(notification);

        Assert.False(string.IsNullOrWhiteSpace(providerId));
    }
}
