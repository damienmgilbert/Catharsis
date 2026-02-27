using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

[TestClass]
public class IntervalTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NormalizesOrder()
    {
        Interval<int> interval = new Interval<int>(10, 5);
        Assert.AreEqual(5, interval.Start);
        Assert.AreEqual(10, interval.End);
    }

    [TestMethod]
    public void Contains_ValueInRange_ReturnsTrue()
    {
        Interval<int> interval = new Interval<int>(1, 10);
        Assert.IsTrue(interval.Contains(5));
        Assert.IsTrue(interval.Contains(1));
        Assert.IsTrue(interval.Contains(10));
        Assert.IsFalse(interval.Contains(0));
        Assert.IsFalse(interval.Contains(11));
    }

    [TestMethod]
    public void Equals_DifferentInterval_ReturnsFalse()
    {
        Interval<int> a = new Interval<int>(1, 5);
        Interval<int> b = new Interval<int>(1, 6);
        Assert.IsFalse(a.Equals(b));
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Equals_SameInterval_ReturnsTrue()
    {
        Interval<int> a = new Interval<int>(1, 5);
        Interval<int> b = new Interval<int>(1, 5);
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void GetHashCode_EqualIntervals_SameHash()
    {
        Interval<int> a = new Interval<int>(1, 5);
        Interval<int> b = new Interval<int>(1, 5);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Intersect_NonOverlapping_ReturnsNull()
    {
        Interval<int> a = new Interval<int>(1, 3);
        Interval<int> b = new Interval<int>(5, 8);
        Assert.IsNull(a.Intersect(b));
    }

    [TestMethod]
    public void Intersect_OverlappingIntervals_ReturnsIntersection()
    {
        Interval<int> a = new Interval<int>(1, 5);
        Interval<int> b = new Interval<int>(3, 8);
        Interval<int>? result = a.Intersect(b);
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Value.Start);
        Assert.AreEqual(5, result.Value.End);
    }

    [TestMethod]
    public void Overlaps_NonOverlapping_ReturnsFalse()
    {
        Interval<int> a = new Interval<int>(1, 3);
        Interval<int> b = new Interval<int>(5, 8);
        Assert.IsFalse(a.Overlaps(b));
    }

    [TestMethod]
    public void Overlaps_OverlappingIntervals_ReturnsTrue()
    {
        Interval<int> a = new Interval<int>(1, 5);
        Interval<int> b = new Interval<int>(3, 8);
        Assert.IsTrue(a.Overlaps(b));
    }

    [TestMethod]
    public void ToString_ReturnsCorrectFormat()
    {
        Interval<int> interval = new Interval<int>(1, 5);
        Assert.AreEqual("[1, 5]", interval.ToString());
    }

    [TestMethod]
    public void Union_ReturnsSmallestCoveringInterval()
    {
        Interval<int> a = new Interval<int>(1, 5);
        Interval<int> b = new Interval<int>(3, 8);
        Interval<int> result = a.Union(b);
        Assert.AreEqual(1, result.Start);
        Assert.AreEqual(8, result.End);
    }
    #endregion
}
