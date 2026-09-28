using Catharsis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catharsis.UnitTests.Services;

///<summary>
///Unit tests for the <see cref="BackgroundQueueService"/> class.
///</summary>
[TestClass]
public class BackgroundQueueServiceTests
{
    #region Private methods
    private static ILogger<BackgroundQueueService> CreateLogger() => NullLoggerFactory.Instance.CreateLogger<BackgroundQueueService>();
    #endregion

    #region Constructor

    [TestMethod]
    public void Constructor_NullLogger_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new BackgroundQueueService(null!)); }

    [TestMethod]
    public void Constructor_NonPositiveCapacity_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BackgroundQueueService(CreateLogger(), 0)); }

    #endregion

    #region EnqueueAsync

    [TestMethod]
    public async Task EnqueueAsync_NullWorkItem_Throws()
    {
        await using BackgroundQueueService service = new(CreateLogger());
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => service.EnqueueAsync(null!).AsTask());
    }

    [TestMethod]
    public async Task EnqueueAsync_QueuedWorkItem_EventuallyRuns()
    {
        await using BackgroundQueueService service = new(CreateLogger());
        TaskCompletionSource<bool> ran = new();

        await service.EnqueueAsync(_ => { ran.SetResult(true); return Task.CompletedTask; });

        bool result = await ran.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task EnqueueAsync_IncrementsQueuedCount()
    {
        await using BackgroundQueueService service = new(CreateLogger());
        await service.EnqueueAsync(static _ => Task.CompletedTask);

        Assert.AreEqual(1, service.QueuedCount);
    }

    [TestMethod]
    public async Task EnqueueAsync_WorkItemThrows_DoesNotStopSubsequentItems()
    {
        await using BackgroundQueueService service = new(CreateLogger());
        TaskCompletionSource<bool> secondRan = new();

        await service.EnqueueAsync(static _ => throw new InvalidOperationException());
        await service.EnqueueAsync(_ => { secondRan.SetResult(true); return Task.CompletedTask; });

        bool result = await secondRan.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsTrue(result);
    }

    #endregion

    #region DisposeAsync

    [TestMethod]
    public async Task DisposeAsync_AfterDispose_EnqueueThrowsObjectDisposedException()
    {
        BackgroundQueueService service = new(CreateLogger());
        await service.DisposeAsync();

        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => service.EnqueueAsync(static _ => Task.CompletedTask).AsTask());
    }

    [TestMethod]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        BackgroundQueueService service = new(CreateLogger());
        await service.DisposeAsync();
        await service.DisposeAsync();
    }

    #endregion
}
