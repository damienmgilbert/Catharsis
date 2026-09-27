using System.Collections;

namespace Catharsis.Collections;

///<summary>
///A collection with a fixed maximum capacity that rejects additions once the limit is reached.
///</summary>
///<typeparam name="T">The type of elements stored in the collection.</typeparam>
public class BoundedCollection<T> : ICollection<T>, IReadOnlyCollection<T>
{
    #region Fields
    readonly List<T> _items;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="BoundedCollection{T}"/> with the specified maximum capacity.
    ///</summary>
    ///<param name="maxCapacity">The maximum number of items allowed. Must be greater than zero.</param>
    ///<exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxCapacity"/> is less than or equal to zero.</exception>
    public BoundedCollection(int maxCapacity)
    {
        if (maxCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCapacity), "Maximum capacity must be greater than zero.");
        }

        MaxCapacity = maxCapacity;
        _items = [with(maxCapacity)];
    }
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds an item to the collection.
    ///</summary>
    ///<param name="item">The item to add.</param>
    ///<exception cref="InvalidOperationException">Thrown when the collection is already at maximum capacity.</exception>
    public void Add(T item)
    {
        if (IsFull)
        {
            throw new InvalidOperationException($"The collection has reached its maximum capacity of {MaxCapacity}.");
        }

        _items.Add(item);
    }

    ///<inheritdoc/>
    public void Clear() { _items.Clear(); }
    ///<inheritdoc/>
    public bool Contains(T item) { return _items.Contains(item); }
    ///<inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex) { _items.CopyTo(array, arrayIndex); }
    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator() { return _items.GetEnumerator(); }
    ///<inheritdoc/>
    public bool Remove(T item) { return _items.Remove(item); }

    ///<summary>
    ///Attempts to add an item to the collection without throwing if full.
    ///</summary>
    ///<param name="item">The item to add.</param>
    ///<returns><c>true</c> if the item was added; <c>false</c> if the collection is full.</returns>
    public bool TryAdd(T item)
    {
        if (IsFull)
        {
            return false;
        }

        _items.Add(item);
        return true;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the current number of items in the collection.
    ///</summary>
    public int Count => _items.Count;

    ///<summary>
    ///Gets whether the collection has reached its maximum capacity.
    ///</summary>
    public bool IsFull => _items.Count >= MaxCapacity;

    ///<inheritdoc/>
    public bool IsReadOnly => false;

    ///<summary>
    ///Gets the maximum number of items this collection can hold.
    ///</summary>
    public int MaxCapacity { get; }
    #endregion
}
