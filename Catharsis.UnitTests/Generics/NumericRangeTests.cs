using Catharsis.Generics;

namespace Catharsis.UnitTests.Generics;

///<summary>
///Unit tests for the <see cref="NumericRange{T}"/> struct.
///</summary>
[TestClass]
public class NumericRangeTests
{
    [TestMethod]
    public void Constructor_EndBeforeStart_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new NumericRange<int>(5, 1)); }

    [TestMethod]
    public void Contains_Value_IsInclusiveAtBothEnds()
    {
        NumericRange<int> range = new(1, 5);
        Assert.IsTrue(range.Contains(1));
        Assert.IsTrue(range.Contains(5));
        Assert.IsFalse(range.Contains(0));
        Assert.IsFalse(range.Contains(6));
    }

    [TestMethod]
    public void Contains_Range_RequiresFullCoverage()
    {
        NumericRange<int> range = new(1, 10);
        Assert.IsTrue(range.Contains(new NumericRange<int>(2, 3)));
        Assert.IsFalse(range.Contains(new NumericRange<int>(5, 11)));
    }

    [TestMethod]
    public void Overlaps_TouchingEndpoints_Overlap()
    {
        Assert.IsTrue(new NumericRange<int>(1, 5).Overlaps(new NumericRange<int>(5, 9)));
        Assert.IsFalse(new NumericRange<int>(1, 4).Overlaps(new NumericRange<int>(5, 9)));
    }

    [TestMethod]
    public void TryIntersect_Overlapping_ReturnsShared()
    {
        Assert.IsTrue(new NumericRange<int>(1, 7).TryIntersect(new NumericRange<int>(5, 9), out NumericRange<int> shared));
        Assert.AreEqual(new NumericRange<int>(5, 7), shared);
        Assert.IsFalse(new NumericRange<int>(1, 2).TryIntersect(new NumericRange<int>(5, 9), out _));
    }

    [TestMethod]
    public void Span_CoversBoth() { Assert.AreEqual(new NumericRange<int>(1, 9), new NumericRange<int>(1, 2).Span(new NumericRange<int>(5, 9))); }

    [TestMethod]
    public void Clamp_LimitsToBounds()
    {
        NumericRange<double> range = new(0, 1);
        Assert.AreEqual(0d, range.Clamp(-3));
        Assert.AreEqual(1d, range.Clamp(3));
        Assert.AreEqual(0.5, range.Clamp(0.5));
    }

    [TestMethod]
    public void Remap_MapsProportionally() { Assert.AreEqual(50d, new NumericRange<double>(0, 10).Remap(5, new NumericRange<double>(0, 100))); }

    [TestMethod]
    public void Remap_ZeroLengthSource_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => new NumericRange<int>(3, 3).Remap(3, new NumericRange<int>(0, 10))); }

    [TestMethod]
    public void Step_EnumeratesInclusive() { CollectionAssert.AreEqual(new[] { 0, 3, 6, 9 }, new NumericRange<int>(0, 10).Step(3).ToArray()); }

    [TestMethod]
    public void Step_NonPositive_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new NumericRange<int>(0, 10).Step(0)); }

    [TestMethod]
    public void Works_ForDecimal_AndReportsLength()
    {
        NumericRange<decimal> range = new(1.5m, 4m);
        Assert.AreEqual(2.5m, range.Length);
        Assert.IsTrue(range.Contains(2m));
    }
}
