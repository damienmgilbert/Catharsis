using Catharsis.Concurrency;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="AsyncCountdownEvent"/> class.
///</summary>
[TestClass]
public class AsyncCountdownEventTests
{
    #region Construction

    [TestMethod]
    public void Constructor_NegativeInitialCount_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new AsyncCountdownEvent(-1));
    }

    [TestMethod]
    public void Constructor_ZeroInitialCount_CurrentCountIsZero()
    {
        AsyncCountdownEvent countdown = new(0);
        Assert.AreEqual(0, countdown.CurrentCount);
    }

    [TestMethod]
    public async Task Constructor_ZeroInitialCount_WaitAsyncCompletesImmediately()
    {
        AsyncCountdownEvent countdown = new(0);
        await countdown.WaitAsync();
    }

    #endregion

    #region Signal / WaitAsync

    [TestMethod]
    public async Task Signal_ReachesZero_ReleasesWaiters()
    {
        AsyncCountdownEvent countdown = new(2);
        Task wait = countdown.WaitAsync();

        countdown.Signal();
        Assert.IsFalse(wait.IsCompleted);

        countdown.Signal();
        await wait;
    }

    [TestMethod]
    public void Signal_DecrementsCurrentCount()
    {
        AsyncCountdownEvent countdown = new(3);
        countdown.Signal();
        Assert.AreEqual(2, countdown.CurrentCount);
    }

    [TestMethod]
    public void Signal_WithCount_DecrementsByThatAmount()
    {
        AsyncCountdownEvent countdown = new(5);
        countdown.Signal(3);
        Assert.AreEqual(2, countdown.CurrentCount);
    }

    [TestMethod]
    public void Signal_ZeroOrNegativeCount_Throws()
    {
        AsyncCountdownEvent countdown = new(3);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => countdown.Signal(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => countdown.Signal(-1));
    }

    [TestMethod]
    public void Signal_CountExceedsCurrentCount_Throws()
    {
        AsyncCountdownEvent countdown = new(2);
        Assert.ThrowsExactly<InvalidOperationException>(() => countdown.Signal(3));
    }

    #endregion

    #region Cancellation

    [TestMethod]
    public async Task WaitAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        AsyncCountdownEvent countdown = new(1);
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await countdown.WaitAsync(cts.Token));
    }

    #endregion
}
