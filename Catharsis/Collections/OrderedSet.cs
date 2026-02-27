using System.Collections;

namespace Catharsis.Collections;

///<summary>
///An insertion-ordered set that maintains uniqueness while preserving the order in which items were added. Duplicate
///additions are silently ignored.
///</summary>
///<typeparam name="T">The type of elements stored in the set.</typeparam>
public class OrderedSet<T> : ICollection<T>, IReadOnlyCollection<T>
{
    #region Fields
    readonly List<T> _items = [];
    readonly HashSet<T> _set;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName <see cref="OrderedSet{T}"/> using the default equality comparer.
    ///</summary>
    public OrderedSet() : this(EqualityComparer<T>.Default)
    {
    }

    ///<summary>
    ///Initializes a FileName <see cref="OrderedSet{T}"/> with the specified equality comparer.
    ///</summary>
    ///<param name="comparer">The comparer used to determine element equality.</param>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="comparer"/> is <c>null</c>.</exception>
    public OrderedSet(IEqualityComparer<T> comparer)
    {
        if(comparer is null)
        {
            throw new ArgumentNullException(nameof(comparer), "Equality comparer must not be null.");
        }

        _set = new HashSet<T>(comparer);
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets the item at the specified index in insertion order.
    ///</summary>
    ///<param name="index">The zero-based index.</param>
    ///<returns>The item at <paramref name="index"/>.</returns>
    ///<exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is out of range.</exception>
    public T this[int index] => _items[index];
    #endregion

    #region Explicit interface implementations
    ///<summary>
    ///Adds an item to the set. Duplicates are silently ignored.
    ///</summary>
    ///<param name="item">The item to add.</param>
    void ICollection<T>.Add(T item) { TryAdd(item); }
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Clear()
    {
        _items.Clear();
        _set.Clear();
    }

    ///<inheritdoc/>
    public bool Contains(T item) { return _set.Contains(item); }
    ///<inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex) { _items.CopyTo(array, arrayIndex); }
    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator() { return _items.GetEnumerator(); }

    ///<inheritdoc/>
    public bool Remove(T item)
    {
        if(!_set.Remove(item))
        {
            return false;
        }

        _items.Remove(item);
        return true;
    }

    ///<summary>
    ///Adds an item to the set if it is not already present.
    ///</summary>
    ///<param name="item">The item to add.</param>
    ///<returns><c>true</c> if the item was added; <c>false</c> if it was already in the set.</returns>
    public bool TryAdd(T item)
    {
        if(!_set.Add(item))
        {
            return false;
        }

        _items.Add(item);
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
