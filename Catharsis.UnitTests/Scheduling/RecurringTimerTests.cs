using Catharsis.Scheduling;

namespace Catharsis.UnitTests.Scheduling;

///<summary>
///Unit tests for the <see cref="RecurringTimer"/> class.
///</summary>
[TestClass]
public class RecurringTimerTests
{
    #region Construction

    [TestMethod]
    public void Constructor_ZeroOrNegativePeriod_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new RecurringTimer(TimeSpan.Zero, static _ => Task.CompletedTask));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new RecurringTimer(TimeSpan.FromMilliseconds(-1), static _ => Task.CompletedTask));
    }

    [TestMethod]
    public void Constructor_NullCallback_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new RecurringTimer(TimeSpan.FromMilliseconds(10), null!));
    }

    #endregion

    #region Ticking

    [TestMethod]
    public async Task Callback_InvokedRepeatedlyOnPeriod()
    {
        int callCount = 0;
        RecurringTimer timer = new(TimeSpan.FromMilliseconds(10), _ =>
        {
            Interlocked.Increment(ref callCount);
            return Task.CompletedTask;
        });

        await Task.Delay(100);
        await timer.DisposeAsync();

        Assert.IsGreaterThanOrEqualTo(2, callCount);
    }

    #endregion

    #region Disposal

    [TestMethod]
    public async Task DisposeAsync_StopsFurtherCallbacks()
    {
        int callCount = 0;
        RecurringTimer timer = new(TimeSpan.FromMilliseconds(10), _ =>
        {
            Interlocked.Increment(ref callCount);
            return Task.CompletedTask;
        });

        await Task.Delay(30);
        await timer.DisposeAsync();
        int countAtDisposal = callCount;

        await Task.Delay(50);

        Assert.AreEqual(countAtDisposal, callCount);
    }

    [TestMethod]
    public async Task DisposeAsync_IsIdempotent()
    {
        RecurringTimer timer = new(TimeSpan.FromMilliseconds(50), static _ => Task.CompletedTask);
        await timer.DisposeAsync();
        await timer.DisposeAsync();
    }

    [TestMethod]
    public async Task Completion_CallbackThrows_SurfacesException()
    {
        RecurringTimer timer = new(TimeSpan.FromMilliseconds(10), static _ => throw new InvalidOperationException("boom"));

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await timer.Completion);

        await timer.DisposeAsync();
    }

    #endregion
}
