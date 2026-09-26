using Catharsis.Concurrency;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="AsyncAutoResetEvent"/> class.
///</summary>
[TestClass]
public class AsyncAutoResetEventTests
{
    #region Construction

    [TestMethod]
    public async Task Constructor_InitialStateTrue_FirstWaitCompletesImmediately()
    {
        AsyncAutoResetEvent are = new(initialState: true);
        await are.WaitAsync();
    }

    #endregion

    #region Set / WaitAsync

    [TestMethod]
    public async Task Set_ReleasesExactlyOneWaiter()
    {
        AsyncAutoResetEvent are = new();
        Task wait1 = are.WaitAsync();
        Task wait2 = are.WaitAsync();

        are.Set();
        await Task.Delay(20);

        Assert.IsTrue(wait1.IsCompleted != wait2.IsCompleted);
    }

    [TestMethod]
    public async Task Set_TwiceReleasesTwoWaiters()
    {
        AsyncAutoResetEvent are = new();
        Task wait1 = are.WaitAsync();
        Task wait2 = are.WaitAsync();

        are.Set();
        are.Set();

        await Task.WhenAll(wait1, wait2);
    }

    [TestMethod]
    public async Task Set_WithoutWaiters_RetainsSignalForNextWait()
    {
        AsyncAutoResetEvent are = new();
        are.Set();
        await are.WaitAsync();
    }

    [TestMethod]
    public async Task Set_ConsumedSignal_DoesNotReleaseSecondWait()
    {
        AsyncAutoResetEvent are = new();
        are.Set();
        await are.WaitAsync();

        Task wait = are.WaitAsync();
        await Task.Delay(20);

        Assert.IsFalse(wait.IsCompleted);
    }

    #endregion

    #region Cancellation

    [TestMethod]
    public void WaitAsync_CancellationAlreadyRequested_ThrowsImmediately()
    {
        AsyncAutoResetEvent are = new();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.ThrowsExactly<OperationCanceledException>(() => are.WaitAsync(cts.Token));
    }

    [TestMethod]
    public async Task WaitAsync_CanceledWhileWaiting_ThrowsAndReleasesLaterSetToNextWaiter()
    {
        AsyncAutoResetEvent are = new();
        using CancellationTokenSource cts = new();

        Task canceledWait = are.WaitAsync(cts.Token);
        await cts.CancelAsync();

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await canceledWait);

        Task nextWait = are.WaitAsync();
        are.Set();
        await nextWait;
    }

    #endregion
}
