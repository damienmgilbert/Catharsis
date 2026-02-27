namespace Catharsis.Collections;

public class TrackingCollection<T> : ICollection<T>
{
    #region Fields
    readonly List<T> _items = [];
    readonly Action<T> _onAdd;
    #endregion

    #region Constructors
    public TrackingCollection(Action<T> onAdd) { _onAdd = onAdd; }
    #endregion

    #region Explicit interface implementations
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    public void Add(T item)
    {
        _onAdd(item);
        _items.Add(item);
    }

    public void Clear() { _items.Clear(); }
    public bool Contains(T item) { return _items.Contains(item); }
    public void CopyTo(T[] array, int arrayIndex) { _items.CopyTo(array, arrayIndex); }
    public IEnumerator<T> GetEnumerator() { return _items.GetEnumerator(); }
    public bool Remove(T item) { return _items.Remove(item); }
    #endregion

    #region Public properties
    public int Count => _items.Count;

    public bool IsReadOnly => false;
    #endregion
}
