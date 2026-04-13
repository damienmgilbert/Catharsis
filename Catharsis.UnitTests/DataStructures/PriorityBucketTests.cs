using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

///<summary>
///Unit tests for the <see cref="PriorityBucket"/> class.
///</summary>
[TestClass]
public class PriorityBucketTests
{
    #region Public methods
    [TestMethod]
    public void Clear_RemovesAllElements()
    {
        PriorityBucket<string, int> pq = new();
        pq.Enqueue("a", 1);
        pq.Clear();
        Assert.IsEmpty(pq);
        Assert.IsTrue(pq.IsEmpty);
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new PriorityBucket<string, int>(null!)); }
    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        PriorityBucket<string, int> pq = new();
        pq.Enqueue("test", 1);
        Assert.IsTrue(pq.Contains("test"));
        Assert.IsFalse(pq.Contains("other"));
    }

    [TestMethod]
    public void Enqueue_And_Dequeue_ReturnsInPriorityOrder()
    {
        PriorityBucket<string, int> pq = new();
        pq.Enqueue("low", 3);
        pq.Enqueue("high", 1);
        pq.Enqueue("mid", 2);
        Assert.AreEqual("high", pq.Dequeue());
        Assert.AreEqual("mid", pq.Dequeue());
    }

    [TestMethod]
    public void Peek_ReturnsHighestPriority()
    {
        PriorityBucket<string, int> pq = new();
        pq.Enqueue("a", 2);
        pq.Enqueue("b", 1);
        Assert.AreEqual("b", pq.Peek());
        Assert.HasCount(2, pq);
    }

    [TestMethod]
    public void TryDequeue_EmptyQueue_ReturnsFalse()
    {
        PriorityBucket<string, int> pq = new();
        Assert.IsFalse(pq.TryDequeue(out _, out _));
    }

    [TestMethod]
    public void TryPeek_EmptyQueue_ReturnsFalse()
    {
        PriorityBucket<string, int> pq = new();
        Assert.IsFalse(pq.TryPeek(out _, out _));
    }
    #endregion
}
