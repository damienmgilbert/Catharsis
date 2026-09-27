namespace Catharsis.Collections;

///<summary>
///A collection that invokes a callback when an item is added. Wraps a <see cref="List{T}"/> and implements
///<see cref="ICollection{T}"/>.
///</summary>
///<typeparam name="T">The type of elements in the collection.</typeparam>
///<remarks>
///Initializes a new <see cref="TrackingCollection{T}"/> with the specified add callback.
///</remarks>
///<param name="onAdd">The action to invoke each time an item is added.</param>
public sealed class TrackingCollection<T>(Action<T> onAdd) : ICollection<T>
{
    #region Fields
    readonly List<T> _items = [];
    readonly Action<T> _onAdd = onAdd;

    #endregion
    #region Constructors
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Invokes the add callback and then adds the item to the collection.
    ///</summary>
    ///<param name="item">The item to add.</param>
    public void Add(T item)
    {
        _onAdd(item);
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
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public int Count => _items.Count;

    ///<inheritdoc/>
    public bool IsReadOnly => false;
    #endregion
}
