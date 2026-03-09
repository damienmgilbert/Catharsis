using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="CollectionExtensions"/> class.
///</summary>
[TestClass]
public class CollectionExtensionsTests
{
    #region Public methods
    [TestMethod]
    public void AddRange_AddsAllItems_CollectionContainsAll()
    {
        ICollection<int> source = new List<int> { 1, 2 };
        source.AddRange(new[] { 3, 4, 5 });
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, source.ToList());
    }

    [TestMethod]
    public void AddRange_ReturnsSameInstance_ForFluentChaining()
    {
        ICollection<int> source = new List<int>();
        ICollection<int> result = source.AddRange(new[] { 1 });
        Assert.AreSame(source, result);
    }

    [TestMethod]
    public void InsertRange_InsertsAtIndex_ElementsInOrder()
    {
        IList<int> source = new List<int> { 1, 5 };
        source.InsertRange(1, new[] { 2, 3, 4 });
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, source.ToList());
    }

    [TestMethod]
    public void ModifyAll_TransformsAllElements()
    {
        IList<int> source = new List<int> { 1, 2, 3 };
        source.ModifyAll(static x => x * 10);
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, source.ToList());
    }

    [TestMethod]
    public void ModifyWhere_TransformsMatchingOnly()
    {
        IList<int> source = new List<int> { 1, 2, 3, 4, 5 };
        source.ModifyWhere(static x => x % 2 == 0, static x => x * 10);
        CollectionAssert.AreEqual(new[] { 1, 20, 3, 40, 5 }, source.ToList());
    }

    [TestMethod]
    public void NullSource_ThrowsArgumentNullException()
    {
        ICollection<int>? source = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.AddRange(new[] { 1 }));
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.RemoveRange(new[] { 1 }));
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.RemoveWhere(x => true));
    }

    [TestMethod]
    public void RemoveRange_ByIndexAndCount_RemovesElements()
    {
        IList<int> source = new List<int> { 1, 2, 3, 4, 5 };
        source.RemoveRange(1, 2);
        CollectionAssert.AreEqual(new[] { 1, 4, 5 }, source.ToList());
    }

    [TestMethod]
    public void RemoveRange_InvalidRange_ThrowsArgumentOutOfRange()
    {
        IList<int> source = new List<int> { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.RemoveRange(2, 5));
    }

    [TestMethod]
    public void RemoveRange_RemovesMatchingItems_ReturnsCount()
    {
        ICollection<int> source = new List<int> { 1, 2, 3, 4, 5 };
        int removed = source.RemoveRange(new[] { 2, 4, 99 });
        Assert.AreEqual(2, removed);
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, source.ToList());
    }

    [TestMethod]
    public void RemoveWhere_NoMatch_ReturnsZero()
    {
        ICollection<int> source = new List<int> { 1, 3, 5 };
        int removed = source.RemoveWhere(static x => x % 2 == 0);
        Assert.AreEqual(0, removed);
    }

    [TestMethod]
    public void RemoveWhere_RemovesMatchingItems_ReturnsCount()
    {
        ICollection<int> source = new List<int> { 1, 2, 3, 4, 5, 6 };
        int removed = source.RemoveWhere(static x => x % 2 == 0);
        Assert.AreEqual(3, removed);
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, source.ToList());
    }

    [TestMethod]
    public void ReplaceAll_NoMatch_ReturnsZero()
    {
        IList<int> source = new List<int> { 1, 2, 3 };
        int count = source.ReplaceAll(99, 0);
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void ReplaceAll_ReplacesAllOccurrences_ReturnsCount()
    {
        IList<int> source = new List<int> { 1, 2, 1, 3, 1 };
        int count = source.ReplaceAll(1, 99);
        Assert.AreEqual(3, count);
        CollectionAssert.AreEqual(new[] { 99, 2, 99, 3, 99 }, source.ToList());
    }

    [TestMethod]
    public void ReplaceAt_InvalidIndex_ThrowsArgumentOutOfRange()
    {
        IList<int> source = new List<int> { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.ReplaceAt(5, 99));
    }

    [TestMethod]
    public void ReplaceAt_ValidIndex_ReplacesElement()
    {
        IList<string> source = new List<string> { "a", "b", "c" };
        source.ReplaceAt(1, "X");
        CollectionAssert.AreEqual(new[] { "a", "X", "c" }, source.ToList());
    }

    [TestMethod]
    public void Swap_InvalidIndices_ThrowsArgumentOutOfRange()
    {
        IList<int> source = new List<int> { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.Swap(-1, 2));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.Swap(0, 5));
    }

    [TestMethod]
    public void Swap_SwapsElements_AtGivenIndices()
    {
        IList<int> source = new List<int> { 1, 2, 3 };
        source.Swap(0, 2);
        CollectionAssert.AreEqual(new[] { 3, 2, 1 }, source.ToList());
    }
    #endregion
}
