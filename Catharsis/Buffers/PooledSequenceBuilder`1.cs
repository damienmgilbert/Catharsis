using System.Buffers;
using System.Runtime.CompilerServices;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Buffers;

///<summary>
///Builds a <see cref="ReadOnlySequence{T}"/> incrementally from pooled segments, minimizing allocations by renting from
public sealed class PooledSequenceBuilder<T> : IBufferWriter<T>, IDisposable
{
    #region Fields
    private T[]? _currentBuffer;
    private int _currentOffset;
    private readonly int _defaultSegmentSize;
    private bool _disposed;
    private readonly ArrayPool<T> _pool;
    private readonly List<PooledSegment> _segments = [];
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="PooledSequenceBuilder{T}"/> using the shared array pool.
    ///</summary>
    ///<param name="defaultSegmentSize">The default segment size for new allocations.</param>
    public PooledSequenceBuilder(int defaultSegmentSize = 4096) : this(ArrayPool<T>.Shared, defaultSegmentSize)
    {
    }

    ///<summary>
    ///Initializes a new <see cref="PooledSequenceBuilder{T}"/> with a specified pool and segment size.
    ///</summary>
    ///<param name="pool">The array pool to rent from.</param>
    ///<param name="defaultSegmentSize">The default segment size for new allocations.</param>
    public PooledSequenceBuilder(ArrayPool<T> pool, int defaultSegmentSize = 4096)
    {
        Guard.IsNotNull(pool);
        Guard.IsGreaterThan(defaultSegmentSize, 0);

        _pool = pool;
        _defaultSegmentSize = defaultSegmentSize;
    }
    #endregion

    #region Private methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureBuffer(int sizeHint)
    {
        int required = Math.Max(sizeHint, 1);
        if((_currentBuffer is not null) && (_currentOffset + required <= _currentBuffer.Length))
        {
            return;
        }

        FlushCurrent();
        int size = Math.Max(_defaultSegmentSize, required);
        _currentBuffer = _pool.Rent(size);
        _currentOffset = 0;
    }

    private void FlushCurrent()
    {
        if((_currentBuffer is not null) && (_currentOffset > 0))
        {
            _segments.Add(new PooledSegment(_currentBuffer, _currentOffset));
            _currentBuffer = null;
            _currentOffset = 0;
        } else if((_currentBuffer is not null) && (_currentOffset == 0))
        {
            _pool.Return(_currentBuffer);
            _currentBuffer = null;
        }
    }

    private void ReturnSegments()
    {
        foreach(PooledSegment seg in _segments)
        {
            _pool.Return(seg.Array);
        }

        _segments.Clear();

        if(_currentBuffer is not null)
        {
            _pool.Return(_currentBuffer);
            _currentBuffer = null;
            _currentOffset = 0;
        }
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Advance(int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.IsGreaterThanOrEqualTo(count, 0);

        if((_currentBuffer is null) || (_currentOffset + count > _currentBuffer.Length))
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(count), "Cannot advance past the end of the current buffer.");
        }

        _currentOffset += count;
    }

    ///<summary>
    ///Builds a <see cref="ReadOnlySequence{T}"/> from the written segments. The caller should not dispose this builder
    ///until the sequence is no longer needed.
    ///</summary>
    ///<returns>A <see cref="ReadOnlySequence{T}"/> covering all written data.</returns>
    public ReadOnlySequence<T> Build()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        FlushCurrent();

        if(_segments.Count == 0)
        {
            return ReadOnlySequence<T>.Empty;
        }

        if(_segments.Count == 1)
        {
            return new ReadOnlySequence<T>(_segments[0].Array, 0, _segments[0].Length);
        }

        SequenceSegment<T>? first = null;
        SequenceSegment<T>? last = null;

        foreach(PooledSegment seg in _segments)
        {
            SequenceSegment<T> node = new(seg.Array.AsMemory(0, seg.Length), last);
            first ??= node;
            last = node;
        }

        return new ReadOnlySequence<T>(first!, 0, last!, last!.Memory.Length);
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;
        Reset();
    }

    ///<inheritdoc/>
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureBuffer(sizeHint);
        return _currentBuffer.AsMemory(_currentOffset);
    }

    ///<inheritdoc/>
    public Span<T> GetSpan(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureBuffer(sizeHint);
        return _currentBuffer.AsSpan(_currentOffset);
    }

    ///<summary>
    ///Resets the builder, returning all rented segments to the pool.
    ///</summary>
    public void Reset()
    {
        ReturnSegments();
        _currentBuffer = null;
        _currentOffset = 0;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the total number of elements written across all segments.
    ///</summary>
    public long WrittenCount
    {
        get
        {
            long total = 0;
            foreach(PooledSegment seg in _segments)
            {
                total += seg.Length;
            }

            total += _currentOffset;
            return total;
        }
    }
    #endregion

    private readonly record struct PooledSegment(T[] Array, int Length);
}
