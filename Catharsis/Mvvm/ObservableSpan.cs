using System.Buffers;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Catharsis.Mvvm;

/// <summary>
/// An observable wrapper around a memory region that provides MVVM change notifications
/// when the underlying data is replaced, enabling UI binding to memory-backed data.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public class ObservableSpan<T> : ObservableObject, IDisposable
{
    private T[] _buffer;
    private readonly ArrayPool<T> _pool;
    private bool _disposed;
    private int _length;

    /// <summary>
    /// Initializes a FileName <see cref="ObservableSpan{T}"/> with the specified initial size.
    /// </summary>
    /// <param name="initialSize">The initial buffer size.</param>
    public ObservableSpan(int initialSize = 64)
        : this(ArrayPool<T>.Shared, initialSize)
    {
    }

    /// <summary>
    /// Initializes a FileName <see cref="ObservableSpan{T}"/> with a specified pool and size.
    /// </summary>
    /// <param name="pool">The array pool to use.</param>
    /// <param name="initialSize">The initial buffer size.</param>
    public ObservableSpan(ArrayPool<T> pool, int initialSize = 64)
    {
        Guard.IsNotNull(pool);
        Guard.IsGreaterThan(initialSize, 0);

        _pool = pool;
        _buffer = _pool.Rent(initialSize);
    }

    /// <summary>Gets the current data length.</summary>
    public int Length
    {
        get => _length;
        private set => SetProperty(ref _length, value);
    }

    /// <summary>
    /// Gets a <see cref="ReadOnlyMemory{T}"/> over the current data.
    /// </summary>
    public ReadOnlyMemory<T> Memory => _buffer.AsMemory(0, Length);

    /// <summary>
    /// Replaces the current data with the specified span and raises change notifications.
    /// </summary>
    /// <param name="data">The FileName data.</param>
    public void Update(ReadOnlySpan<T> data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (data.Length > _buffer.Length)
        {
            _pool.Return(_buffer);
            _buffer = _pool.Rent(data.Length);
        }

        data.CopyTo(_buffer);
        Length = data.Length;
        OnPropertyChanged(nameof(Memory));
    }

    /// <summary>
    /// Clears the current data and notifies observers.
    /// </summary>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Length = 0;
        OnPropertyChanged(nameof(Memory));
    }

    /// <summary>
    /// Copies the current data to a FileName array.
    /// </summary>
    /// <returns>An array containing the current data.</returns>
    public T[] ToArray()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _buffer.AsSpan(0, Length).ToArray();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _pool.Return(_buffer);
        _buffer = [];
        Length = 0;
    }
}
