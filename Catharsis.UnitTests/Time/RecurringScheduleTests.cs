using Catharsis.Time;

namespace Catharsis.UnitTests.Time;

///<summary>
///Unit tests for the <see cref="RecurringSchedule"/> class.
///</summary>
[TestClass]
public class RecurringScheduleTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullTimeZone_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new RecurringSchedule(DayOfWeek.Monday, new TimeOnly(9, 0), null!)); }

    #endregion

    #region GetNextOccurrence

    [TestMethod]
    public void GetNextOccurrence_LaterThisWeek_ReturnsThisWeeksOccurrence()
    {
        RecurringSchedule schedule = new(DayOfWeek.Friday, new TimeOnly(9, 0), TimeZoneInfo.Utc);
        DateTimeOffset after = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

        DateTimeOffset next = schedule.GetNextOccurrence(after);

        Assert.AreEqual(new DateTimeOffset(2024, 1, 5, 9, 0, 0, TimeSpan.Zero), next);
    }

    [TestMethod]
    public void GetNextOccurrence_SameDayButTimeAlreadyPassed_RollsToNextWeek()
    {
        RecurringSchedule schedule = new(DayOfWeek.Monday, new TimeOnly(9, 0), TimeZoneInfo.Utc);
        DateTimeOffset after = new(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);

        DateTimeOffset next = schedule.GetNextOccurrence(after);

        Assert.AreEqual(new DateTimeOffset(2024, 1, 8, 9, 0, 0, TimeSpan.Zero), next);
    }

    [TestMethod]
    public void GetNextOccurrence_SameDayBeforeTime_ReturnsToday()
    {
        RecurringSchedule schedule = new(DayOfWeek.Monday, new TimeOnly(9, 0), TimeZoneInfo.Utc);
        DateTimeOffset after = new(2024, 1, 1, 8, 0, 0, TimeSpan.Zero);

        DateTimeOffset next = schedule.GetNextOccurrence(after);

        Assert.AreEqual(new DateTimeOffset(2024, 1, 1, 9, 0, 0, TimeSpan.Zero), next);
    }

    [TestMethod]
    public void GetNextOccurrence_AlwaysStrictlyAfterInput()
    {
        RecurringSchedule schedule = new(DayOfWeek.Wednesday, new TimeOnly(9, 0), TimeZoneInfo.Utc);
        DateTimeOffset after = DateTimeOffset.UtcNow;

        DateTimeOffset next = schedule.GetNextOccurrence(after);

        Assert.IsTrue(next > after);
        Assert.AreEqual(DayOfWeek.Wednesday, next.DayOfWeek);
    }

    #endregion

    #region ToString

    [TestMethod]
    public void ToString_IncludesDayTimeAndTimeZone()
    {
        RecurringSchedule schedule = new(DayOfWeek.Monday, new TimeOnly(9, 0), TimeZoneInfo.Utc);
        string result = schedule.ToString();

        StringAssert.Contains(result, "Monday");
        StringAssert.Contains(result, "UTC");
    }

    #endregion
}
