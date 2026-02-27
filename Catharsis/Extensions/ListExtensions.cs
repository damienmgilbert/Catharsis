namespace Catharsis.Extensions;

/// <summary>
/// Provides extension methods for <see cref="List{T}"/> and <see cref="LinkedList{T}"/>
/// to add, remove, and modify elements or ranges of elements.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Replaces a range of elements in the list starting at <paramref name="index"/> with <paramref name="items"/>.
    /// The existing elements in the range are removed and the FileName items are inserted in their place.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The target list.</param>
    /// <param name="index">The zero-based starting index of the range to replace.</param>
    /// <param name="count">The number of elements to remove.</param>
    /// <param name="items">The items to insert in place of the removed range.</param>
    /// <returns>The original <paramref name="source"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> or <paramref name="count"/> is invalid.</exception>
    public static List<T> ReplaceRange<T>(this List<T> source, int index, int count, IEnumerable<T> items)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        if (items is null) throw new ArgumentNullException(nameof(items), "Items must not be null.");
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Index must not be negative.");
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must not be negative.");
        if (index + count > source.Count) throw new ArgumentOutOfRangeException(nameof(count), "Range extends beyond the list bounds.");

        source.RemoveRange(index, count);
        source.InsertRange(index, items);
        return source;
    }

    /// <summary>
    /// Moves an element from <paramref name="fromIndex"/> to <paramref name="toIndex"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The target list.</param>
    /// <param name="fromIndex">The zero-based index of the element to move.</param>
    /// <param name="toIndex">The zero-based destination index.</param>
    /// <returns>The original <paramref name="source"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside the bounds of the list.</exception>
    public static List<T> MoveItem<T>(this List<T> source, int fromIndex, int toIndex)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        if (fromIndex < 0 || fromIndex >= source.Count) throw new ArgumentOutOfRangeException(nameof(fromIndex), "Source index is outside the bounds of the list.");
        if (toIndex < 0 || toIndex >= source.Count) throw new ArgumentOutOfRangeException(nameof(toIndex), "Destination index is outside the bounds of the list.");

        if (fromIndex == toIndex)
            return source;

        var item = source[fromIndex];
        source.RemoveAt(fromIndex);
        source.Insert(toIndex, item);
        return source;
    }

    /// <summary>
    /// Adds all elements from <paramref name="items"/> to the end of the linked list.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The target linked list.</param>
    /// <param name="items">The items to add.</param>
    /// <returns>The original <paramref name="source"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static LinkedList<T> AddRange<T>(this LinkedList<T> source, IEnumerable<T> items)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source linked list must not be null.");
        if (items is null) throw new ArgumentNullException(nameof(items), "Items must not be null.");

        foreach (var item in items)
            source.AddLast(item);

        return source;
    }

    /// <summary>
    /// Removes all elements matching <paramref name="predicate"/> from the linked list.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The target linked list.</param>
    /// <param name="predicate">A function that returns <c>true</c> for elements to remove.</param>
    /// <returns>The number of elements removed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static int RemoveWhere<T>(this LinkedList<T> source, Func<T, bool> predicate)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source linked list must not be null.");
        if (predicate is null) throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");

        int removed = 0;
        var node = source.First;
        while (node is not null)
        {
            var next = node.Next;
            if (predicate(node.Value))
            {
                source.Remove(node);
                removed++;
            }
            node = next;
        }

        return removed;
    }

    /// <summary>
    /// Modifies all elements in the linked list by replacing each node value with the result of <paramref name="modifier"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The target linked list.</param>
    /// <param name="modifier">A function that transforms each element.</param>
    /// <returns>The original <paramref name="source"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="modifier"/> is <c>null</c>.</exception>
    public static LinkedList<T> ModifyAll<T>(this LinkedList<T> source, Func<T, T> modifier)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source linked list must not be null.");
        if (modifier is null) throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");

        var node = source.First;
        while (node is not null)
        {
            node.Value = modifier(node.Value);
            node = node.Next;
        }

        return source;
    }
}
