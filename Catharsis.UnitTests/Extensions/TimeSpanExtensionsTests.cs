using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="TimeSpanExtensions"/> class.
///</summary>
[TestClass]
public class TimeSpanExtensionsTests
{
    #region Clamp

    [TestMethod]
    public void Clamp_MinGreaterThanMax_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => TimeSpan.FromSeconds(5).Clamp(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1)));
    }

    [TestMethod]
    public void Clamp_WithinRange_ReturnsUnchanged()
    {
        TimeSpan value = TimeSpan.FromSeconds(5);
        Assert.AreEqual(value, value.Clamp(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10)));
    }

    [TestMethod]
    public void Clamp_BelowMin_ReturnsMin()
    {
        TimeSpan result = TimeSpan.FromSeconds(0).Clamp(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
        Assert.AreEqual(TimeSpan.FromSeconds(1), result);
    }

    [TestMethod]
    public void Clamp_AboveMax_ReturnsMax()
    {
        TimeSpan result = TimeSpan.FromSeconds(20).Clamp(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
        Assert.AreEqual(TimeSpan.FromSeconds(10), result);
    }

    #endregion

    #region ToHumanReadable

    [TestMethod]
    public void ToHumanReadable_Zero_ReturnsZeroSeconds()
    {
        Assert.AreEqual("0s", TimeSpan.Zero.ToHumanReadable());
    }

    [TestMethod]
    public void ToHumanReadable_HoursAndMinutes_FormatsBothUnits()
    {
        Assert.AreEqual("2h 15m", TimeSpan.FromMinutes(135).ToHumanReadable());
    }

    [TestMethod]
    public void ToHumanReadable_DaysAndHours_FormatsBothUnits()
    {
        Assert.AreEqual("3d 4h", (TimeSpan.FromDays(3) + TimeSpan.FromHours(4)).ToHumanReadable());
    }

    [TestMethod]
    public void ToHumanReadable_OnlySeconds_FormatsSingleUnit()
    {
        Assert.AreEqual("45s", TimeSpan.FromSeconds(45).ToHumanReadable());
    }

    [TestMethod]
    public void ToHumanReadable_SkipsZeroUnitsInTheMiddle()
    {
        // Exactly 1 day, 0 hours, 5 minutes: the largest two non-zero units are days and minutes.
        TimeSpan value = TimeSpan.FromDays(1) + TimeSpan.FromMinutes(5);
        Assert.AreEqual("1d 5m", value.ToHumanReadable());
    }

    [TestMethod]
    public void ToHumanReadable_Negative_IsPrefixedWithMinus()
    {
        Assert.AreEqual("-2h 15m", TimeSpan.FromMinutes(-135).ToHumanReadable());
    }

    #endregion
}
