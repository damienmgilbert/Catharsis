using System.Collections;

namespace Catharsis.Collections;

/// <summary>
/// A collection that raises events when items are added or removed.
/// </summary>
/// <typeparam name="T">The type of elements stored in the collection.</typeparam>
public class EventCollection<T> : ICollection<T>, IReadOnlyCollection<T>
{
    private readonly List<T> _items = [];

    /// <summary>
    /// Raised after an item has been added to the collection.
    /// </summary>
    public event EventHandler<T>? ItemAdded;

    /// <summary>
    /// Raised after an item has been removed from the collection.
    /// </summary>
    public event EventHandler<T>? ItemRemoved;

    /// <summary>
    /// Raised after the collection has been cleared.
    /// </summary>
    public event EventHandler? Cleared;

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds an item to the collection and raises <see cref="ItemAdded"/>.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        _items.Add(item);
        ItemAdded?.Invoke(this, item);
    }

    /// <summary>
    /// Removes the first occurrence of an item from the collection
    /// and raises <see cref="ItemRemoved"/> if successful.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><c>true</c> if the item was found and removed; otherwise <c>false</c>.</returns>
    public bool Remove(T item)
    {
        if (!_items.Remove(item))
            return false;

        ItemRemoved?.Invoke(this, item);
        return true;
    }

    /// <summary>
    /// Removes all items from the collection and raises <see cref="Cleared"/>.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        Cleared?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public bool Contains(T item) => _items.Contains(item);

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
