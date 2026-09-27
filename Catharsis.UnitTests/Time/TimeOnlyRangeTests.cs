using Catharsis.Time;

namespace Catharsis.UnitTests.Time;

///<summary>
///Unit tests for the <see cref="TimeOnlyRange"/> struct.
///</summary>
[TestClass]
public class TimeOnlyRangeTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_StartAfterEnd_Swaps()
    {
        TimeOnlyRange range = new(new TimeOnly(17, 0), new TimeOnly(9, 0));
        Assert.AreEqual(new TimeOnly(9, 0), range.Start);
        Assert.AreEqual(new TimeOnly(17, 0), range.End);
    }

    #endregion

    #region Contains / Duration

    [TestMethod]
    public void Contains_ValueWithinRange_ReturnsTrue()
    {
        TimeOnlyRange range = new(new TimeOnly(9, 0), new TimeOnly(17, 0));
        Assert.IsTrue(range.Contains(new TimeOnly(12, 0)));
    }

    [TestMethod]
    public void Contains_ValueOutsideRange_ReturnsFalse()
    {
        TimeOnlyRange range = new(new TimeOnly(9, 0), new TimeOnly(17, 0));
        Assert.IsFalse(range.Contains(new TimeOnly(18, 0)));
    }

    [TestMethod]
    public void Duration_ReturnsSpanBetweenBounds()
    {
        TimeOnlyRange range = new(new TimeOnly(9, 0), new TimeOnly(17, 30));
        Assert.AreEqual(TimeSpan.FromHours(8.5), range.Duration);
    }

    #endregion

    #region Overlaps / Intersect / Union

    [TestMethod]
    public void Overlaps_OverlappingRanges_ReturnsTrue()
    {
        TimeOnlyRange first = new(new TimeOnly(9, 0), new TimeOnly(13, 0));
        TimeOnlyRange second = new(new TimeOnly(12, 0), new TimeOnly(17, 0));
        Assert.IsTrue(first.Overlaps(second));
    }

    [TestMethod]
    public void Overlaps_NonOverlappingRanges_ReturnsFalse()
    {
        TimeOnlyRange first = new(new TimeOnly(9, 0), new TimeOnly(11, 0));
        TimeOnlyRange second = new(new TimeOnly(12, 0), new TimeOnly(17, 0));
        Assert.IsFalse(first.Overlaps(second));
    }

    [TestMethod]
    public void Intersect_OverlappingRanges_ReturnsOverlappingPortion()
    {
        TimeOnlyRange first = new(new TimeOnly(9, 0), new TimeOnly(13, 0));
        TimeOnlyRange second = new(new TimeOnly(12, 0), new TimeOnly(17, 0));

        TimeOnlyRange? result = first.Intersect(second);

        Assert.AreEqual(new TimeOnlyRange(new TimeOnly(12, 0), new TimeOnly(13, 0)), result);
    }

    [TestMethod]
    public void Intersect_NonOverlappingRanges_ReturnsNull()
    {
        TimeOnlyRange first = new(new TimeOnly(9, 0), new TimeOnly(11, 0));
        TimeOnlyRange second = new(new TimeOnly(12, 0), new TimeOnly(17, 0));

        Assert.IsNull(first.Intersect(second));
    }

    [TestMethod]
    public void Union_ReturnsSpanningRange()
    {
        TimeOnlyRange first = new(new TimeOnly(9, 0), new TimeOnly(11, 0));
        TimeOnlyRange second = new(new TimeOnly(12, 0), new TimeOnly(17, 0));

        TimeOnlyRange result = first.Union(second);

        Assert.AreEqual(new TimeOnlyRange(new TimeOnly(9, 0), new TimeOnly(17, 0)), result);
    }

    #endregion

    #region Equality

    [TestMethod]
    public void Equals_SameBounds_ReturnsTrue()
    {
        TimeOnlyRange first = new(new TimeOnly(9, 0), new TimeOnly(17, 0));
        TimeOnlyRange second = new(new TimeOnly(9, 0), new TimeOnly(17, 0));

        Assert.IsTrue(first == second);
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
    }

    [TestMethod]
    public void Equals_DifferentBounds_ReturnsFalse()
    {
        TimeOnlyRange first = new(new TimeOnly(9, 0), new TimeOnly(17, 0));
        TimeOnlyRange second = new(new TimeOnly(9, 0), new TimeOnly(18, 0));

        Assert.IsTrue(first != second);
    }

    #endregion
}
