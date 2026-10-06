using System.Collections;

namespace Catharsis.Collections;

///<summary>
///A double-ended queue (deque) that supports efficient insertion and removal at both the front and back.
///</summary>
///<typeparam name="T">The type of elements stored in the deque.</typeparam>
public sealed class Deque<T> : IEnumerable<T>, IReadOnlyCollection<T>
{
    #region Constants
    private const int DefaultCapacity = 4;
    #endregion

    #region Fields
    private T[] _buffer;
    private int _count;
    private int _head;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new empty <see cref="Deque{T}"/> with the default initial capacity.
    ///</summary>
    public Deque() : this(DefaultCapacity)
    {
    }

    ///<summary>
    ///Initializes a new empty <see cref="Deque{T}"/> with the specified initial capacity.
    ///</summary>
    ///<param name="capacity">The initial capacity. Must be greater than or equal to zero.</param>
    ///<exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is negative.</exception>
    public Deque(int capacity)
    {
        if(capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must not be negative.");
        }

        _buffer = new T[Math.Max(capacity, 1)];
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets the item at the specified logical index where 0 is the front.
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
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Private methods
    private void EnsureCapacity()
    {
        if(_count < _buffer.Length)
        {
            return;
        }

        int newCapacity = _buffer.Length * 2;
        T[] newBuffer = new T[newCapacity];

        for(int i = 0; i < _count; i++)
        {
            newBuffer[i] = _buffer[(_head + i) % _buffer.Length];
        }

        _buffer = newBuffer;
        _head = 0;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds an item to the front of the deque.
    ///</summary>
    ///<param name="item">The item to add.</param>
    public void AddFirst(T item)
    {
        EnsureCapacity();
        _head = ((_head - 1) + _buffer.Length) % _buffer.Length;
        _buffer[_head] = item;
        _count++;
    }

    ///<summary>
    ///Adds an item to the back of the deque.
    ///</summary>
    ///<param name="item">The item to add.</param>
    public void AddLast(T item)
    {
        EnsureCapacity();
        int tail = (_head + _count) % _buffer.Length;
        _buffer[tail] = item;
        _count++;
    }

    ///<summary>
    ///Removes all items from the deque.
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
    ///Returns the item at the front of the deque without removing it.
    ///</summary>
    ///<returns>The item at the front.</returns>
    ///<exception cref="InvalidOperationException">Thrown when the deque is empty.</exception>
    public T PeekFirst()
    {
        if(_count == 0)
        {
            throw new InvalidOperationException("The deque is empty.");
        }

        return _buffer[_head];
    }

    ///<summary>
    ///Returns the item at the back of the deque without removing it.
    ///</summary>
    ///<returns>The item at the back.</returns>
    ///<exception cref="InvalidOperationException">Thrown when the deque is empty.</exception>
    public T PeekLast()
    {
        if(_count == 0)
        {
            throw new InvalidOperationException("The deque is empty.");
        }

        int tail = ((_head + _count) - 1) % _buffer.Length;
        return _buffer[tail];
    }

    ///<summary>
    ///Removes and returns the item at the front of the deque.
    ///</summary>
    ///<returns>The item that was at the front.</returns>
    ///<exception cref="InvalidOperationException">Thrown when the deque is empty.</exception>
    public T RemoveFirst()
    {
        if(_count == 0)
        {
            throw new InvalidOperationException("The deque is empty.");
        }

        T item = _buffer[_head];
        _buffer[_head] = default!;
        _head = (_head + 1) % _buffer.Length;
        _count--;
        return item;
    }

    ///<summary>
    ///Removes and returns the item at the back of the deque.
    ///</summary>
    ///<returns>The item that was at the back.</returns>
    ///<exception cref="InvalidOperationException">Thrown when the deque is empty.</exception>
    public T RemoveLast()
    {
        if(_count == 0)
        {
            throw new InvalidOperationException("The deque is empty.");
        }

        int tail = ((_head + _count) - 1) % _buffer.Length;
        T item = _buffer[tail];
        _buffer[tail] = default!;
        _count--;
        return item;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of items currently in the deque.
    ///</summary>
    public int Count => _count;

    ///<summary>
    ///Gets whether the deque contains no items.
    ///</summary>
    public bool IsEmpty => _count == 0;
    #endregion
}
