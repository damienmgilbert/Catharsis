using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="SequenceTopN"/> class.
///</summary>
[TestClass]
public class SequenceTopNTests
{
    #region TopN

    [TestMethod]
    public void TopN_NullSource_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).TopN(3));
    }

    [TestMethod]
    public void TopN_ZeroOrNegativeCount_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new[] { 1 }.TopN(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new[] { 1 }.TopN(-1));
    }

    [TestMethod]
    public void TopN_ReturnsLargestElementsDescending()
    {
        int[] values = [5, 3, 8, 1, 9, 2, 7];
        IReadOnlyList<int> result = values.TopN(3);
        CollectionAssert.AreEqual(new[] { 9, 8, 7 }, result.ToList());
    }

    [TestMethod]
    public void TopN_CountExceedsSourceLength_ReturnsAllSorted()
    {
        int[] values = [5, 3, 8];
        IReadOnlyList<int> result = values.TopN(10);
        CollectionAssert.AreEqual(new[] { 8, 5, 3 }, result.ToList());
    }

    [TestMethod]
    public void TopN_EmptySource_ReturnsEmpty()
    {
        Assert.IsEmpty(Array.Empty<int>().TopN(5));
    }

    [TestMethod]
    public void TopN_CustomComparer_ReversesOrder()
    {
        int[] values = [5, 3, 8, 1, 9];
        IReadOnlyList<int> result = values.TopN(2, Comparer<int>.Create(static (a, b) => b.CompareTo(a)));
        CollectionAssert.AreEqual(new[] { 1, 3 }, result.ToList());
    }

    #endregion

    #region BottomN

    [TestMethod]
    public void BottomN_NullSource_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).BottomN(3));
    }

    [TestMethod]
    public void BottomN_ZeroOrNegativeCount_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new[] { 1 }.BottomN(0));
    }

    [TestMethod]
    public void BottomN_ReturnsSmallestElementsAscending()
    {
        int[] values = [5, 3, 8, 1, 9, 2, 7];
        IReadOnlyList<int> result = values.BottomN(3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.ToList());
    }

    [TestMethod]
    public void BottomN_CountExceedsSourceLength_ReturnsAllSorted()
    {
        int[] values = [5, 3, 8];
        IReadOnlyList<int> result = values.BottomN(10);
        CollectionAssert.AreEqual(new[] { 3, 5, 8 }, result.ToList());
    }

    #endregion
}
