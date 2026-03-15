using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="SortedList{T}"/> class.
///</summary>
[TestClass]
public class SortedListTests
{
    [TestMethod]
    public void Add_MaintainsSortedOrder()
    {
        SortedList<int> list = new SortedList<int>();
        list.Add(3);
        list.Add(1);
        list.Add(2);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToList());
    }

    [TestMethod]
    public void Add_DuplicatesArePermitted()
    {
        SortedList<int> list = new SortedList<int>();
        list.Add(1);
        list.Add(1);
        Assert.HasCount(2, list);
    }

    [TestMethod]
    public void Indexer_ReturnsSortedElement()
    {
        SortedList<string> list = new SortedList<string>();
        list.Add("banana");
        list.Add("apple");
        list.Add("cherry");
        Assert.AreEqual("apple", list[0]);
        Assert.AreEqual("banana", list[1]);
        Assert.AreEqual("cherry", list[2]);
    }

    [TestMethod]
    public void Contains_ExistingItem_ReturnsTrue()
    {
        SortedList<int> list = new SortedList<int>();
        list.Add(5);
        list.Add(10);
        Assert.IsTrue(list.Contains(5));
    }

    [TestMethod]
    public void Contains_MissingItem_ReturnsFalse()
    {
        SortedList<int> list = new SortedList<int>();
        list.Add(5);
        Assert.IsFalse(list.Contains(99));
    }

    [TestMethod]
    public void Remove_ExistingItem_ReturnsTrue()
    {
        SortedList<int> list = new SortedList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        Assert.IsTrue(list.Remove(2));
        Assert.HasCount(2, list);
        Assert.IsFalse(list.Contains(2));
    }

    [TestMethod]
    public void Remove_MissingItem_ReturnsFalse()
    {
        SortedList<int> list = new SortedList<int>();
        list.Add(1);
        Assert.IsFalse(list.Remove(99));
    }

    [TestMethod]
    public void IndexOf_ExistingItem_ReturnsIndex()
    {
        SortedList<int> list = new SortedList<int>();
        list.Add(10);
        list.Add(20);
        list.Add(30);
        Assert.AreEqual(1, list.IndexOf(20));
    }

    [TestMethod]
    public void IndexOf_MissingItem_ReturnsNegativeOne()
    {
        SortedList<int> list = new SortedList<int>();
        list.Add(10);
        Assert.AreEqual(-1, list.IndexOf(99));
    }

    [TestMethod]
    public void Clear_RemovesAllItems()
    {
        SortedList<int> list = new SortedList<int>();
        list.Add(1);
        list.Add(2);
        list.Clear();
        Assert.IsEmpty(list);
    }

    [TestMethod]
    public void Constructor_CustomComparer_RespectsSortOrder()
    {
        SortedList<int> list = new SortedList<int>(Comparer<int>.Create(static (a, b) => b.CompareTo(a)));
        list.Add(1);
        list.Add(3);
        list.Add(2);
        CollectionAssert.AreEqual(new[] { 3, 2, 1 }, list.ToList());
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => new SortedList<int>(null!));
    }

    [TestMethod]
    public void CopyTo_CopiesToArray()
    {
        SortedList<int> list = new SortedList<int>();
        list.Add(3);
        list.Add(1);
        list.Add(2);
        int[] array = new int[3];
        list.CopyTo(array, 0);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, array);
    }

    [TestMethod]
    public void IsReadOnly_ReturnsFalse()
    {
        SortedList<int> list = new SortedList<int>();
        Assert.IsFalse(list.IsReadOnly);
    }
}
