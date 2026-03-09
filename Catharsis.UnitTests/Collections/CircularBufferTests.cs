using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="CircularBuffer"/> class.
///</summary>
[TestClass]
public class CircularBufferTests
{
    [TestMethod]
    public void Constructor_ValidCapacity_CreatesBuffer()
    {
        CircularBuffer<int> buffer = new CircularBuffer<int>(5);
        Assert.AreEqual(5, buffer.Capacity);
        Assert.IsEmpty(buffer);
        Assert.IsFalse(buffer.IsFull);
    }

    [TestMethod]
    public void Constructor_ZeroCapacity_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new CircularBuffer<int>(0));
    }

    [TestMethod]
    public void Add_BelowCapacity_IncreasesCount()
    {
        CircularBuffer<int> buffer = new CircularBuffer<int>(3);
        buffer.Add(1);
        buffer.Add(2);
        Assert.HasCount(2, buffer);
        Assert.IsFalse(buffer.IsFull);
    }

    [TestMethod]
    public void Add_AtCapacity_OverwritesOldest()
    {
        CircularBuffer<int> buffer = new CircularBuffer<int>(3);
        buffer.Add(1); buffer.Add(2); buffer.Add(3);
        Assert.IsTrue(buffer.IsFull);
        buffer.Add(4);
        Assert.HasCount(3, buffer);
        Assert.AreEqual(2, buffer.Peek());
    }

    [TestMethod]
    public void Peek_ReturnsOldestItem()
    {
        CircularBuffer<int> buffer = new CircularBuffer<int>(3);
        buffer.Add(10); buffer.Add(20);
        Assert.AreEqual(10, buffer.Peek());
    }

    [TestMethod]
    public void Peek_EmptyBuffer_Throws()
    {
        CircularBuffer<int> buffer = new CircularBuffer<int>(3);
        Assert.ThrowsExactly<InvalidOperationException>(() => buffer.Peek());
    }

    [TestMethod]
    public void Remove_ReturnsAndRemovesOldest()
    {
        CircularBuffer<int> buffer = new CircularBuffer<int>(3);
        buffer.Add(1); buffer.Add(2); buffer.Add(3);
        Assert.AreEqual(1, buffer.Remove());
        Assert.HasCount(2, buffer);
        Assert.AreEqual(2, buffer.Peek());
    }

    [TestMethod]
    public void Remove_EmptyBuffer_Throws()
    {
        CircularBuffer<int> buffer = new CircularBuffer<int>(3);
        Assert.ThrowsExactly<InvalidOperationException>(() => buffer.Remove());
    }

    [TestMethod]
    public void Clear_ResetsBuffer()
    {
        CircularBuffer<int> buffer = new CircularBuffer<int>(3);
        buffer.Add(1); buffer.Add(2);
        buffer.Clear();
        Assert.IsEmpty(buffer);
        Assert.IsFalse(buffer.IsFull);
    }

    [TestMethod]
    public void Indexer_ReturnsCorrectLogicalOrder()
    {
        CircularBuffer<int> buffer = new CircularBuffer<int>(3);
        buffer.Add(10); buffer.Add(20); buffer.Add(30);
        Assert.AreEqual(10, buffer[0]);
        Assert.AreEqual(20, buffer[1]);
        Assert.AreEqual(30, buffer[2]);
    }

    [TestMethod]
    public void Indexer_OutOfRange_Throws()
    {
        CircularBuffer<int> buffer = new CircularBuffer<int>(3);
        buffer.Add(1);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = buffer[5]);
    }

    [TestMethod]
    public void Enumeration_ReturnsItemsInOrder()
    {
        CircularBuffer<int> buffer = new CircularBuffer<int>(3);
        buffer.Add(1); buffer.Add(2); buffer.Add(3); buffer.Add(4);
        CollectionAssert.AreEqual(new[] { 2, 3, 4 }, buffer.ToList());
    }
}
