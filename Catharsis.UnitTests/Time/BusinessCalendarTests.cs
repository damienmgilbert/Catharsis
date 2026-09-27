using Catharsis.Time;

namespace Catharsis.UnitTests.Time;

///<summary>
///Unit tests for the <see cref="BusinessCalendar"/> class.
///</summary>
[TestClass]
public class BusinessCalendarTests
{
    #region IsBusinessDay

    [TestMethod]
    public void IsBusinessDay_Weekday_ReturnsTrue()
    {
        BusinessCalendar calendar = new();
        Assert.IsTrue(calendar.IsBusinessDay(new DateOnly(2024, 1, 8)));
    }

    [TestMethod]
    public void IsBusinessDay_DefaultWeekend_ReturnsFalse()
    {
        BusinessCalendar calendar = new();
        Assert.IsFalse(calendar.IsBusinessDay(new DateOnly(2024, 1, 6)));
        Assert.IsFalse(calendar.IsBusinessDay(new DateOnly(2024, 1, 7)));
    }

    [TestMethod]
    public void IsBusinessDay_ConfiguredHoliday_ReturnsFalse()
    {
        DateOnly holiday = new(2024, 1, 1);
        BusinessCalendar calendar = new([holiday]);

        Assert.IsFalse(calendar.IsBusinessDay(holiday));
    }

    [TestMethod]
    public void IsBusinessDay_CustomWeekendDays_HonorsConfiguration()
    {
        BusinessCalendar calendar = new(weekendDays: [DayOfWeek.Friday, DayOfWeek.Saturday]);

        Assert.IsFalse(calendar.IsBusinessDay(new DateOnly(2024, 1, 5)));
        Assert.IsTrue(calendar.IsBusinessDay(new DateOnly(2024, 1, 7)));
    }

    #endregion

    #region AddBusinessDays

    [TestMethod]
    public void AddBusinessDays_SkipsWeekend()
    {
        BusinessCalendar calendar = new();
        DateOnly result = calendar.AddBusinessDays(new DateOnly(2024, 1, 5), 1);
        Assert.AreEqual(new DateOnly(2024, 1, 8), result);
    }

    [TestMethod]
    public void AddBusinessDays_SkipsHoliday()
    {
        DateOnly holiday = new(2024, 1, 8);
        BusinessCalendar calendar = new([holiday]);

        DateOnly result = calendar.AddBusinessDays(new DateOnly(2024, 1, 5), 1);

        Assert.AreEqual(new DateOnly(2024, 1, 9), result);
    }

    [TestMethod]
    public void AddBusinessDays_Negative_CountsBackward()
    {
        BusinessCalendar calendar = new();
        DateOnly result = calendar.AddBusinessDays(new DateOnly(2024, 1, 8), -1);
        Assert.AreEqual(new DateOnly(2024, 1, 5), result);
    }

    [TestMethod]
    public void AddBusinessDays_Zero_ReturnsSameDate()
    {
        BusinessCalendar calendar = new();
        DateOnly date = new(2024, 1, 8);
        Assert.AreEqual(date, calendar.AddBusinessDays(date, 0));
    }

    #endregion

    #region CountBusinessDays

    [TestMethod]
    public void CountBusinessDays_FullWeek_ReturnsFive()
    {
        BusinessCalendar calendar = new();
        int count = calendar.CountBusinessDays(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 7));
        Assert.AreEqual(5, count);
    }

    [TestMethod]
    public void CountBusinessDays_ReversedArguments_StillCountsCorrectly()
    {
        BusinessCalendar calendar = new();
        int count = calendar.CountBusinessDays(new DateOnly(2024, 1, 7), new DateOnly(2024, 1, 1));
        Assert.AreEqual(5, count);
    }

    #endregion
}
