using Catharsis.Common;

namespace Catharsis.UnitTests.Common;

///<summary>
///Unit tests for the <see cref="Clock"/> class.
///</summary>
[TestClass]
public class ClockTests
{
    #region Public methods

    [TestMethod]
    public void UtcNow_DefaultProvider_IsCloseToRealUtcNow()
    {
        ISystemClock clock = new Clock();

        DateTimeOffset before = DateTimeOffset.UtcNow;
        DateTimeOffset result = clock.UtcNow;
        DateTimeOffset after = DateTimeOffset.UtcNow;

        Assert.IsTrue((result >= before) && (result <= after));
    }

    [TestMethod]
    public void UtcNow_CustomTimeProvider_ReturnsProviderTime()
    {
        DateTimeOffset fixedTime = new(2030, 6, 15, 12, 0, 0, TimeSpan.Zero);
        FakeTimeProvider timeProvider = new(fixedTime);
        ISystemClock clock = new Clock(timeProvider);

        Assert.AreEqual(fixedTime, clock.UtcNow);
    }

    #endregion

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        #region Public methods
        public override DateTimeOffset GetUtcNow() => now;
        #endregion
    }
}
