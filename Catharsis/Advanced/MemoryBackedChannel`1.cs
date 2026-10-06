using System.Buffers;
using System.Threading.Channels;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Advanced;

///<summary>
///A <see cref="Channel{T}"/>-based producer/consumer that transports pooled memory blocks between async pipelines,
///combining <see cref="MemoryPool{T}"/> with channel-based concurrency.
///</summary>
///<typeparam name="T">The element type of the memory blocks.</typeparam>
public sealed class MemoryBackedChannel<T> : IDisposable
{
    #region Fields
    private readonly Channel<OwnedSegment> _channel;
    private bool _disposed;
    private readonly MemoryPool<T> _pool;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="MemoryBackedChannel{T}"/> with the specified capacity.
    ///</summary>
    ///<param name="capacity">The maximum number of segments that can be buffered. Use 0 for unbounded.</param>
    public MemoryBackedChannel(int capacity = 0) : this(MemoryPool<T>.Shared, capacity)
    {
    }

    ///<summary>
    ///Initializes a new <see cref="MemoryBackedChannel{T}"/> with a specified pool and capacity.
    ///</summary>
    ///<param name="pool">The memory pool to rent from.</param>
    ///<param name="capacity">The channel capacity. Use 0 for unbounded.</param>
    public MemoryBackedChannel(MemoryPool<T> pool, int capacity = 0)
    {
        Guard.IsNotNull(pool);
        Guard.IsGreaterThanOrEqualTo(capacity, 0);

        _pool = pool;
        _channel = (capacity > 0) ? Channel.CreateBounded<OwnedSegment>(new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.Wait }) : Channel.CreateUnbounded<OwnedSegment>();
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Signals that no more data will be written to the channel.
    ///</summary>
    public void Complete() => _channel.Writer.TryComplete();

    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;
        _channel.Writer.TryComplete();

        while(_channel.Reader.TryRead(out OwnedSegment? segment))
        {
            segment.Dispose();
        }
    }

    ///<summary>
    ///Reads the next memory segment from the channel. The caller must dispose the returned segment when done.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token.</param>
    ///<returns>An owned memory segment.</returns>
    public async ValueTask<OwnedSegment> ReadAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await _channel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
    }

    ///<summary>
    ///Writes data to the channel using a pooled memory block.
    ///</summary>
    ///<param name="data">The data to write.</param>
    ///<param name="cancellationToken">A cancellation token.</param>
    ///<returns>A task representing the write operation.</returns>
    public async ValueTask WriteAsync(ReadOnlyMemory<T> data, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        IMemoryOwner<T> owner = _pool.Rent(data.Length);
        data.CopyTo(owner.Memory);

        OwnedSegment segment = new(owner, data.Length);
        await _channel.Writer.WriteAsync(segment, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the reader side of the channel.
    ///</summary>
    public ChannelReader<OwnedSegment> Reader => _channel.Reader;
    #endregion

    ///<summary>
    ///Represents a memory segment owned by a pooled <see cref="IMemoryOwner{T}"/>. Must be disposed after use to return
    ///memory to the pool.
    ///</summary>
    public sealed class OwnedSegment : IDisposable
    {
        #region Fields
        private bool _disposed;
        private readonly IMemoryOwner<T> _owner;
        #endregion

        #region Constructors
        internal OwnedSegment(IMemoryOwner<T> owner, int length)
        {
            _owner = owner;
            Length = length;
        }
        #endregion

        #region Public methods
        ///<inheritdoc/>
        public void Dispose()
        {
            if(_disposed)
            {
                return;
            }

            _disposed = true;
            _owner.Dispose();
        }
        #endregion

        #region Public properties
        ///<summary>
        ///Gets the length of valid data in this segment.
        ///</summary>
        public int Length { get; }

        ///<summary>
        ///Gets a <see cref="ReadOnlyMemory{T}"/> over the valid data.
        ///</summary>
        public ReadOnlyMemory<T> Memory => _owner.Memory[..Length];
        #endregion
    }
}
