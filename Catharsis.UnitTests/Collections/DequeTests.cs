using Catharsis;
using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="Deque"/> class.
///</summary>
[TestClass]
public class DequeTests
{
    [TestMethod]
    public void Constructor_Default_CreatesEmptyDeque()
    {
        Deque<int> d = new();
        Assert.IsEmpty(d);
        Assert.IsTrue(d.IsEmpty);
    }

    [TestMethod]
    public void Constructor_NegativeCapacity_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new Deque<int>(-1));
    }

    [TestMethod]
    public void AddFirst_AddsToFront()
    {
        Deque<int> d = new();
        d.AddFirst(1); d.AddFirst(2);
        Assert.AreEqual(2, d.PeekFirst());
        Assert.AreEqual(1, d.PeekLast());
    }

    [TestMethod]
    public void AddLast_AddsToBack()
    {
        Deque<int> d = new();
        d.AddLast(1); d.AddLast(2);
        Assert.AreEqual(1, d.PeekFirst());
        Assert.AreEqual(2, d.PeekLast());
    }

    [TestMethod]
    public void RemoveFirst_RemovesFromFront()
    {
        Deque<int> d = new();
        d.AddLast(1); d.AddLast(2); d.AddLast(3);
        Assert.AreEqual(1, d.RemoveFirst());
        Assert.HasCount(2, d);
    }

    [TestMethod]
    public void RemoveLast_RemovesFromBack()
    {
        Deque<int> d = new();
        d.AddLast(1); d.AddLast(2); d.AddLast(3);
        Assert.AreEqual(3, d.RemoveLast());
        Assert.HasCount(2, d);
    }

    [TestMethod]
    public void RemoveFirst_EmptyDeque_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(static () => new Deque<int>().RemoveFirst());
    }

    [TestMethod]
    public void RemoveLast_EmptyDeque_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(static () => new Deque<int>().RemoveLast());
    }

    [TestMethod]
    public void PeekFirst_EmptyDeque_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(static () => new Deque<int>().PeekFirst());
    }

    [TestMethod]
    public void PeekLast_EmptyDeque_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(static () => new Deque<int>().PeekLast());
    }

    [TestMethod]
    public void Indexer_ReturnsCorrectLogicalOrder()
    {
        Deque<int> d = new();
        d.AddLast(10); d.AddLast(20); d.AddLast(30);
        Assert.AreEqual(10, d[0]);
        Assert.AreEqual(30, d[2]);
    }

    [TestMethod]
    public void Clear_ResetsDeque()
    {
        Deque<int> d = new();
        d.AddLast(1); d.AddLast(2);
        d.Clear();
        Assert.IsEmpty(d);
        Assert.IsTrue(d.IsEmpty);
    }

    [TestMethod]
    public void AddFirst_GrowsCapacityWhenFull()
    {
        Deque<int> d = new(1);
        d.AddFirst(1); d.AddFirst(2); d.AddFirst(3);
        Assert.HasCount(3, d);
        Assert.AreEqual(3, d.PeekFirst());
    }

    [TestMethod]
    public void Enumeration_ReturnsItemsFrontToBack()
    {
        Deque<int> d = new();
        d.AddLast(1); d.AddLast(2); d.AddLast(3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, d.ToList());
    }
}
