namespace Catharsis.Linq;

///<summary>
///Provides extension methods for converting and adapting between <see cref="IGrouping{TKey,TElement}"/>, ///<see
///cref="ILookup{TKey,TElement}"/>, and dictionary representations. Includes re-keying, projecting, filtering, and
///aggregating groupings.
///</summary>
public static class GroupingAdapter
{
    #region Public methods

    ///<summary>
    ///Aggregates each grouping into a single result per key.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<typeparam name="TResult">The aggregated result type.</typeparam>
    ///<param name="groupings">The source groupings.</param>
    ///<param name="aggregator">A function that aggregates the key and its elements into a result.</param>
    ///<returns>A sequence of aggregated results, one per group.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="groupings"/> or <paramref name="aggregator"/> is <c>null</c>.</exception>
    public static IEnumerable<TResult> AggregatePerGroup<TKey, TElement, TResult>(this IEnumerable<IGrouping<TKey, TElement>> groupings, Func<TKey, IEnumerable<TElement>, TResult> aggregator)
    {
        ArgumentNullException.ThrowIfNull(groupings, nameof(groupings));
        ArgumentNullException.ThrowIfNull(aggregator, nameof(aggregator));
        return groupings.Select(g => aggregator(g.Key, g));
    }

    ///<summary>
    ///Aggregates each group in a lookup into a single result per key.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<typeparam name="TResult">The aggregated result type.</typeparam>
    ///<param name="lookup">The source lookup.</param>
    ///<param name="aggregator">A function that aggregates the key and its elements into a result.</param>
    ///<returns>A sequence of aggregated results, one per group.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="lookup"/> or <paramref name="aggregator"/> is <c>null</c>.</exception>
    public static IEnumerable<TResult> AggregatePerGroup<TKey, TElement, TResult>(this ILookup<TKey, TElement> lookup, Func<TKey, IEnumerable<TElement>, TResult> aggregator)
    {
        ArgumentNullException.ThrowIfNull(lookup, nameof(lookup));
        ArgumentNullException.ThrowIfNull(aggregator, nameof(aggregator));
        return lookup.Select(g => aggregator(g.Key, g));
    }

    ///<summary>
    ///Projects elements within each grouping using <paramref name="elementSelector"/>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The original element type.</typeparam>
    ///<typeparam name="TResult">The projected element type.</typeparam>
    ///<param name="groupings">The source groupings.</param>
    ///<param name="elementSelector">A function that projects each element.</param>
    ///<returns>Groupings with projected elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="groupings"/> or <paramref name="elementSelector"/> is <c>null</c>.</exception>
    public static IEnumerable<IGrouping<TKey, TResult>> ProjectElements<TKey, TElement, TResult>(this IEnumerable<IGrouping<TKey, TElement>> groupings, Func<TElement, TResult> elementSelector)
    {
        ArgumentNullException.ThrowIfNull(groupings, nameof(groupings));
        ArgumentNullException.ThrowIfNull(elementSelector, nameof(elementSelector));
        return groupings.Select(g => SequenceFactory.Grouping(g.Key, g.Select(elementSelector)));
    }

    ///<summary>
    ///Projects elements within each group of a lookup.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The original element type.</typeparam>
    ///<typeparam name="TResult">The projected element type.</typeparam>
    ///<param name="lookup">The source lookup.</param>
    ///<param name="elementSelector">A function that projects each element.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>A new lookup with projected elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="lookup"/> or <paramref name="elementSelector"/> is <c>null</c>.</exception>
    public static ILookup<TKey, TResult> ProjectElements<TKey, TElement, TResult>(this ILookup<TKey, TElement> lookup, Func<TElement, TResult> elementSelector, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(lookup, nameof(lookup));
        ArgumentNullException.ThrowIfNull(elementSelector, nameof(elementSelector));
        return lookup
            .SelectMany(g => g.Select(e => (g.Key, Element: elementSelector(e))))
            .ToLookup(p => p.Key, p => p.Element, comparer);
    }

    ///<summary>
    ///Transforms the key of each grouping using <paramref name="keySelector"/>.
    ///</summary>
    ///<typeparam name="TKey">The original key type.</typeparam>
    ///<typeparam name="TNewKey">The new key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="groupings">The source groupings.</param>
    ///<param name="keySelector">A function that produces a new key from the original key.</param>
    ///<returns>A sequence of re-keyed groupings.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="groupings"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IEnumerable<IGrouping<TNewKey, TElement>> ReKey<TKey, TNewKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> groupings, Func<TKey, TNewKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(groupings, nameof(groupings));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return groupings.Select(g => SequenceFactory.Grouping(keySelector(g.Key), (IEnumerable<TElement>)g));
    }

    ///<summary>
    ///Transforms the key of each grouping in a lookup, producing a new lookup.
    ///</summary>
    ///<typeparam name="TKey">The original key type.</typeparam>
    ///<typeparam name="TNewKey">The new key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="lookup">The source lookup.</param>
    ///<param name="keySelector">A function that produces a new key from the original key.</param>
    ///<param name="comparer">An optional equality comparer for the new keys.</param>
    ///<returns>A new lookup with re-keyed groups.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="lookup"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static ILookup<TNewKey, TElement> ReKey<TKey, TNewKey, TElement>(this ILookup<TKey, TElement> lookup, Func<TKey, TNewKey> keySelector, IEqualityComparer<TNewKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(lookup, nameof(lookup));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return lookup
            .SelectMany(g => g.Select(e => (Key: keySelector(g.Key), Element: e)))
            .ToLookup(p => p.Key, p => p.Element, comparer);
    }

    ///<summary>
    ///Converts an <see cref="ILookup{TKey,TElement}"/> to a <see cref="Dictionary{TKey,TValue}"/> of lists.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="lookup">The lookup to convert.</param>
    ///<returns>A dictionary mapping each key to its element list.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c>.</exception>
    public static Dictionary<TKey, List<TElement>> ToDictionary<TKey, TElement>(this ILookup<TKey, TElement> lookup) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(lookup, nameof(lookup));
        return lookup.ToDictionary(static g => g.Key, static g => g.ToList());
    }

    ///<summary>
    ///Converts a sequence of <see cref="IGrouping{TKey,TElement}"/> to a ///<see cref="Dictionary{TKey,TValue}"/> of
    ///lists.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="groupings">The groupings to convert.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>A dictionary mapping each key to its element list.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="groupings"/> is <c>null</c>.</exception>
    public static Dictionary<TKey, List<TElement>> ToDictionary<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> groupings, IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(groupings, nameof(groupings));

        Dictionary<TKey, List<TElement>> result = [ with(comparer) ];

        foreach(IGrouping<TKey, TElement> group in groupings)
        {
            if(result.TryGetValue(group.Key, out List<TElement>? existing))
            {
                existing.AddRange(group);
            } else
            {
                result[group.Key] = [ .. group ];
            }
        }

        return result;
    }

    ///<summary>
    ///Converts an <see cref="ILookup{TKey,TElement}"/> to a sequence of ///<see cref="IGrouping{TKey,TElement}"/>
    ///instances.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="lookup">The lookup to convert.</param>
    ///<returns>A sequence of groupings.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c>.</exception>
    public static IEnumerable<IGrouping<TKey, TElement>> ToGroupings<TKey, TElement>(this ILookup<TKey, TElement> lookup)
    {
        ArgumentNullException.ThrowIfNull(lookup, nameof(lookup));
        return lookup;
    }

    ///<summary>
    ///Converts an <see cref="IGrouping{TKey,TElement}"/> to a <see cref="KeyValuePair{TKey,TValue}"/> where the value
    ///is a <see cref="List{T}"/>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="grouping">The grouping to convert.</param>
    ///<returns>A key-value pair containing the key and materialized element list.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="grouping"/> is <c>null</c>.</exception>
    public static KeyValuePair<TKey, List<TElement>> ToKeyValuePair<TKey, TElement>(this IGrouping<TKey, TElement> grouping)
    {
        ArgumentNullException.ThrowIfNull(grouping, nameof(grouping));
        return KeyValuePair.Create(grouping.Key, grouping.ToList());
    }

    ///<summary>
    ///Converts a sequence of <see cref="IGrouping{TKey,TElement}"/> to an <see cref="ILookup{TKey,TElement}"/>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="groupings">The groupings to convert.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="groupings"/> is <c>null</c>.</exception>
    public static ILookup<TKey, TElement> ToLookup<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> groupings, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(groupings, nameof(groupings));
        return LookupFactory.FromGroupings(groupings, comparer);
    }

    ///<summary>
    ///Filters groupings by minimum group size.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="groupings">The source groupings.</param>
    ///<param name="minCount">The minimum number of elements a group must have.</param>
    ///<returns>Groupings with at least <paramref name="minCount"/> elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="groupings"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="minCount"/> is negative.</exception>
    public static IEnumerable<IGrouping<TKey, TElement>> WhereCountAtLeast<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> groupings, int minCount)
    {
        ArgumentNullException.ThrowIfNull(groupings, nameof(groupings));
        ArgumentOutOfRangeException.ThrowIfNegative(minCount, nameof(minCount));
        return groupings.Where(g => g.Count() >= minCount);
    }

    ///<summary>
    ///Filters groupings by key predicate, keeping only groups whose key satisfies <paramref name="keyPredicate"/>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="groupings">The source groupings.</param>
    ///<param name="keyPredicate">A function that tests the key.</param>
    ///<returns>Groupings whose key satisfies the predicate.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="groupings"/> or <paramref name="keyPredicate"/> is <c>null</c>.</exception>
    public static IEnumerable<IGrouping<TKey, TElement>> WhereKey<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> groupings, Func<TKey, bool> keyPredicate)
    {
        ArgumentNullException.ThrowIfNull(groupings, nameof(groupings));
        ArgumentNullException.ThrowIfNull(keyPredicate, nameof(keyPredicate));
        return groupings.Where(g => keyPredicate(g.Key));
    }
    #endregion
}
