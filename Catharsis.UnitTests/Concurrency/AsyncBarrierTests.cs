using Catharsis.Concurrency;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="AsyncBarrier"/> class.
///</summary>
[TestClass]
public class AsyncBarrierTests
{
    #region Construction

    [TestMethod]
    public void Constructor_ZeroOrNegativeParticipantCount_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new AsyncBarrier(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new AsyncBarrier(-1));
    }

    [TestMethod]
    public void Constructor_SetsParticipantCount()
    {
        AsyncBarrier barrier = new(3);
        Assert.AreEqual(3, barrier.ParticipantCount);
    }

    #endregion

    #region SignalAndWaitAsync

    [TestMethod]
    public async Task SignalAndWaitAsync_SingleParticipant_CompletesImmediately()
    {
        AsyncBarrier barrier = new(1);
        await barrier.SignalAndWaitAsync();
    }

    [TestMethod]
    public async Task SignalAndWaitAsync_AllParticipantsArrive_ReleasesEveryone()
    {
        AsyncBarrier barrier = new(3);

        Task p1 = barrier.SignalAndWaitAsync();
        Task p2 = barrier.SignalAndWaitAsync();
        Assert.IsFalse(p1.IsCompleted);
        Assert.IsFalse(p2.IsCompleted);

        Task p3 = barrier.SignalAndWaitAsync();

        await Task.WhenAll(p1, p2, p3);
    }

    [TestMethod]
    public async Task SignalAndWaitAsync_ResetsForNextPhase()
    {
        AsyncBarrier barrier = new(2);

        await Task.WhenAll(barrier.SignalAndWaitAsync(), barrier.SignalAndWaitAsync());

        Task phase2A = barrier.SignalAndWaitAsync();
        Assert.IsFalse(phase2A.IsCompleted);

        Task phase2B = barrier.SignalAndWaitAsync();
        await Task.WhenAll(phase2A, phase2B);
    }

    #endregion

    #region Cancellation

    [TestMethod]
    public async Task SignalAndWaitAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        AsyncBarrier barrier = new(2);
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await barrier.SignalAndWaitAsync(cts.Token));
    }

    #endregion
}
