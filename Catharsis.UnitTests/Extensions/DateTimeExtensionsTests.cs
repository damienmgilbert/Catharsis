using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="DateTimeExtensions"/> class.
///</summary>
[TestClass]
public class DateTimeExtensionsTests
{
    #region StartOfDay / EndOfDay

    [TestMethod]
    public void StartOfDay_ReturnsMidnight()
    {
        DateTime value = new(2026, 3, 15, 14, 30, 45);
        Assert.AreEqual(new DateTime(2026, 3, 15, 0, 0, 0), value.StartOfDay());
    }

    [TestMethod]
    public void EndOfDay_ReturnsLastTickOfDay()
    {
        DateTime value = new(2026, 3, 15, 14, 30, 45);
        DateTime result = value.EndOfDay();

        Assert.AreEqual(new DateTime(2026, 3, 15), result.Date);
        Assert.AreEqual(new DateTime(2026, 3, 16).AddTicks(-1), result);
    }

    #endregion

    #region StartOfMonth / EndOfMonth

    [TestMethod]
    public void StartOfMonth_ReturnsFirstDayAtMidnight()
    {
        DateTime value = new(2026, 3, 15, 14, 30, 45);
        Assert.AreEqual(new DateTime(2026, 3, 1), value.StartOfMonth());
    }

    [TestMethod]
    public void EndOfMonth_ReturnsLastTickOfMonth()
    {
        DateTime value = new(2026, 2, 10);
        DateTime result = value.EndOfMonth();
        Assert.AreEqual(new DateTime(2026, 3, 1).AddTicks(-1), result);
    }

    #endregion

    #region StartOfWeek

    [TestMethod]
    public void StartOfWeek_DefaultsToMonday()
    {
        // 2026-03-18 is a Wednesday.
        DateTime value = new(2026, 3, 18);
        Assert.AreEqual(new DateTime(2026, 3, 16), value.StartOfWeek());
        Assert.AreEqual(DayOfWeek.Monday, value.StartOfWeek().DayOfWeek);
    }

    [TestMethod]
    public void StartOfWeek_CustomStartDay_IsRespected()
    {
        // 2026-03-18 is a Wednesday; week starting Sunday should be 2026-03-15.
        DateTime value = new(2026, 3, 18);
        Assert.AreEqual(new DateTime(2026, 3, 15), value.StartOfWeek(DayOfWeek.Sunday));
    }

    [TestMethod]
    public void StartOfWeek_OnTheStartDay_ReturnsSameDay()
    {
        DateTime value = new(2026, 3, 16); // Monday
        Assert.AreEqual(new DateTime(2026, 3, 16), value.StartOfWeek());
    }

    #endregion

    #region IsWeekend

    [TestMethod]
    public void IsWeekend_Saturday_ReturnsTrue()
    {
        Assert.IsTrue(new DateTime(2026, 3, 21).IsWeekend());
    }

    [TestMethod]
    public void IsWeekend_Weekday_ReturnsFalse()
    {
        Assert.IsFalse(new DateTime(2026, 3, 18).IsWeekend());
    }

    #endregion

    #region AddBusinessDays

    [TestMethod]
    public void AddBusinessDays_Zero_ReturnsSameDate()
    {
        DateTime value = new(2026, 3, 18);
        Assert.AreEqual(value, value.AddBusinessDays(0));
    }

    [TestMethod]
    public void AddBusinessDays_SkipsWeekend()
    {
        // 2026-03-19 is a Thursday; adding 3 business days lands on Tuesday 2026-03-24 (skipping the weekend).
        DateTime value = new(2026, 3, 19);
        Assert.AreEqual(new DateTime(2026, 3, 24), value.AddBusinessDays(3));
    }

    [TestMethod]
    public void AddBusinessDays_Negative_GoesBackwardSkippingWeekend()
    {
        // 2026-03-23 is a Monday; going back 1 business day should land on Friday 2026-03-20.
        DateTime value = new(2026, 3, 23);
        Assert.AreEqual(new DateTime(2026, 3, 20), value.AddBusinessDays(-1));
    }

    #endregion
}
