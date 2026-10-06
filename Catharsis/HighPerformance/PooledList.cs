using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;
using CommunityToolkit.Diagnostics;

namespace Catharsis.HighPerformance;

///<summary>
///A list backed by <see cref="ArrayPool{T}"/> for reduced allocation pressure in high-throughput scenarios.
///</summary>
///<typeparam name="T">The type of elements in the list.</typeparam>
public sealed class PooledList<T> : IList<T>, IReadOnlyList<T>, IDisposable
{
    #region Fields
    private int _count;
    private bool _disposed;
    private T[] _items;
    private readonly ArrayPool<T> _pool;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="PooledList{T}"/> with the specified initial capacity.
    ///</summary>
    ///<param name="initialCapacity">The initial capacity of the list.</param>
    public PooledList(int initialCapacity = 16) : this(ArrayPool<T>.Shared, initialCapacity)
    {
    }

    ///<summary>
    ///Initializes a new <see cref="PooledList{T}"/> with the specified pool and capacity.
    ///</summary>
    ///<param name="pool">The array pool to rent from.</param>
    ///<param name="initialCapacity">The initial capacity.</param>
    public PooledList(ArrayPool<T> pool, int initialCapacity = 16)
    {
        Guard.IsNotNull(pool);
        Guard.IsGreaterThan(initialCapacity, 0);

        _pool = pool;
        _items = _pool.Rent(initialCapacity);
    }
    #endregion

    #region Indexers
    ///<inheritdoc/>
    public T this[int index]
    {
        get
        {
            Guard.IsInRange(index, 0, _count);
            return _items[index];
        }
        set
        {
            Guard.IsInRange(index, 0, _count);
            _items[index] = value;
        }
    }
    #endregion

    #region Explicit interface implementations
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Private methods
    private void EnsureCapacity(int required)
    {
        if(required <= _items.Length)
        {
            return;
        }

        int newSize = Math.Max(_items.Length * 2, required);
        T[] newArray = _pool.Rent(newSize);
        Array.Copy(_items, newArray, _count);
        _pool.Return(_items, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        _items = newArray;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Add(T item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureCapacity(_count + 1);
        _items[_count++] = item;
    }

    ///<summary>
    ///Adds a span of items to the list.
    ///</summary>
    ///<param name="items">The items to add.</param>
    public void AddRange(ReadOnlySpan<T> items)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureCapacity(_count + items.Length);
        items.CopyTo(_items.AsSpan(_count));
        _count += items.Length;
    }

    ///<inheritdoc/>
    public void Clear()
    {
        if(RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(_items, 0, _count);
        }

        _count = 0;
    }

    ///<inheritdoc/>
    public bool Contains(T item) => IndexOf(item) >= 0;

    ///<inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex) => Array.Copy(_items, 0, array, arrayIndex, _count);

    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;

        _pool.Return(_items, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        _items = [];
        _count = 0;
    }

    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        for(int i = 0; i < _count; i++)
        {
            yield return _items[i];
        }
    }

    ///<inheritdoc/>
    public int IndexOf(T item) => Array.IndexOf(_items, item, 0, _count);

    ///<inheritdoc/>
    public void Insert(int index, T item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.IsInRange(index, 0, _count + 1);
        EnsureCapacity(_count + 1);

        if(index < _count)
        {
            Array.Copy(_items, index, _items, index + 1, _count - index);
        }

        _items[index] = item;
        _count++;
    }

    ///<inheritdoc/>
    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if(index < 0)
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    ///<inheritdoc/>
    public void RemoveAt(int index)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.IsInRange(index, 0, _count);

        _count--;
        if(index < _count)
        {
            Array.Copy(_items, index + 1, _items, index, _count - index);
        }

        if(RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _items[_count] = default!;
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the current capacity of the internal array.
    ///</summary>
    public int Capacity => _items.Length;

    ///<summary>
    ///Gets the number of elements in the list.
    ///</summary>
    public int Count => _count;

    ///<inheritdoc/>
    public bool IsReadOnly => false;

    ///<summary>
    ///Gets a span over the current elements of the list.
    ///</summary>
    public Span<T> Span => _items.AsSpan(0, _count);
    #endregion
}
