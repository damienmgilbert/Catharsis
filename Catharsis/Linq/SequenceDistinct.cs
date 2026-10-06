namespace Catharsis.Linq;

///<summary>
///Provides extension methods for producing distinct elements from <see cref="IEnumerable{T}"/>, ///<see
///cref="IOrderedEnumerable{TElement}"/>, and <see cref="IGrouping{TKey,TElement}"/> sequences based on a projected key,
///supporting custom equality comparers.
///</summary>
public static class SequenceDistinct
{
    #region Private methods
    private static IEnumerable<T> DistinctByKeyIterator<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        HashSet<TKey> seen = [with(comparer)];

        foreach (T item in source)
        {
            if (seen.Add(keySelector(item)))
            {
                yield return item;
            }
        }
    }

    private static IEnumerable<IGrouping<TGroupKey, TElement>> DistinctByKeyPerGroupIterator<TGroupKey, TElement, TKey>(IEnumerable<IGrouping<TGroupKey, TElement>> source, Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        foreach (IGrouping<TGroupKey, TElement> group in source)
        {
            IEnumerable<TElement> distinct = DistinctByKeyIterator(group, keySelector, comparer);
            yield return SequenceFactory.Grouping(group.Key, distinct);
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Counts the number of elements for each distinct key.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="keySelector">A function that extracts the key from each element.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>A dictionary mapping each distinct key to its occurrence count.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static Dictionary<TKey, int> CountByKey<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));

        Dictionary<TKey, int> counts = [with(comparer ?? EqualityComparer<TKey>.Default)];

        foreach (T item in source)
        {
            TKey key = keySelector(item);

            if (counts.TryGetValue(key, out int count))
            {
                counts[key] = count + 1;
            }
            else
            {
                counts[key] = 1;
            }
        }

        return counts;
    }

    ///<summary>
    ///Returns distinct elements from a sequence based on a projected key. The first element with each unique key is
    ///retained.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The key type used for equality comparison.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="keySelector">A function that extracts the comparison key from each element.</param>
    ///<param name="comparer">An optional equality comparer for keys; defaults to the default comparer.</param>
    ///<returns>A sequence of elements with distinct keys.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IEnumerable<T> DistinctByKey<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return DistinctByKeyIterator(source, keySelector, comparer ?? EqualityComparer<TKey>.Default);
    }

    ///<summary>
    ///Returns distinct elements from an ordered sequence based on a projected key, preserving the existing order.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The key type used for equality comparison.</typeparam>
    ///<param name="source">The ordered source sequence.</param>
    ///<param name="keySelector">A function that extracts the comparison key from each element.</param>
    ///<param name="comparer">An optional equality comparer for keys; defaults to the default comparer.</param>
    ///<returns>An ordered sequence of elements with distinct keys.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IEnumerable<T> DistinctByKey<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return DistinctByKeyIterator(source, keySelector, comparer ?? EqualityComparer<TKey>.Default);
    }

    ///<summary>
    ///Returns distinct elements based on a projected key, using <paramref name="duplicateResolver"/> to choose which
    ///element to keep when a duplicate key is encountered.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="keySelector">A function that extracts the comparison key from each element.</param>
    ///<param name="duplicateResolver">
    ///A function that receives the existing element and the new duplicate and returns the element to keep.
    ///</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>A sequence of elements with distinct keys, resolved by <paramref name="duplicateResolver"/>.</returns>
    ///<exception cref="ArgumentNullException">Any delegate argument is <c>null</c>.</exception>
    public static IEnumerable<T> DistinctByKey<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, Func<T, T, T> duplicateResolver, IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        ArgumentNullException.ThrowIfNull(duplicateResolver, nameof(duplicateResolver));

        Dictionary<TKey, T> seen = [with(comparer ?? EqualityComparer<TKey>.Default)];

        foreach (T item in source)
        {
            TKey key = keySelector(item);

            if (seen.TryGetValue(key, out T? existing))
            {
                seen[key] = duplicateResolver(existing, item);
            }
            else
            {
                seen[key] = item;
            }
        }

        return seen.Values;
    }

    ///<summary>
    ///Applies <see cref="DistinctByKey{T,TKey}(IEnumerable{T},Func{T,TKey},IEqualityComparer{TKey}?)"/> within each
    ///group of a grouped sequence, returning new groupings with distinct elements per group.
    ///</summary>
    ///<typeparam name="TGroupKey">The group key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<typeparam name="TKey">The key type for distinctness.</typeparam>
    ///<param name="source">The grouped sequence.</param>
    ///<param name="keySelector">A function that extracts the distinctness key from each element.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>A sequence of groupings where each group contains only distinct elements by key.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IEnumerable<IGrouping<TGroupKey, TElement>> DistinctByKeyPerGroup<TGroupKey, TElement, TKey>(this IEnumerable<IGrouping<TGroupKey, TElement>> source, Func<TElement, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return DistinctByKeyPerGroupIterator(source, keySelector, comparer ?? EqualityComparer<TKey>.Default);
    }
    #endregion
}
