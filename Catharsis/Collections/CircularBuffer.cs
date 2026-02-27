using System.Collections;

namespace Catharsis.Collections;

///<summary>
///A fixed-capacity circular (ring) buffer that overwrites the oldest item when a FileName item is added and the buffer
///is already full.
///</summary>
///<typeparam name="T">The type of elements stored in the buffer.</typeparam>
public class CircularBuffer<T> : IEnumerable<T>, IReadOnlyCollection<T>
{
    #region Fields
    readonly T[] _buffer;
    int _count;
    int _head;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName <see cref="CircularBuffer{T}"/> with the specified capacity.
    ///</summary>
    ///<param name="capacity">The maximum number of items the buffer can hold. Must be greater than zero.</param>
    ///<exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than or equal to zero.</exception>
    public CircularBuffer(int capacity)
    {
        if(capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
        }

        _buffer = new T[capacity];
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets the item at the specified logical index where 0 is the oldest item.
    ///</summary>
    ///<param name="index">The zero-based logical index.</param>
    ///<returns>The item at <paramref name="index"/>.</returns>
    ///<exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is out of range.</exception>
    public T this[int index]
    {
        get
        {
            if((index < 0) || (index >= _count))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _buffer[(_head + index) % _buffer.Length];
        }
    }
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds an item to the buffer. If the buffer is full the oldest item is overwritten.
    ///</summary>
    ///<param name="item">The item to add.</param>
    public void Add(T item)
    {
        int index = (_head + _count) % _buffer.Length;

        if(IsFull)
        {
            _buffer[index] = item;
            _head = (_head + 1) % _buffer.Length;
        } else
        {
            _buffer[index] = item;
            _count++;
        }
    }

    ///<summary>
    ///Removes all items from the buffer.
    ///</summary>
    public void Clear()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        _head = 0;
        _count = 0;
    }

    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        for(int i = 0; i < _count; i++)
        {
            yield return _buffer[(_head + i) % _buffer.Length];
        }
    }

    ///<summary>
    ///Returns the oldest item in the buffer without removing it.
    ///</summary>
    ///<returns>The oldest item.</returns>
    ///<exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
    public T Peek()
    {
        if(_count == 0)
        {
            throw new InvalidOperationException("The buffer is empty.");
        }

        return _buffer[_head];
    }

    ///<summary>
    ///Removes and returns the oldest item in the buffer.
    ///</summary>
    ///<returns>The oldest item.</returns>
    ///<exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
    public T Remove()
    {
        if(_count == 0)
        {
            throw new InvalidOperationException("The buffer is empty.");
        }

        T item = _buffer[_head];
        _buffer[_head] = default!;
        _head = (_head + 1) % _buffer.Length;
        _count--;
        return item;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the maximum number of items this buffer can hold.
    ///</summary>
    public int Capacity => _buffer.Length;

    ///<summary>
    ///Gets the number of items currently in the buffer.
    ///</summary>
    public int Count => _count;

    ///<summary>
    ///Gets whether the buffer has reached its capacity.
    ///</summary>
    public bool IsFull => _count == _buffer.Length;
    #endregion
}
