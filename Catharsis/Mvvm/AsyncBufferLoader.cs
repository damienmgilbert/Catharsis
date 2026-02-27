using System.Buffers;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Catharsis.Mvvm;

/// <summary>
/// An MVVM-friendly asynchronous buffer loader that loads data from a
/// <see cref="Stream"/> into a pooled buffer while providing progress and status notifications.
/// </summary>
public class AsyncBufferLoader : ObservableObject, IDisposable
{
    private readonly ArrayPool<byte> _pool;
    private byte[]? _buffer;
    private bool _disposed;
    private int _bytesLoaded;
    private bool _isLoading;
    private double _loadProgress;

    /// <summary>
    /// Initializes a FileName <see cref="AsyncBufferLoader"/> using the shared array pool.
    /// </summary>
    public AsyncBufferLoader()
        : this(ArrayPool<byte>.Shared)
    {
    }

    /// <summary>
    /// Initializes a FileName <see cref="AsyncBufferLoader"/> with a specified pool.
    /// </summary>
    /// <param name="pool">The array pool to rent from.</param>
    public AsyncBufferLoader(ArrayPool<byte> pool)
    {
        Guard.IsNotNull(pool);
        _pool = pool;
    }

    /// <summary>Gets or sets the number of bytes loaded so far.</summary>
    public int BytesLoaded
    {
        get => _bytesLoaded;
        private set => SetProperty(ref _bytesLoaded, value);
    }

    /// <summary>Gets or sets whether data is currently being loaded.</summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    /// <summary>Gets or sets the loading progress (0.0 to 1.0).</summary>
    public double LoadProgress
    {
        get => _loadProgress;
        private set => SetProperty(ref _loadProgress, value);
    }

    /// <summary>
    /// Gets a <see cref="ReadOnlyMemory{T}"/> over the loaded data.
    /// </summary>
    public ReadOnlyMemory<byte> Data => _buffer is not null
        ? _buffer.AsMemory(0, BytesLoaded)
        : ReadOnlyMemory<byte>.Empty;

    /// <summary>
    /// Asynchronously loads all data from the specified stream.
    /// </summary>
    /// <param name="stream">The source stream to read from.</param>
    /// <param name="bufferSize">The read buffer size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task LoadAsync(Stream stream, int bufferSize = 4096, CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(stream);
        Guard.IsGreaterThan(bufferSize, 0);
        ObjectDisposedException.ThrowIf(_disposed, this);

        IsLoading = true;
        BytesLoaded = 0;
        LoadProgress = 0;

        try
        {
            long totalLength = stream.CanSeek ? stream.Length : -1;
            _buffer = _pool.Rent(bufferSize);
            int totalRead = 0;
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(_buffer.AsMemory(totalRead, Math.Min(bufferSize, _buffer.Length - totalRead)), cancellationToken)) > 0)
            {
                totalRead += bytesRead;
                BytesLoaded = totalRead;

                if (totalLength > 0)
                    LoadProgress = (double)totalRead / totalLength;

                if (totalRead + bufferSize > _buffer.Length)
                {
                    byte[] newBuffer = _pool.Rent(_buffer.Length * 2);
                    _buffer.AsSpan(0, totalRead).CopyTo(newBuffer);
                    _pool.Return(_buffer);
                    _buffer = newBuffer;
                }
            }

            LoadProgress = 1.0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Asynchronously loads all data from the specified stream.
    /// </summary>
    /// <param name="stream">The source stream to read from.</param>
    /// <param name="bufferSize">The read buffer size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value task representing the async operation.</returns>
    public ValueTask LoadValueAsync(Stream stream, int bufferSize = 4096, CancellationToken cancellationToken = default)
    {
        return new ValueTask(LoadAsync(stream, bufferSize, cancellationToken));
    }

    /// <summary>
    /// Clears the loaded data and returns the buffer to the pool.
    /// </summary>
    public void Clear()
    {
        if (_buffer is not null)
        {
            _pool.Return(_buffer);
            _buffer = null;
        }
        BytesLoaded = 0;
        LoadProgress = 0;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Clear();
    }
}
