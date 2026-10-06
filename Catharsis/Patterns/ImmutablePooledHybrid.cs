using Catharsis.Buffers;
using Catharsis.Immutable;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Patterns;

///<summary>
///Demonstrates a hybrid data structure that uses pooled buffers for mutable construction and freezes them into
///immutable form for thread-safe read access.
///</summary>
///<remarks>
///Initializes a new <see cref="ImmutablePooledHybrid{T}"/>.
///</remarks>
///<param name="initialCapacity">The initial mutable buffer capacity.</param>
public sealed class ImmutablePooledHybrid<T>(int initialCapacity = 256) : IDisposable
{
    #region Fields
    bool _disposed;
    ImmutableBuffer<T> _frozen = ImmutableBuffer<T>.Empty;
    bool _isFrozen;
    readonly PooledBuffer<T> _mutableBuffer = new PooledBuffer<T>(initialCapacity);

    #endregion
    #region Constructors
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _mutableBuffer.Dispose();
    }

    ///<summary>
    ///Freezes the mutable buffer into an immutable snapshot. After freezing, no more writes are allowed but reads
    ///become thread-safe.
    ///</summary>
    ///<returns>The immutable buffer.</returns>
    public ImmutableBuffer<T> Freeze()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_isFrozen)
        {
            _frozen = new ImmutableBuffer<T>(_mutableBuffer.WrittenSpan);
            _isFrozen = true;
        }

        return _frozen;
    }

    ///<summary>
    ///Resets the hybrid to mutable mode, discarding the frozen snapshot.
    ///</summary>
    public void Reset()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _mutableBuffer.Reset();
        _frozen = ImmutableBuffer<T>.Empty;
        _isFrozen = false;
    }

    ///<summary>
    ///Writes data to the mutable buffer. Throws if already frozen.
    ///</summary>
    ///<param name="data">The data to write.</param>
    public void Write(ReadOnlySpan<T> data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.IsFalse(_isFrozen, "Cannot write to a frozen buffer.");

        Span<T> span = _mutableBuffer.GetSpan(data.Length);
        data.CopyTo(span);
        _mutableBuffer.Advance(data.Length);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of elements.
    ///</summary>
    public int Count => _isFrozen ? _frozen.Count : _mutableBuffer.WrittenCount;

    ///<summary>
    ///Gets whether the buffer has been frozen into immutable form.
    ///</summary>
    public bool IsFrozen => _isFrozen;

    ///<summary>
    ///Gets the data as a read-only span. Returns the frozen data if frozen, otherwise returns the current mutable data.
    ///</summary>
    public ReadOnlySpan<T> Span => _isFrozen ? _frozen.Span : _mutableBuffer.WrittenSpan;
    #endregion
}
