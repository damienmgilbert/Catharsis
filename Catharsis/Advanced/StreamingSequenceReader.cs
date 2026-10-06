using System.Buffers;
using Catharsis.Buffers;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Advanced;

///<summary>
///Reads structured data from a <see cref="Stream"/> by building a <see cref="ReadOnlySequence{T}"/> incrementally using
///<see cref="PooledSequenceBuilder{T}"/>.
///</summary>
public sealed class StreamingSequenceReader : IDisposable
{
    #region Fields
    readonly PooledSequenceBuilder<byte> _builder;
    bool _disposed;
    readonly ArrayPool<byte> _pool;
    readonly int _readBufferSize;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="StreamingSequenceReader"/> with the specified buffer size.
    ///</summary>
    ///<param name="readBufferSize">The size of each read chunk.</param>
    public StreamingSequenceReader(int readBufferSize = 4096) : this(ArrayPool<byte>.Shared, readBufferSize)
    {
    }

    ///<summary>
    ///Initializes a new <see cref="StreamingSequenceReader"/> with a specified pool and buffer size.
    ///</summary>
    ///<param name="pool">The array pool to use.</param>
    ///<param name="readBufferSize">The size of each read chunk.</param>
    public StreamingSequenceReader(ArrayPool<byte> pool, int readBufferSize = 4096)
    {
        Guard.IsNotNull(pool);
        Guard.IsGreaterThan(readBufferSize, 0);

        _pool = pool;
        _readBufferSize = readBufferSize;
        _builder = new PooledSequenceBuilder<byte>(pool, readBufferSize);
    }
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
        _builder.Dispose();
    }

    ///<summary>
    ///Reads all data from the stream into a <see cref="ReadOnlySequence{T}"/>. The builder must not be disposed while
    ///the sequence is in use.
    ///</summary>
    ///<param name="stream">The source stream.</param>
    ///<param name="cancellationToken">A cancellation token.</param>
    ///<returns>A <see cref="ReadOnlySequence{T}"/> over the read data.</returns>
    public async Task<ReadOnlySequence<byte>> ReadAllAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(stream);
        ObjectDisposedException.ThrowIf(_disposed, this);

        byte[] readBuffer = _pool.Rent(_readBufferSize);
        try
        {
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(readBuffer.AsMemory(0, _readBufferSize), cancellationToken).ConfigureAwait(false)) > 0)
            {
                Span<byte> span = _builder.GetSpan(bytesRead);
                readBuffer.AsSpan(0, bytesRead).CopyTo(span);
                _builder.Advance(bytesRead);
                TotalBytesRead += bytesRead;
            }
        }
        finally
        {
            _pool.Return(readBuffer);
        }

        return _builder.Build();
    }

    ///<summary>
    ///Reads all data from the stream into a <see cref="ReadOnlySequence{T}"/>.
    ///</summary>
    ///<param name="stream">The source stream.</param>
    ///<param name="cancellationToken">A cancellation token.</param>
    ///<returns>A <see cref="ReadOnlySequence{T}"/> over the read data.</returns>
    public async ValueTask<ReadOnlySequence<byte>> ReadAllValueAsync(Stream stream, CancellationToken cancellationToken = default) { return await ReadAllAsync(stream, cancellationToken).ConfigureAwait(false); }

    ///<summary>
    ///Resets the reader for reuse.
    ///</summary>
    public void Reset()
    {
        _builder.Reset();
        TotalBytesRead = 0;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the total number of bytes read so far.
    ///</summary>
    public long TotalBytesRead { get; private set; }
    #endregion
}
