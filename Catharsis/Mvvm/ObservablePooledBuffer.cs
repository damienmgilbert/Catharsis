using System.Buffers;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Catharsis.Mvvm;

///<summary>
///An observable buffer that combines pooled memory management with MVVM change notification, allowing UI bindings to
///react to buffer content changes.
///</summary>
///<typeparam name="T">The type of elements in the buffer.</typeparam>
public class ObservablePooledBuffer<T> : ObservableObject, IDisposable
{
    #region Fields
    T[] _buffer;
    int _capacity;
    int _count;
    bool _disposed;
    readonly ArrayPool<T> _pool;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="ObservablePooledBuffer{T}"/> with the specified initial capacity.
    ///</summary>
    ///<param name="initialCapacity">The initial buffer capacity.</param>
    public ObservablePooledBuffer(int initialCapacity = 256) : this(ArrayPool<T>.Shared, initialCapacity)
    {
    }

    ///<summary>
    ///Initializes a new <see cref="ObservablePooledBuffer{T}"/> with a specified pool and capacity.
    ///</summary>
    ///<param name="pool">The array pool to rent from.</param>
    ///<param name="initialCapacity">The initial capacity.</param>
    public ObservablePooledBuffer(ArrayPool<T> pool, int initialCapacity = 256)
    {
        Guard.IsNotNull(pool);
        Guard.IsGreaterThan(initialCapacity, 0);

        _pool = pool;
        _buffer = _pool.Rent(initialCapacity);
        _capacity = _buffer.Length;
    }
    #endregion

    #region Private methods
    void EnsureCapacity(int required)
    {
        if(required <= _buffer.Length)
        {
            return;
        }

        int newSize = Math.Max(_buffer.Length * 2, required);
        T[] newBuffer = _pool.Rent(newSize);
        _buffer.AsSpan(0, Count).CopyTo(newBuffer);
        _pool.Return(_buffer);
        _buffer = newBuffer;
        Capacity = newBuffer.Length;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Clears the buffer and notifies observers.
    ///</summary>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Count = 0;
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;

        _pool.Return(_buffer);
        _buffer = [];
        Count = 0;
        Capacity = 0;
        GC.SuppressFinalize(this);
    }

    ///<summary>
    ///Copies the written data to a new array.
    ///</summary>
    ///<returns>An array containing the written data.</returns>
    public T[] ToArray()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _buffer.AsSpan(0, Count).ToArray();
    }

    ///<summary>
    ///Writes data to the buffer and raises property change notifications.
    ///</summary>
    ///<param name="data">The data to write.</param>
    public void Write(ReadOnlySpan<T> data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        EnsureCapacity(Count + data.Length);
        data.CopyTo(_buffer.AsSpan(Count));
        Count += data.Length;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the current capacity of the internal buffer.
    ///</summary>
    public int Capacity { get => _capacity; private set => SetProperty(ref _capacity, value); }

    ///<summary>
    ///Gets the number of elements written to the buffer.
    ///</summary>
    public int Count { get => _count; private set => SetProperty(ref _count, value); }

    ///<summary>
    ///Gets a <see cref="ReadOnlyMemory{T}"/> over the written portion of the buffer.
    ///</summary>
    public ReadOnlyMemory<T> WrittenMemory => _buffer.AsMemory(0, Count);
    #endregion
}
