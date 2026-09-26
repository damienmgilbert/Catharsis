using Catharsis.Concurrency;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="AsyncManualResetEvent"/> class.
///</summary>
[TestClass]
public class AsyncManualResetEventTests
{
    #region Construction

    [TestMethod]
    public void Constructor_Default_IsNotSet()
    {
        AsyncManualResetEvent mre = new();
        Assert.IsFalse(mre.IsSet);
    }

    [TestMethod]
    public void Constructor_InitialStateTrue_IsSet()
    {
        AsyncManualResetEvent mre = new(initialState: true);
        Assert.IsTrue(mre.IsSet);
    }

    #endregion

    #region Set / Reset

    [TestMethod]
    public async Task Set_ReleasesWaiters()
    {
        AsyncManualResetEvent mre = new();
        Task wait = mre.WaitAsync();
        Assert.IsFalse(wait.IsCompleted);

        mre.Set();

        await wait;
        Assert.IsTrue(mre.IsSet);
    }

    [TestMethod]
    public async Task Set_ReleasesMultipleWaiters()
    {
        AsyncManualResetEvent mre = new();
        Task wait1 = mre.WaitAsync();
        Task wait2 = mre.WaitAsync();

        mre.Set();

        await Task.WhenAll(wait1, wait2);
    }

    [TestMethod]
    public async Task WaitAsync_AlreadySet_CompletesImmediately()
    {
        AsyncManualResetEvent mre = new(initialState: true);
        await mre.WaitAsync();
    }

    [TestMethod]
    public void Reset_ClearsSignaledState()
    {
        AsyncManualResetEvent mre = new(initialState: true);
        mre.Reset();
        Assert.IsFalse(mre.IsSet);
    }

    [TestMethod]
    public async Task Reset_ThenWaitAsync_BlocksUntilSetAgain()
    {
        AsyncManualResetEvent mre = new(initialState: true);
        mre.Reset();

        Task wait = mre.WaitAsync();
        Assert.IsFalse(wait.IsCompleted);

        mre.Set();
        await wait;
    }

    #endregion

    #region Cancellation

    [TestMethod]
    public async Task WaitAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        AsyncManualResetEvent mre = new();
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await mre.WaitAsync(cts.Token));
    }

    #endregion
}
