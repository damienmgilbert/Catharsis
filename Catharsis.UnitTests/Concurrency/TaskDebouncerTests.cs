using Catharsis.Concurrency;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="TaskDebouncer"/> class.
///</summary>
[TestClass]
public class TaskDebouncerTests
{
    #region Construction

    [TestMethod]
    public void Constructor_NegativeDelay_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new TaskDebouncer(TimeSpan.FromMilliseconds(-1), static _ => Task.CompletedTask));
    }

    [TestMethod]
    public void Constructor_NullAction_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new TaskDebouncer(TimeSpan.Zero, null!));
    }

    #endregion

    #region Trigger

    [TestMethod]
    public async Task Trigger_SingleCall_RunsActionAfterDelay()
    {
        int callCount = 0;
        using TaskDebouncer debouncer = new(TimeSpan.FromMilliseconds(20), _ =>
        {
            Interlocked.Increment(ref callCount);
            return Task.CompletedTask;
        });

        debouncer.Trigger();
        await Task.Delay(100);

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public async Task Trigger_RapidCalls_CoalesceIntoSingleRun()
    {
        int callCount = 0;
        using TaskDebouncer debouncer = new(TimeSpan.FromMilliseconds(40), _ =>
        {
            Interlocked.Increment(ref callCount);
            return Task.CompletedTask;
        });

        debouncer.Trigger();
        await Task.Delay(10);
        debouncer.Trigger();
        await Task.Delay(10);
        debouncer.Trigger();

        await Task.Delay(100);

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public async Task Trigger_AfterDisposal_Throws()
    {
        TaskDebouncer debouncer = new(TimeSpan.FromMilliseconds(10), static _ => Task.CompletedTask);
        debouncer.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(debouncer.Trigger);
        await Task.CompletedTask;
    }

    #endregion

    #region Disposal

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        TaskDebouncer debouncer = new(TimeSpan.FromMilliseconds(10), static _ => Task.CompletedTask);
        debouncer.Dispose();
        debouncer.Dispose();
    }

    [TestMethod]
    public async Task Dispose_CancelsPendingRun()
    {
        int callCount = 0;
        TaskDebouncer debouncer = new(TimeSpan.FromMilliseconds(50), _ =>
        {
            Interlocked.Increment(ref callCount);
            return Task.CompletedTask;
        });

        debouncer.Trigger();
        debouncer.Dispose();

        await Task.Delay(100);

        Assert.AreEqual(0, callCount);
    }

    #endregion
}
