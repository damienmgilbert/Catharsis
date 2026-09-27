using System.Buffers;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Buffers;

///<summary>
///A <see cref="Stream"/> wrapper around an <see cref="IBufferWriter{T}"/> for byte data, enabling stream-based APIs to
///write directly into a buffer writer without intermediate copies.
///</summary>
public sealed class BufferWriterStream : Stream
{
    #region Fields
    long _bytesWritten;
    bool _disposed;
    readonly IBufferWriter<byte> _writer;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="BufferWriterStream"/> that writes to the specified buffer writer.
    ///</summary>
    ///<param name="writer">The buffer writer to write to.</param>
    public BufferWriterStream(IBufferWriter<byte> writer)
    {
        Guard.IsNotNull(writer);
        _writer = writer;
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _disposed = true;
        base.Dispose(disposing);
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override void Flush()
    {
    }
    ///<inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken) { return Task.CompletedTask; }
    ///<inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) { throw new NotSupportedException("Reading is not supported."); }
    ///<inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException("Seeking is not supported."); }
    ///<inheritdoc/>
    public override void SetLength(long value) { throw new NotSupportedException("Setting length is not supported."); }

    ///<inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if(buffer.IsEmpty)
        {
            return;
        }

        Span<byte> destination = _writer.GetSpan(buffer.Length);
        buffer.CopyTo(destination);
        _writer.Advance(buffer.Length);
        _bytesWritten += buffer.Length;
    }

    ///<inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.IsNotNull(buffer);
        Guard.IsInRangeFor(offset, buffer);
        Guard.IsGreaterThanOrEqualTo(count, 0);

        Write(buffer.AsSpan(offset, count));
    }

    ///<inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }

    ///<inheritdoc/>
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.IsNotNull(buffer);

        cancellationToken.ThrowIfCancellationRequested();
        Write(buffer, offset, count);
        await Task.CompletedTask.ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public override void WriteByte(byte value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Span<byte> span = _writer.GetSpan(1);
        span[0] = value;
        _writer.Advance(1);
        _bytesWritten++;
    }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public override bool CanRead => false;

    ///<inheritdoc/>
    public override bool CanSeek => false;

    ///<inheritdoc/>
    public override bool CanWrite => !_disposed;

    ///<inheritdoc/>
    public override long Length => _bytesWritten;

    ///<inheritdoc/>
    public override long Position { get => _bytesWritten; set => throw new NotSupportedException("Seeking is not supported."); }
    #endregion
}
