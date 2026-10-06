using System.Threading.Channels;
using Catharsis.Concurrency;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="AsyncProducerConsumerQueue{T}"/> class.
///</summary>
[TestClass]
public class AsyncProducerConsumerQueueTests
{
    #region Construction

    [TestMethod]
    public void Constructor_BoundedZeroOrNegativeCapacity_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new AsyncProducerConsumerQueue<int>(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new AsyncProducerConsumerQueue<int>(-1));
    }

    #endregion

    #region Enqueue / Dequeue

    [TestMethod]
    public async Task EnqueueAsync_ThenDequeueAsync_ReturnsItem()
    {
        AsyncProducerConsumerQueue<int> queue = new();
        await queue.EnqueueAsync(42);
        int result = await queue.DequeueAsync();
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void TryEnqueue_Unbounded_AlwaysSucceeds()
    {
        AsyncProducerConsumerQueue<int> queue = new();
        Assert.IsTrue(queue.TryEnqueue(1));
        Assert.AreEqual(1, queue.Count);
    }

    [TestMethod]
    public void TryDequeue_EmptyQueue_ReturnsFalse()
    {
        AsyncProducerConsumerQueue<int> queue = new();
        Assert.IsFalse(queue.TryDequeue(out int item));
        Assert.AreEqual(0, item);
    }

    [TestMethod]
    public void TryDequeue_ItemAvailable_ReturnsTrue()
    {
        AsyncProducerConsumerQueue<int> queue = new();
        queue.TryEnqueue(7);
        Assert.IsTrue(queue.TryDequeue(out int item));
        Assert.AreEqual(7, item);
    }

    [TestMethod]
    public void TryEnqueue_BoundedFullWithDropWriteMode_Fails()
    {
        AsyncProducerConsumerQueue<int> queue = new(1, BoundedChannelFullMode.Wait);
        Assert.IsTrue(queue.TryEnqueue(1));
        Assert.IsFalse(queue.TryEnqueue(2));
    }

    [TestMethod]
    public async Task DequeueAllAsync_EnumeratesUntilCompleted()
    {
        AsyncProducerConsumerQueue<int> queue = new();
        queue.TryEnqueue(1);
        queue.TryEnqueue(2);
        queue.Complete();

        List<int> items = [];

        await foreach (int item in queue.DequeueAllAsync())
        {
            items.Add(item);
        }

        Assert.HasCount(2, items);
        Assert.AreEqual(1, items[0]);
        Assert.AreEqual(2, items[1]);
    }

    [TestMethod]
    public void Complete_MarksQueueCompleted()
    {
        AsyncProducerConsumerQueue<int> queue = new();
        Assert.IsFalse(queue.IsCompleted);
        queue.Complete();
        Assert.IsTrue(queue.IsCompleted);
    }

    [TestMethod]
    public async Task Complete_WithError_FaultsPendingDequeue()
    {
        AsyncProducerConsumerQueue<int> queue = new();
        Task<int> dequeue = queue.DequeueAsync().AsTask();

        queue.Complete(new InvalidOperationException("boom"));

        ChannelClosedException ex = await Assert.ThrowsExactlyAsync<ChannelClosedException>(async () => await dequeue);
        Assert.IsInstanceOfType<InvalidOperationException>(ex.InnerException);
    }

    #endregion
}
