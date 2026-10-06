using System.Collections;

namespace Catharsis.Collections;

///<summary>
///A list that automatically maintains its elements in sorted order using an <see cref="IComparer{T}"/>. Duplicate
///values are permitted. Insertions use binary search to find the correct position, giving O(log n) lookup and O(n)
///insertion.
///</summary>
///<typeparam name="T">The type of elements stored in the list.</typeparam>
public sealed class SortedList<T> : ICollection<T>, IReadOnlyList<T>
{
    #region Fields
    readonly IComparer<T> _comparer;
    readonly List<T> _items = [];
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="SortedList{T}"/> using the default comparer for <typeparamref name="T"/>.
    ///</summary>
    public SortedList() : this(Comparer<T>.Default)
    {
    }

    ///<summary>
    ///Initializes a new <see cref="SortedList{T}"/> with the specified comparer.
    ///</summary>
    ///<param name="comparer">The comparer used to determine element order.</param>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="comparer"/> is <c>null</c>.</exception>
    public SortedList(IComparer<T> comparer)
    {
        if (comparer is null)
        {
            throw new ArgumentNullException(nameof(comparer), "Comparer must not be null.");
        }

        _comparer = comparer;
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets the element at the specified index in sorted order.
    ///</summary>
    ///<param name="index">The zero-based index.</param>
    ///<returns>The element at <paramref name="index"/>.</returns>
    ///<exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is out of range.</exception>
    public T this[int index] => _items[index];
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds an element to the list in its sorted position.
    ///</summary>
    ///<param name="item">The element to add.</param>
    public void Add(T item)
    {
        int index = _items.BinarySearch(item, _comparer);

        if (index < 0)
        {
            index = ~index;
        }

        _items.Insert(index, item);
    }

    ///<inheritdoc/>
    public void Clear() { _items.Clear(); }

    ///<inheritdoc/>
    public bool Contains(T item) { return _items.BinarySearch(item, _comparer) >= 0; }

    ///<inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex) { _items.CopyTo(array, arrayIndex); }

    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator() { return _items.GetEnumerator(); }

    ///<summary>
    ///Returns the zero-based index of the first occurrence of the specified element using binary search.
    ///</summary>
    ///<param name="item">The element to locate.</param>
    ///<returns>The index of <paramref name="item"/> if found; otherwise <c>-1</c>.</returns>
    public int IndexOf(T item)
    {
        int index = _items.BinarySearch(item, _comparer);
        return index >= 0 ? index : -1;
    }

    ///<inheritdoc/>
    public bool Remove(T item)
    {
        int index = _items.BinarySearch(item, _comparer);

        if (index < 0)
        {
            return false;
        }

        _items.RemoveAt(index);
        return true;
    }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public int Count => _items.Count;

    ///<inheritdoc/>
    public bool IsReadOnly => false;
    #endregion
}
