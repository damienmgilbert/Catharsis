using System.Threading.Channels;

namespace Catharsis.Concurrency;

///<summary>
///A friendly async producer/consumer queue built on <see cref="Channel{T}"/>, supporting either unbounded capacity or a
///bounded capacity with configurable backpressure behavior.
///</summary>
///<typeparam name="T">The type of item carried by the queue.</typeparam>
public sealed class AsyncProducerConsumerQueue<T>
{
    #region Fields
    private readonly Channel<T> _channel;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates an unbounded queue.
    ///</summary>
    public AsyncProducerConsumerQueue() { _channel = Channel.CreateUnbounded<T>(); }

    ///<summary>
    ///Creates a bounded queue with the specified capacity.
    ///</summary>
    ///<param name="capacity">The maximum number of items the queue holds before applying <paramref name="fullMode"/>.</param>
    ///<param name="fullMode">The behavior applied when a producer writes to a full queue.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 1.</exception>
    public AsyncProducerConsumerQueue(int capacity, BoundedChannelFullMode fullMode = BoundedChannelFullMode.Wait)
    {
        if(capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be at least 1.");
        }

        _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity) { FullMode = fullMode });
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Marks the queue as complete, so that no further items may be enqueued. Consumers observe the remaining buffered
    ///items and then completion.
    ///</summary>
    ///<param name="error">An optional error with which to fault the queue instead of completing it normally.</param>
    public void Complete(Exception? error = null) => _channel.Writer.Complete(error);

    ///<summary>
    ///Asynchronously enumerates every item as it becomes available, until the queue is completed.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token that can abandon enumeration.</param>
    ///<returns>An async sequence of the queued items.</returns>
    public IAsyncEnumerable<T> DequeueAllAsync(CancellationToken cancellationToken = default) => _channel.Reader.ReadAllAsync(cancellationToken);

    ///<summary>
    ///Asynchronously dequeues the next available item, waiting if the queue is empty.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token that can abandon the wait.</param>
    ///<returns>The dequeued item.</returns>
    public ValueTask<T> DequeueAsync(CancellationToken cancellationToken = default) => _channel.Reader.ReadAsync(cancellationToken);

    ///<summary>
    ///Asynchronously enqueues an item, waiting if the queue is bounded and full.
    ///</summary>
    ///<param name="item">The item to enqueue.</param>
    ///<param name="cancellationToken">A cancellation token that can abandon the wait.</param>
    public ValueTask EnqueueAsync(T item, CancellationToken cancellationToken = default) => _channel.Writer.WriteAsync(item, cancellationToken);

    ///<summary>
    ///Attempts to dequeue an item without waiting.
    ///</summary>
    ///<param name="item">The dequeued item, if one was available.</param>
    ///<returns><c>true</c> if an item was dequeued; otherwise <c>false</c>.</returns>
    public bool TryDequeue(out T? item) => _channel.Reader.TryRead(out item);

    ///<summary>
    ///Attempts to enqueue an item without waiting.
    ///</summary>
    ///<param name="item">The item to enqueue.</param>
    ///<returns><c>true</c> if the item was enqueued; otherwise <c>false</c>.</returns>
    public bool TryEnqueue(T item) => _channel.Writer.TryWrite(item);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of items currently buffered in the queue.
    ///</summary>
    public int Count => _channel.Reader.Count;

    ///<summary>
    ///Gets whether the queue has been completed and fully drained.
    ///</summary>
    public bool IsCompleted => _channel.Reader.Completion.IsCompleted;
    #endregion
}
