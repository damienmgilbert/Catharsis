using System.Collections;
using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="ReadOnlyListAdapter{T}"/> class.
///</summary>
[TestClass]
public class ReadOnlyListAdapterTests
{
    [TestMethod]
    public void Constructor_NullSource_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => new ReadOnlyListAdapter<int>(null!));
    }

    [TestMethod]
    public void Count_ReflectsSourceCount()
    {
        List<int> source = [1, 2, 3];
        ReadOnlyListAdapter<int> adapter = new(source);
        Assert.AreEqual(3, adapter.Count);
    }

    [TestMethod]
    public void Indexer_ReturnsCorrectElement()
    {
        List<string> source = ["a", "b", "c"];
        ReadOnlyListAdapter<string> adapter = new(source);
        Assert.AreEqual("a", adapter[0]);
        Assert.AreEqual("b", adapter[1]);
        Assert.AreEqual("c", adapter[2]);
    }

    [TestMethod]
    public void Contains_ExistingItem_ReturnsTrue()
    {
        List<int> source = [10, 20, 30];
        ReadOnlyListAdapter<int> adapter = new(source);
        Assert.IsTrue(adapter.Contains(20));
    }

    [TestMethod]
    public void Contains_MissingItem_ReturnsFalse()
    {
        List<int> source = [10, 20, 30];
        ReadOnlyListAdapter<int> adapter = new(source);
        Assert.IsFalse(adapter.Contains(99));
    }

    [TestMethod]
    public void Enumeration_ReturnsAllItems()
    {
        List<int> source = [1, 2, 3];
        ReadOnlyListAdapter<int> adapter = new(source);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, adapter.ToList());
    }

    [TestMethod]
    public void ReflectsSourceChanges()
    {
        List<int> source = [1, 2];
        ReadOnlyListAdapter<int> adapter = new(source);
        source.Add(3);
        Assert.AreEqual(3, adapter.Count);
        Assert.AreEqual(3, adapter[2]);
    }

    [TestMethod]
    public void ICollection_CopyTo_CopiesToArray()
    {
        List<int> source = [1, 2, 3];
        ICollection adapter = new ReadOnlyListAdapter<int>(source);
        int[] array = new int[3];
        adapter.CopyTo(array, 0);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, array);
    }

    [TestMethod]
    public void ICollection_CopyTo_NullArray_Throws()
    {
        ICollection adapter = new ReadOnlyListAdapter<int>([1]);
        Assert.ThrowsExactly<ArgumentNullException>(() => adapter.CopyTo(null!, 0));
    }

    [TestMethod]
    public void ICollection_CopyTo_NegativeIndex_Throws()
    {
        ICollection adapter = new ReadOnlyListAdapter<int>([1]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => adapter.CopyTo(new int[1], -1));
    }

    [TestMethod]
    public void ICollection_CopyTo_InsufficientSpace_Throws()
    {
        ICollection adapter = new ReadOnlyListAdapter<int>([1, 2, 3]);
        Assert.ThrowsExactly<ArgumentException>(() => adapter.CopyTo(new int[2], 0));
    }

    [TestMethod]
    public void ICollection_IsSynchronized_ReturnsFalse()
    {
        ICollection adapter = new ReadOnlyListAdapter<int>([1]);
        Assert.IsFalse(adapter.IsSynchronized);
    }

    [TestMethod]
    public void NonGenericEnumerator_EnumeratesItems()
    {
        List<int> source = [1, 2, 3];
        IEnumerable adapter = new ReadOnlyListAdapter<int>(source);
        List<object?> items = [];

        foreach(object? item in adapter)
        {
            items.Add(item);
        }

        Assert.HasCount(3, items);
    }
}
