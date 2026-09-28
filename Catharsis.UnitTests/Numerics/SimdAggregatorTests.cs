using Catharsis.Numerics;

namespace Catharsis.UnitTests.Numerics;

///<summary>
///Unit tests for the <see cref="SimdAggregator"/> class.
///</summary>
[TestClass]
public class SimdAggregatorTests
{
    #region Sum

    [TestMethod]
    public void Sum_EmptySpan_ReturnsZero() { Assert.AreEqual(0, SimdAggregator.Sum<int>([])); }

    [TestMethod]
    public void Sum_FewerElementsThanVectorWidth_ReturnsCorrectSum() { Assert.AreEqual(6, SimdAggregator.Sum<int>([1, 2, 3])); }

    [TestMethod]
    public void Sum_ManyElements_SpanningMultipleVectorWidths_ReturnsCorrectSum()
    {
        int[] values = [.. Enumerable.Range(1, 1000)];
        Assert.AreEqual(values.Sum(), SimdAggregator.Sum<int>(values));
    }

    [TestMethod]
    public void Sum_DoubleValues_ReturnsCorrectSum()
    {
        double[] values = [1.5, 2.5, 3.0];
        Assert.AreEqual(7.0, SimdAggregator.Sum<double>(values));
    }

    #endregion

    #region Min / Max

    [TestMethod]
    public void Min_EmptySpan_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => SimdAggregator.Min<int>([])); }

    [TestMethod]
    public void Min_ManyElements_ReturnsSmallestValue()
    {
        int[] values = [.. Enumerable.Range(1, 1000).Reverse()];
        Assert.AreEqual(1, SimdAggregator.Min<int>(values));
    }

    [TestMethod]
    public void Max_EmptySpan_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => SimdAggregator.Max<int>([])); }

    [TestMethod]
    public void Max_ManyElements_ReturnsLargestValue()
    {
        int[] values = [.. Enumerable.Range(1, 1000)];
        Assert.AreEqual(1000, SimdAggregator.Max<int>(values));
    }

    [TestMethod]
    public void Min_SingleElement_ReturnsThatElement() { Assert.AreEqual(42, SimdAggregator.Min<int>([42])); }

    [TestMethod]
    public void Max_NegativeValues_ReturnsLeastNegative() { Assert.AreEqual(-1, SimdAggregator.Max<int>([-5, -1, -10])); }

    #endregion

    #region Dot

    [TestMethod]
    public void Dot_MismatchedLengths_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => SimdAggregator.Dot<int>([1, 2], [1, 2, 3])); }

    [TestMethod]
    public void Dot_SmallVectors_ReturnsCorrectProduct() { Assert.AreEqual(32, SimdAggregator.Dot<int>([1, 2, 3], [4, 5, 6])); }

    [TestMethod]
    public void Dot_ManyElements_SpanningMultipleVectorWidths_ReturnsCorrectProduct()
    {
        int[] left = [.. Enumerable.Repeat(2, 1000)];
        int[] right = [.. Enumerable.Repeat(3, 1000)];

        Assert.AreEqual(6000, SimdAggregator.Dot<int>(left, right));
    }

    [TestMethod]
    public void Dot_EmptySpans_ReturnsZero() { Assert.AreEqual(0, SimdAggregator.Dot<int>([], [])); }

    #endregion
}
