namespace Catharsis.Linq;

///<summary>
///Provides extension methods for partitioning, splitting, and segmenting <see cref="IEnumerable{T}"/>, ///<see
///cref="IOrderedEnumerable{TElement}"/>, and <see cref="IGrouping{TKey,TElement}"/> sequences into logical groups based
///on predicates, keys, counts, or boundaries.
///</summary>
public static class SequencePartition
{
    #region Private methods
    private static IEnumerable<IGrouping<TKey, T>> ChunkByIterator<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            yield break;
        }

        TKey currentKey = keySelector(enumerator.Current);
        List<T> currentChunk = [enumerator.Current];

        while (enumerator.MoveNext())
        {
            TKey key = keySelector(enumerator.Current);

            if (comparer.Equals(key, currentKey))
            {
                currentChunk.Add(enumerator.Current);
            }
            else
            {
                yield return SequenceFactory.Grouping(currentKey, (IEnumerable<T>)currentChunk);
                currentKey = key;
                currentChunk = [enumerator.Current];
            }
        }

        yield return SequenceFactory.Grouping(currentKey, (IEnumerable<T>)currentChunk);
    }

    private static IEnumerable<IReadOnlyList<T>> SplitByIterator<T>(IEnumerable<T> source, T separator, IEqualityComparer<T> comparer)
    {
        List<T> segment = [];

        foreach (T item in source)
        {
            if (comparer.Equals(item, separator))
            {
                yield return segment.AsReadOnly();
                segment = [];
            }
            else
            {
                segment.Add(item);
            }
        }

        yield return segment.AsReadOnly();
    }

    private static IEnumerable<IReadOnlyList<T>> SplitWhenIterator<T>(IEnumerable<T> source, Func<T, bool> predicate)
    {
        List<T> segment = [];

        foreach (T item in source)
        {
            if (predicate(item) && segment.Count > 0)
            {
                yield return segment.AsReadOnly();
                segment = [];
            }

            segment.Add(item);
        }

        if (segment.Count > 0)
        {
            yield return segment.AsReadOnly();
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Groups consecutive elements that share the same key into segments. Unlike <see
    ///cref="Enumerable.GroupBy{TSource,TKey}(IEnumerable{TSource},Func{TSource,TKey})"/>, this method only groups
    ///adjacent elements and produces a new group each time the key changes.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="keySelector">A function that extracts a key from each element.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>A sequence of <see cref="IGrouping{TKey, TElement}"/> representing consecutive runs.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IEnumerable<IGrouping<TKey, T>> ChunkBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return ChunkByIterator(source, keySelector, comparer ?? EqualityComparer<TKey>.Default);
    }

    ///<summary>
    ///Splits a sequence into two lists: one containing elements that satisfy <paramref name="predicate"/> and one
    ///containing elements that do not.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="predicate">A function that tests each element.</param>
    ///<returns>
    ///A tuple of <c>(Matched, Unmatched)</c> where <c>Matched</c> contains elements for which <paramref
    ///name="predicate"/> returned <c>true</c> and <c>Unmatched</c> contains the rest.
    ///</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static (List<T> Matched, List<T> Unmatched) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        List<T> matched = [];
        List<T> unmatched = [];

        foreach (T item in source)
        {
            if (predicate(item))
            {
                matched.Add(item);
            }
            else
            {
                unmatched.Add(item);
            }
        }

        return (matched, unmatched);
    }

    ///<summary>
    ///Splits an <see cref="IOrderedEnumerable{TElement}"/> into two lists by predicate, preserving order.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The ordered source sequence.</param>
    ///<param name="predicate">A function that tests each element.</param>
    ///<returns>A tuple of <c>(Matched, Unmatched)</c> lists.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static (List<T> Matched, List<T> Unmatched) Partition<T>(this IOrderedEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return ((IEnumerable<T>)source).Partition(predicate);
    }

    ///<summary>
    ///Distributes elements evenly into <paramref name="groupCount"/> partitions in round-robin order. Each partition is
    ///returned as a list.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="groupCount">The number of partitions. Must be at least 1.</param>
    ///<returns>A list of <paramref name="groupCount"/> partitions.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="groupCount"/> is less than 1.</exception>
    public static List<List<T>> PartitionEvenly<T>(this IEnumerable<T> source, int groupCount)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(groupCount, 1, nameof(groupCount));

        List<List<T>> partitions = new(groupCount);

        for (int i = 0; i < groupCount; i++)
        {
            partitions.Add([]);
        }

        int index = 0;

        foreach (T item in source)
        {
            partitions[index % groupCount].Add(item);
            index++;
        }

        return partitions;
    }

    ///<summary>
    ///Partitions groups in a grouped sequence by applying a predicate to each group's key. Groups whose key matches go
    ///into <c>Matched</c>; the rest go into <c>Unmatched</c>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="source">The grouped sequence.</param>
    ///<param name="keyPredicate">A function that tests the group key.</param>
    ///<returns>A tuple of matched and unmatched grouping lists.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keyPredicate"/> is <c>null</c>.</exception>
    public static (List<IGrouping<TKey, TElement>> Matched, List<IGrouping<TKey, TElement>> Unmatched) PartitionGroups<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> source, Func<TKey, bool> keyPredicate)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keyPredicate, nameof(keyPredicate));

        List<IGrouping<TKey, TElement>> matched = [];
        List<IGrouping<TKey, TElement>> unmatched = [];

        foreach (IGrouping<TKey, TElement> group in source)
        {
            if (keyPredicate(group.Key))
            {
                matched.Add(group);
            }
            else
            {
                unmatched.Add(group);
            }
        }

        return (matched, unmatched);
    }

    ///<summary>
    ///Splits a sequence into a prefix and a suffix at the point where <paramref name="predicate"/> first returns
    public static (List<T> Prefix, List<T> Suffix) Span<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        List<T> prefix = [];
        List<T> suffix = [];
        bool switched = false;

        foreach (T item in source)
        {
            if (!switched && predicate(item))
            {
                prefix.Add(item);
            }
            else
            {
                switched = true;
                suffix.Add(item);
            }
        }

        return (prefix, suffix);
    }

    ///<summary>
    ///Splits a sequence into two lists at the specified zero-based index. The first list contains ///<paramref
    ///name="index"/> elements, and the second list contains the rest.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="index">The zero-based index at which to split. Must not be negative.</param>
    ///<returns>A tuple of <c>(Before, After)</c> lists.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative.</exception>
    public static (List<T> Before, List<T> After) SplitAt<T>(this IEnumerable<T> source, int index)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));

        List<T> before = [];
        List<T> after = [];
        int i = 0;

        foreach (T item in source)
        {
            if (i < index)
            {
                before.Add(item);
            }
            else
            {
                after.Add(item);
            }

            i++;
        }

        return (before, after);
    }

    ///<summary>
    ///Splits a sequence at every occurrence of <paramref name="separator"/>. The separator elements are not included in
    ///the output.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="separator">The element that marks a split point.</param>
    ///<param name="comparer">An optional equality comparer; defaults to the default comparer for <typeparamref name="T"/>.</param>
    ///<returns>A sequence of segments (lists) between separator elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<IReadOnlyList<T>> SplitBy<T>(this IEnumerable<T> source, T separator, IEqualityComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return SplitByIterator(source, separator, comparer ?? EqualityComparer<T>.Default);
    }

    ///<summary>
    ///Splits a sequence every time <paramref name="predicate"/> returns <c>true</c>. The triggering element begins the
    ///next segment.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="predicate">A function that returns <c>true</c> at split points.</param>
    ///<returns>A sequence of segments.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static IEnumerable<IReadOnlyList<T>> SplitWhen<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return SplitWhenIterator(source, predicate);
    }
    #endregion
}
