namespace Catharsis.Collections;

public class TrackingCollection<T> : ICollection<T>
{
    private readonly List<T> _items = [];
    private readonly Action<T> _onAdd;

    public TrackingCollection(Action<T> onAdd) => _onAdd = onAdd;

    public int Count => _items.Count;
    public bool IsReadOnly => false;

    public void Add(T item)
    {
        _onAdd(item);
        _items.Add(item);
    }

    public void Clear() => _items.Clear();
    public bool Contains(T item) => _items.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    public bool Remove(T item) => _items.Remove(item);
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
