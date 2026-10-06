namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for <see cref="HashSet{T}"/>, <see cref="SortedSet{T}"/>, and <see cref="ISet{T}"/> to
///add, remove, and modify elements.
///</summary>
public static class SetExtensions
{
    #region Public methods

    ///<summary>
    ///Adds all elements from <paramref name="items"/> to the set.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target set.</param>
    ///<param name="items">The items to add.</param>
    ///<returns>The number of elements actually added (duplicates are skipped).</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static int AddRange<T>(this ISet<T> source, IEnumerable<T> items)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source set must not be null.");
        }

        if (items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        int added = 0;
        foreach (T item in items)
        {
            if (source.Add(item))
            {
                added++;
            }
        }

        return added;
    }

    ///<summary>
    ///Removes all elements from <paramref name="items"/> from the set.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target set.</param>
    ///<param name="items">The items to remove.</param>
    ///<returns>The number of elements actually removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static int RemoveRange<T>(this ISet<T> source, IEnumerable<T> items)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source set must not be null.");
        }

        if (items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        int removed = 0;
        foreach (T item in items)
        {
            if (source.Remove(item))
            {
                removed++;
            }
        }

        return removed;
    }

    ///<summary>
    ///Removes all elements matching <paramref name="predicate"/> from the <see cref="HashSet{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target hash set.</param>
    ///<param name="predicate">A function that returns <c>true</c> for elements to remove.</param>
    ///<returns>The number of elements removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static int RemoveWhere<T>(this HashSet<T> source, Func<T, bool> predicate)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source set must not be null.");
        }

        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        return source.RemoveWhere(new Predicate<T>(predicate));
    }

    ///<summary>
    ///Removes all elements matching <paramref name="predicate"/> from the <see cref="SortedSet{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target sorted set.</param>
    ///<param name="predicate">A function that returns <c>true</c> for elements to remove.</param>
    ///<returns>The number of elements removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static int RemoveWhere<T>(this SortedSet<T> source, Func<T, bool> predicate)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source set must not be null.");
        }

        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        return source.RemoveWhere(new Predicate<T>(predicate));
    }

    ///<summary>
    ///Replaces the contents of the set with <paramref name="items"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target set.</param>
    ///<param name="items">The items that replace the existing contents.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static ISet<T> ReplaceWith<T>(this ISet<T> source, IEnumerable<T> items)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source set must not be null.");
        }

        if (items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        source.Clear();
        foreach (T item in items)
        {
            source.Add(item);
        }

        return source;
    }

    ///<summary>
    ///Adds the element if it is not present; removes it if it is present (toggle behavior).
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target set.</param>
    ///<param name="item">The item to toggle.</param>
    ///<returns><c>true</c> if the item was added; <c>false</c> if it was removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static bool Toggle<T>(this ISet<T> source, T item)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source set must not be null.");
        }

        if (!source.Remove(item))
        {
            source.Add(item);
            return true;
        }

        return false;
    }
    #endregion
}
