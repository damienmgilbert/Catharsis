using Catharsis.HighPerformance;

namespace Catharsis.UnitTests.HighPerformance;

///<summary>
///Unit tests for the <see cref="PooledList"/> class.
///</summary>
[TestClass]
public class PooledListTests
{
    #region Public methods
    [TestMethod]
    public void Add_GrowsAutomatically()
    {
        using PooledList<int> list = new PooledList<int>(2);
        for (int i = 0; i < 100; i++)
        {
            list.Add(i);
        }

        Assert.HasCount(100, list);
    }

    [TestMethod]
    public void Add_IncreasesCount()
    {
        using PooledList<int> list = new PooledList<int>();
        list.Add(1);
        list.Add(2);
        Assert.HasCount(2, list);
        Assert.AreEqual(1, list[0]);
        Assert.AreEqual(2, list[1]);
    }

    [TestMethod]
    public void AddRange_AddsMultipleItems()
    {
        using PooledList<int> list = new PooledList<int>();
        list.AddRange([1, 2, 3]);
        Assert.HasCount(3, list);
    }

    [TestMethod]
    public void Clear_ResetsCount()
    {
        using PooledList<int> list = new PooledList<int>();
        list.Add(1);
        list.Add(2);
        list.Clear();
        Assert.IsEmpty(list);
    }

    [TestMethod]
    public void Constructor_Default_CreatesEmptyList()
    {
        using PooledList<int> list = new PooledList<int>();
        Assert.IsEmpty(list);
        Assert.IsGreaterThanOrEqualTo(16, list.Capacity);
    }

    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        using PooledList<string> list = new PooledList<string>();
        list.Add("hello");
        Assert.IsTrue(list.Contains("hello"));
        Assert.IsFalse(list.Contains("world"));
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        PooledList<int> list = new PooledList<int>();
        list.Dispose();
        list.Dispose();
    }

    [TestMethod]
    public void Enumeration_ReturnsAllItems()
    {
        using PooledList<int> list = new PooledList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToList());
    }

    [TestMethod]
    public void Indexer_SetValue()
    {
        using PooledList<int> list = new PooledList<int>();
        list.Add(1);
        list[0] = 42;
        Assert.AreEqual(42, list[0]);
    }

    [TestMethod]
    public void IndexOf_ReturnsCorrectIndex()
    {
        using PooledList<int> list = new PooledList<int>();
        list.Add(10);
        list.Add(20);
        Assert.AreEqual(1, list.IndexOf(20));
        Assert.AreEqual(-1, list.IndexOf(99));
    }

    [TestMethod]
    public void Insert_InsertsAtIndex()
    {
        using PooledList<int> list = new PooledList<int>();
        list.Add(1);
        list.Add(3);
        list.Insert(1, 2);
        Assert.HasCount(3, list);
        Assert.AreEqual(2, list[1]);
    }

    [TestMethod]
    public void Remove_RemovesItem()
    {
        using PooledList<int> list = new PooledList<int>();
        list.Add(1);
        list.Add(2);
        Assert.IsTrue(list.Remove(1));
        Assert.HasCount(1, list);
    }

    [TestMethod]
    public void RemoveAt_RemovesAtIndex()
    {
        using PooledList<int> list = new PooledList<int>();
        list.Add(10);
        list.Add(20);
        list.Add(30);
        list.RemoveAt(1);
        Assert.HasCount(2, list);
        Assert.AreEqual(30, list[1]);
    }
    #endregion
}
