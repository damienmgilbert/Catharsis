using Catharsis.Common;

namespace Catharsis.UnitTests.Common;

[TestClass]
public class ValueStopwatchTests
{
    #region Public methods
    [TestMethod]
    public void Default_IsNotActive()
    {
        ValueStopwatch sw = default;
        Assert.IsFalse(sw.IsActive);
    }

    [TestMethod]
    public void GetElapsedMicroseconds_ReturnsNonNegative()
    {
        ValueStopwatch sw = ValueStopwatch.StartNew();
        Assert.IsGreaterThanOrEqualTo(0, sw.GetElapsedMicroseconds());
    }

    [TestMethod]
    public void GetElapsedMilliseconds_ReturnsNonNegative()
    {
        ValueStopwatch sw = ValueStopwatch.StartNew();
        Assert.IsGreaterThanOrEqualTo(0, sw.GetElapsedMilliseconds());
    }

    [TestMethod]
    public void GetElapsedTime_NotStarted_Throws()
    {
        ValueStopwatch sw = default;
        Assert.ThrowsExactly<InvalidOperationException>(() => sw.GetElapsedTime());
    }

    [TestMethod]
    public void GetElapsedTime_Started_ReturnsNonNegative()
    {
        ValueStopwatch sw = ValueStopwatch.StartNew();
        Thread.Sleep(10);
        TimeSpan elapsed = sw.GetElapsedTime();
        Assert.IsGreaterThanOrEqualTo(0, elapsed.TotalMilliseconds);
    }

    [TestMethod]
    public void StartNew_IsActive()
    {
        ValueStopwatch sw = ValueStopwatch.StartNew();
        Assert.IsTrue(sw.IsActive);
    }
    #endregion
}
