using Catharsis.Scheduling;

namespace Catharsis.UnitTests.Scheduling;

///<summary>
///Unit tests for the <see cref="CronExpression"/> class.
///</summary>
[TestClass]
public class CronExpressionTests
{
    #region Parse / TryParse

    [TestMethod]
    public void Parse_TooFewFields_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() => CronExpression.Parse("* * *"));
    }

    [TestMethod]
    public void Parse_TooManyFields_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() => CronExpression.Parse("* * * * * *"));
    }

    [TestMethod]
    public void Parse_Null_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => CronExpression.Parse(null!));
    }

    [TestMethod]
    public void Parse_Whitespace_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CronExpression.Parse("   "));
    }

    [TestMethod]
    public void Parse_ValueOutOfRange_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() => CronExpression.Parse("60 * * * *"));
    }

    [TestMethod]
    public void Parse_InvalidRange_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() => CronExpression.Parse("10-5 * * * *"));
    }

    [TestMethod]
    public void Parse_InvalidStep_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() => CronExpression.Parse("*/0 * * * *"));
    }

    [TestMethod]
    public void Parse_NonNumericValue_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() => CronExpression.Parse("abc * * * *"));
    }

    [TestMethod]
    public void Parse_Valid_ToStringReturnsOriginalExpression()
    {
        CronExpression cron = CronExpression.Parse("0 9 * * 1-5");
        Assert.AreEqual("0 9 * * 1-5", cron.ToString());
    }

    [TestMethod]
    public void TryParse_Valid_ReturnsTrueWithResult()
    {
        Assert.IsTrue(CronExpression.TryParse("* * * * *", out CronExpression? result));
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void TryParse_Invalid_ReturnsFalseWithNull()
    {
        Assert.IsFalse(CronExpression.TryParse("not a cron", out CronExpression? result));
        Assert.IsNull(result);
    }

    [TestMethod]
    public void TryParse_NullOrWhitespace_ReturnsFalse()
    {
        Assert.IsFalse(CronExpression.TryParse(null, out _));
        Assert.IsFalse(CronExpression.TryParse("  ", out _));
    }

    #endregion

    #region GetNextOccurrence

    [TestMethod]
    public void GetNextOccurrence_EveryMinute_ReturnsNextMinute()
    {
        CronExpression cron = CronExpression.Parse("* * * * *");
        DateTime after = new(2026, 1, 1, 10, 30, 15);
        DateTime next = cron.GetNextOccurrence(after);
        Assert.AreEqual(new DateTime(2026, 1, 1, 10, 31, 0), next);
    }

    [TestMethod]
    public void GetNextOccurrence_SpecificTime_ReturnsThatTimeNextDay()
    {
        CronExpression cron = CronExpression.Parse("0 9 * * *");
        DateTime after = new(2026, 1, 1, 10, 0, 0);
        DateTime next = cron.GetNextOccurrence(after);
        Assert.AreEqual(new DateTime(2026, 1, 2, 9, 0, 0), next);
    }

    [TestMethod]
    public void GetNextOccurrence_SpecificTimeLaterToday_ReturnsToday()
    {
        CronExpression cron = CronExpression.Parse("0 9 * * *");
        DateTime after = new(2026, 1, 1, 8, 0, 0);
        DateTime next = cron.GetNextOccurrence(after);
        Assert.AreEqual(new DateTime(2026, 1, 1, 9, 0, 0), next);
    }

    [TestMethod]
    public void GetNextOccurrence_DayOfWeekRange_SkipsWeekend()
    {
        CronExpression cron = CronExpression.Parse("0 9 * * 1-5");

        // 2026-01-03 is a Saturday.
        DateTime after = new(2026, 1, 3, 10, 0, 0);
        DateTime next = cron.GetNextOccurrence(after);

        // Next weekday at 9am is Monday 2026-01-05.
        Assert.AreEqual(new DateTime(2026, 1, 5, 9, 0, 0), next);
        Assert.AreEqual(DayOfWeek.Monday, next.DayOfWeek);
    }

    [TestMethod]
    public void GetNextOccurrence_CommaList_MatchesAnyListedValue()
    {
        CronExpression cron = CronExpression.Parse("0 9,17 * * *");
        DateTime after = new(2026, 1, 1, 10, 0, 0);
        DateTime next = cron.GetNextOccurrence(after);
        Assert.AreEqual(new DateTime(2026, 1, 1, 17, 0, 0), next);
    }

    [TestMethod]
    public void GetNextOccurrence_StepValue_MatchesEveryNthUnit()
    {
        CronExpression cron = CronExpression.Parse("*/15 * * * *");
        DateTime after = new(2026, 1, 1, 10, 1, 0);
        DateTime next = cron.GetNextOccurrence(after);
        Assert.AreEqual(new DateTime(2026, 1, 1, 10, 15, 0), next);
    }

    [TestMethod]
    public void GetNextOccurrence_DomAndDowBothRestricted_MatchesEither()
    {
        // Day 15 of the month, OR any Friday.
        CronExpression cron = CronExpression.Parse("0 0 15 * 5");

        // 2026-01-02 is a Friday.
        DateTime after = new(2026, 1, 1, 0, 0, 0);
        DateTime next = cron.GetNextOccurrence(after);

        Assert.AreEqual(new DateTime(2026, 1, 2, 0, 0, 0), next);
        Assert.AreEqual(DayOfWeek.Friday, next.DayOfWeek);
    }

    #endregion

    #region GetNextOccurrences

    [TestMethod]
    public void GetNextOccurrences_ReturnsRequestedCountInOrder()
    {
        CronExpression cron = CronExpression.Parse("0 * * * *");
        DateTime after = new(2026, 1, 1, 0, 0, 0);

        List<DateTime> occurrences = [.. cron.GetNextOccurrences(after, 3)];

        Assert.HasCount(3, occurrences);
        Assert.AreEqual(new DateTime(2026, 1, 1, 1, 0, 0), occurrences[0]);
        Assert.AreEqual(new DateTime(2026, 1, 1, 2, 0, 0), occurrences[1]);
        Assert.AreEqual(new DateTime(2026, 1, 1, 3, 0, 0), occurrences[2]);
    }

    [TestMethod]
    public void GetNextOccurrences_ZeroOrNegativeCount_Throws()
    {
        CronExpression cron = CronExpression.Parse("* * * * *");
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => cron.GetNextOccurrences(DateTime.Now, 0).ToList());
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => cron.GetNextOccurrences(DateTime.Now, -1).ToList());
    }

    #endregion
}
