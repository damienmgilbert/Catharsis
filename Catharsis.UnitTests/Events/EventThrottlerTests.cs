using Catharsis.Events;

namespace Catharsis.UnitTests.Events;

///<summary>
///Unit tests for the <see cref="EventThrottler{T}"/> class.
///</summary>
[TestClass]
public class EventThrottlerTests
{
    sealed class ManualTimeProvider : TimeProvider
    {
        long _ticks;

        public override long TimestampFrequency => TimeSpan.TicksPerSecond;

        public override long GetTimestamp() => _ticks;

        public void Advance(TimeSpan by) => _ticks += by.Ticks;
    }

    [TestMethod]
    public void Constructor_NegativeInterval_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new EventThrottler<int>(TimeSpan.FromSeconds(-1), static _ => { })); }

    [TestMethod]
    public void Constructor_NullAction_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new EventThrottler<int>(TimeSpan.Zero, null!)); }

    [TestMethod]
    public void TryInvoke_FirstCall_RunsImmediately()
    {
        List<int> seen = [];
        EventThrottler<int> throttler = new(TimeSpan.FromSeconds(1), seen.Add, new ManualTimeProvider());

        Assert.IsTrue(throttler.TryInvoke(1));
        CollectionAssert.AreEqual(new[] { 1 }, seen);
    }

    [TestMethod]
    public void TryInvoke_InsideWindow_IsDroppedThenRunsAfterWindow()
    {
        ManualTimeProvider clock = new();
        List<int> seen = [];
        EventThrottler<int> throttler = new(TimeSpan.FromSeconds(1), seen.Add, clock);

        Assert.IsTrue(throttler.TryInvoke(1));
        clock.Advance(TimeSpan.FromMilliseconds(500));
        Assert.IsFalse(throttler.TryInvoke(2));
        clock.Advance(TimeSpan.FromMilliseconds(500));
        Assert.IsTrue(throttler.TryInvoke(3));

        CollectionAssert.AreEqual(new[] { 1, 3 }, seen);
    }

    [TestMethod]
    public void Reset_AllowsImmediateRun()
    {
        List<int> seen = [];
        EventThrottler<int> throttler = new(TimeSpan.FromSeconds(1), seen.Add, new ManualTimeProvider());

        throttler.TryInvoke(1);
        throttler.Reset();

        Assert.IsTrue(throttler.TryInvoke(2));
    }
}
