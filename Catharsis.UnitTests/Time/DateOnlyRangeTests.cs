using Catharsis.Time;

namespace Catharsis.UnitTests.Time;

///<summary>
///Unit tests for the <see cref="DateOnlyRange"/> struct.
///</summary>
[TestClass]
public class DateOnlyRangeTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_StartAfterEnd_Swaps()
    {
        DateOnlyRange range = new(new DateOnly(2024, 1, 10), new DateOnly(2024, 1, 1));
        Assert.AreEqual(new DateOnly(2024, 1, 1), range.Start);
        Assert.AreEqual(new DateOnly(2024, 1, 10), range.End);
    }

    #endregion

    #region Contains / DayCount

    [TestMethod]
    public void Contains_ValueWithinRange_ReturnsTrue()
    {
        DateOnlyRange range = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        Assert.IsTrue(range.Contains(new DateOnly(2024, 1, 5)));
    }

    [TestMethod]
    public void Contains_ValueOutsideRange_ReturnsFalse()
    {
        DateOnlyRange range = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        Assert.IsFalse(range.Contains(new DateOnly(2024, 1, 11)));
    }

    [TestMethod]
    public void DayCount_InclusiveOfBothEndpoints()
    {
        DateOnlyRange range = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        Assert.AreEqual(10, range.DayCount);
    }

    [TestMethod]
    public void DayCount_SingleDay_ReturnsOne()
    {
        DateOnlyRange range = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 1));
        Assert.AreEqual(1, range.DayCount);
    }

    #endregion

    #region Overlaps / Intersect / Union

    [TestMethod]
    public void Overlaps_OverlappingRanges_ReturnsTrue()
    {
        DateOnlyRange first = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        DateOnlyRange second = new(new DateOnly(2024, 1, 5), new DateOnly(2024, 1, 15));
        Assert.IsTrue(first.Overlaps(second));
    }

    [TestMethod]
    public void Overlaps_NonOverlappingRanges_ReturnsFalse()
    {
        DateOnlyRange first = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 5));
        DateOnlyRange second = new(new DateOnly(2024, 1, 10), new DateOnly(2024, 1, 15));
        Assert.IsFalse(first.Overlaps(second));
    }

    [TestMethod]
    public void Intersect_OverlappingRanges_ReturnsOverlappingPortion()
    {
        DateOnlyRange first = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        DateOnlyRange second = new(new DateOnly(2024, 1, 5), new DateOnly(2024, 1, 15));

        DateOnlyRange? result = first.Intersect(second);

        Assert.AreEqual(new DateOnlyRange(new DateOnly(2024, 1, 5), new DateOnly(2024, 1, 10)), result);
    }

    [TestMethod]
    public void Intersect_NonOverlappingRanges_ReturnsNull()
    {
        DateOnlyRange first = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 5));
        DateOnlyRange second = new(new DateOnly(2024, 1, 10), new DateOnly(2024, 1, 15));

        Assert.IsNull(first.Intersect(second));
    }

    [TestMethod]
    public void Union_ReturnsSpanningRange()
    {
        DateOnlyRange first = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 5));
        DateOnlyRange second = new(new DateOnly(2024, 1, 10), new DateOnly(2024, 1, 15));

        DateOnlyRange result = first.Union(second);

        Assert.AreEqual(new DateOnlyRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 15)), result);
    }

    #endregion

    #region ToDates

    [TestMethod]
    public void ToDates_EnumeratesEveryDateInclusive()
    {
        DateOnlyRange range = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 3));
        List<DateOnly> dates = [.. range.ToDates()];

        CollectionAssert.AreEqual(new[] { new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 2), new DateOnly(2024, 1, 3) }, dates);
    }

    #endregion

    #region Equality

    [TestMethod]
    public void Equals_SameBounds_ReturnsTrue()
    {
        DateOnlyRange first = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        DateOnlyRange second = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));

        Assert.IsTrue(first == second);
        Assert.IsTrue(first.Equals(second));
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
    }

    [TestMethod]
    public void Equals_DifferentBounds_ReturnsFalse()
    {
        DateOnlyRange first = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        DateOnlyRange second = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 11));

        Assert.IsTrue(first != second);
    }

    #endregion
}
