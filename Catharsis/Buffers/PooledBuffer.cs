using System.Buffers;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Buffers;

///<summary>
///A resizable buffer backed by <see cref="ArrayPool{T}"/> that implements <see cref="IResizableBuffer{T}"/> for
///efficient memory management.
///</summary>
///<typeparam name="T">The type of elements in the buffer.</typeparam>
public sealed class PooledBuffer<T> : IResizableBuffer<T>
{
    #region Fields
    T[] _buffer;
    bool _disposed;
    readonly ArrayPool<T> _pool;
    int _position;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="PooledBuffer{T}"/> with the specified initial capacity and growth strategy.
    ///</summary>
    ///<param name="initialCapacity">The initial buffer capacity.</param>
    ///<param name="growthStrategy">The strategy used to grow the buffer.</param>
    public PooledBuffer(int initialCapacity = 256, BufferGrowthStrategy growthStrategy = BufferGrowthStrategy.Doubling) : this(ArrayPool<T>.Shared, initialCapacity, growthStrategy)
    {
    }

    ///<summary>
    ///Initializes a new <see cref="PooledBuffer{T}"/> with a custom pool, capacity, and growth strategy.
    ///</summary>
    ///<param name="pool">The array pool to rent from.</param>
    ///<param name="initialCapacity">The initial buffer capacity.</param>
    ///<param name="growthStrategy">The strategy used to grow the buffer.</param>
    public PooledBuffer(ArrayPool<T> pool, int initialCapacity = 256, BufferGrowthStrategy growthStrategy = BufferGrowthStrategy.Doubling)
    {
        Guard.IsNotNull(pool);
        Guard.IsGreaterThan(initialCapacity, 0);

        _pool = pool;
        _buffer = _pool.Rent(initialCapacity);
        GrowthStrategy = growthStrategy;
    }
    #endregion

    #region Private methods
    int CalculateNewSize(int required)
    {
        return GrowthStrategy switch
        {
            BufferGrowthStrategy.Doubling => Math.Max(_buffer.Length * 2, required),
            BufferGrowthStrategy.Linear => Math.Max(_buffer.Length + 256, required),
            BufferGrowthStrategy.Exact => required,
            BufferGrowthStrategy.OnePointFive => Math.Max((int)(_buffer.Length * 1.5), required),
            _ => Math.Max(_buffer.Length * 2, required)
        };
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Advance(int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.IsGreaterThanOrEqualTo(count, 0);

        if (_position + count > _buffer.Length)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(count), "Cannot advance past the end of the buffer.");
        }

        _position += count;
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _pool.Return(_buffer);
        _buffer = [];
        _position = 0;
    }

    ///<inheritdoc/>
    public void EnsureCapacity(int capacity)
    {
        if (capacity <= _buffer.Length)
        {
            return;
        }

        int newSize = CalculateNewSize(capacity);
        T[] newBuffer = _pool.Rent(newSize);
        _buffer.AsSpan(0, _position).CopyTo(newBuffer);
        _pool.Return(_buffer);
        _buffer = newBuffer;
    }

    ///<inheritdoc/>
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureCapacity(_position + Math.Max(sizeHint, 1));
        return _buffer.AsMemory(_position);
    }

    ///<inheritdoc/>
    public Span<T> GetSpan(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureCapacity(_position + Math.Max(sizeHint, 1));
        return _buffer.AsSpan(_position);
    }

    ///<inheritdoc/>
    public void Reset() { _position = 0; }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public int Capacity => _buffer.Length;

    ///<inheritdoc/>
    public BufferGrowthStrategy GrowthStrategy { get; }

    ///<inheritdoc/>
    public int WrittenCount => _position;

    ///<inheritdoc/>
    public ReadOnlyMemory<T> WrittenMemory => _buffer.AsMemory(0, _position);

    ///<inheritdoc/>
    public ReadOnlySpan<T> WrittenSpan => _buffer.AsSpan(0, _position);
    #endregion
}
