using Catharsis.Common;
using Catharsis.Patterns.Composed;

namespace Catharsis.UnitTests.Patterns.Composed;

///<summary>
///Unit tests for <see cref="TimeProviderSystemClock"/> and <see cref="SystemClockTimeProvider"/>.
///</summary>
[TestClass]
public class ClockAdapterTests
{
    static readonly DateTimeOffset Fixed = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);

    sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Fixed;
    }

    sealed class FixedClock : ISystemClock
    {
        public DateTimeOffset UtcNow => Fixed;
    }

    [TestMethod]
    public void TimeProviderSystemClock_ReadsFromTimeProvider() { Assert.AreEqual(Fixed, new TimeProviderSystemClock(new FixedTimeProvider()).UtcNow); }

    [TestMethod]
    public void TimeProviderSystemClock_NullProvider_UsesSystemTime()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;
        DateTimeOffset reading = new TimeProviderSystemClock().UtcNow;

        Assert.IsTrue(reading >= before && reading <= DateTimeOffset.UtcNow);
    }

    [TestMethod]
    public void SystemClockTimeProvider_ReadsFromClock()
    {
        SystemClockTimeProvider provider = new(new FixedClock());

        Assert.AreEqual(Fixed, provider.GetUtcNow());
        Assert.AreEqual(Fixed.UtcDateTime, provider.GetUtcNow().UtcDateTime);
    }

    [TestMethod]
    public void SystemClockTimeProvider_NullClock_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new SystemClockTimeProvider(null!)); }

    [TestMethod]
    public void RoundTrip_ClockThroughProviderAndBack_KeepsTime() { Assert.AreEqual(Fixed, new TimeProviderSystemClock(new SystemClockTimeProvider(new FixedClock())).UtcNow); }
}
