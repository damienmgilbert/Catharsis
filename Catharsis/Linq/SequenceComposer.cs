namespace Catharsis.Linq;

///<summary>
///Provides extension methods for composing, merging, interleaving, and zipping <see cref="IEnumerable{T}"/>, ///<see
///cref="IOrderedEnumerable{TElement}"/>, and <see cref="IGrouping{TKey,TElement}"/> sequences.
///</summary>
public static class SequenceComposer
{
    #region Private methods
    private static IEnumerable<T> InterleaveIterator<T>(IEnumerable<T> first, IEnumerable<T> second)
    {
        using IEnumerator<T> e1 = first.GetEnumerator();
        using IEnumerator<T> e2 = second.GetEnumerator();

        bool has1 = e1.MoveNext();
        bool has2 = e2.MoveNext();

        while (has1 && has2)
        {
            yield return e1.Current;
            yield return e2.Current;
            has1 = e1.MoveNext();
            has2 = e2.MoveNext();
        }

        while (has1)
        {
            yield return e1.Current;
            has1 = e1.MoveNext();
        }

        while (has2)
        {
            yield return e2.Current;
            has2 = e2.MoveNext();
        }
    }

    private static IEnumerable<T> InterleaveManyIterator<T>(IEnumerable<T> source, IEnumerable<T>[] others)
    {
        List<IEnumerator<T>> enumerators = [source.GetEnumerator(), .. others.Select(static s => s.GetEnumerator())];

        try
        {
            List<IEnumerator<T>> active = [.. enumerators];

            while (active.Count > 0)
            {
                for (int i = active.Count - 1; i >= 0; i--)
                {
                    if (active[i].MoveNext())
                    {
                        yield return active[i].Current;
                    }
                    else
                    {
                        active.RemoveAt(i);
                    }
                }
            }
        }
        finally
        {
            foreach (IEnumerator<T> e in enumerators)
            {
                e.Dispose();
            }
        }
    }

    private static IEnumerable<T> MergeOrderedIterator<T>(IEnumerable<T> first, IEnumerable<T> second, IComparer<T> comparer)
    {
        using IEnumerator<T> e1 = first.GetEnumerator();
        using IEnumerator<T> e2 = second.GetEnumerator();

        bool has1 = e1.MoveNext();
        bool has2 = e2.MoveNext();

        while (has1 && has2)
        {
            if (comparer.Compare(e1.Current, e2.Current) <= 0)
            {
                yield return e1.Current;
                has1 = e1.MoveNext();
            }
            else
            {
                yield return e2.Current;
                has2 = e2.MoveNext();
            }
        }

        while (has1)
        {
            yield return e1.Current;
            has1 = e1.MoveNext();
        }

        while (has2)
        {
            yield return e2.Current;
            has2 = e2.MoveNext();
        }
    }

    private static IEnumerable<TAccumulate> ScanIterator<T, TAccumulate>(IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> accumulator)
    {
        TAccumulate current = seed;
        yield return current;

        foreach (T element in source)
        {
            current = accumulator(current, element);
            yield return current;
        }
    }

    private static IEnumerable<(TFirst? First, TSecond? Second)> ZipLongestIterator<TFirst, TSecond>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, TFirst? defaultFirst, TSecond? defaultSecond)
    {
        using IEnumerator<TFirst> e1 = first.GetEnumerator();
        using IEnumerator<TSecond> e2 = second.GetEnumerator();

        bool has1 = e1.MoveNext();
        bool has2 = e2.MoveNext();

        while (has1 || has2)
        {
            TFirst? v1 = has1 ? e1.Current : defaultFirst;
            TSecond? v2 = has2 ? e2.Current : defaultSecond;
            yield return (v1, v2);

            if (has1)
            {
                has1 = e1.MoveNext();
            }

            if (has2)
            {
                has2 = e2.MoveNext();
            }
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Appends one or more elements to the end of a sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="items">The items to append.</param>
    ///<returns>A sequence with the original elements followed by <paramref name="items"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<T> AppendMany<T>(this IEnumerable<T> source, params T[] items)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.Concat(items);
    }

    ///<summary>
    ///Appends additional elements to the group with the matching key. If no matching group exists, a new group is
    ///created.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="source">The existing grouping sequence.</param>
    ///<param name="key">The key of the group to extend.</param>
    ///<param name="elements">The elements to append.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>A grouping sequence with the specified group extended.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="elements"/> is <c>null</c>.</exception>
    public static IEnumerable<IGrouping<TKey, TElement>> AppendToGroup<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> source, TKey key, IEnumerable<TElement> elements, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(elements, nameof(elements));
        IGrouping<TKey, TElement> extra = SequenceFactory.Grouping(key, elements);
        return MergeGroupings(source, [extra], comparer);
    }

    ///<summary>
    ///Produces the Cartesian product of two sequences, applying <paramref name="resultSelector"/> to every pair.
    ///</summary>
    ///<typeparam name="TFirst">The element type of the first sequence.</typeparam>
    ///<typeparam name="TSecond">The element type of the second sequence.</typeparam>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="source">The first sequence.</param>
    ///<param name="second">The second sequence.</param>
    ///<param name="resultSelector">A function applied to each pair.</param>
    ///<returns>A sequence of results from every combination.</returns>
    ///<exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
    public static IEnumerable<TResult> CartesianProduct<TFirst, TSecond, TResult>(this IEnumerable<TFirst> source, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        ArgumentNullException.ThrowIfNull(resultSelector, nameof(resultSelector));
        return source.SelectMany(_ => second, resultSelector);
    }

    ///<summary>
    ///Appends all elements from <paramref name="second"/> to the end of <paramref name="source"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The first sequence.</param>
    ///<param name="second">The sequence to append.</param>
    ///<returns>A sequence containing all elements of <paramref name="source"/> followed by <paramref name="second"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IEnumerable<T> ConcatWith<T>(this IEnumerable<T> source, IEnumerable<T> second)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return source.Concat(second);
    }

    ///<summary>
    ///Interleaves elements from <paramref name="source"/> and <paramref name="second"/>, alternating one element at a
    ///time. Remaining elements from the longer sequence are appended at the end.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The first sequence.</param>
    ///<param name="second">The second sequence.</param>
    ///<returns>An interleaved sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IEnumerable<T> Interleave<T>(this IEnumerable<T> source, IEnumerable<T> second)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return InterleaveIterator(source, second);
    }

    ///<summary>
    ///Interleaves elements from multiple sequences in round-robin order. Remaining elements from longer sequences are
    ///appended at the end.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The first sequence.</param>
    ///<param name="others">Additional sequences to interleave.</param>
    ///<returns>An interleaved sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="others"/> is <c>null</c>.</exception>
    public static IEnumerable<T> InterleaveMany<T>(this IEnumerable<T> source, params IEnumerable<T>[] others)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(others, nameof(others));
        return InterleaveManyIterator(source, others);
    }

    ///<summary>
    ///Merges two grouping sequences by key. Groups with the same key are combined into a single group. Keys are
    ///compared using the specified or default equality comparer.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="source">The first grouping sequence.</param>
    ///<param name="second">The second grouping sequence.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>A merged sequence of groupings.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IEnumerable<IGrouping<TKey, TElement>> MergeGroupings<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> source, IEnumerable<IGrouping<TKey, TElement>> second, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return source.Concat(second).GroupBy(static g => g.Key, comparer).Select(static outer => SequenceFactory.Grouping(outer.Key, outer.SelectMany(static g => g)));
    }

    ///<summary>
    ///Merges two ordered sequences into a single ordered sequence using the default comparer. Both inputs must already
    ///be sorted in ascending order.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The first ordered sequence.</param>
    ///<param name="second">The second ordered sequence.</param>
    ///<param name="comparer">An optional comparer; defaults to the default comparer for <typeparamref name="T"/>.</param>
    ///<returns>A merged ordered sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IEnumerable<T> MergeOrdered<T>(this IEnumerable<T> source, IEnumerable<T> second, IComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return MergeOrderedIterator(source, second, comparer ?? Comparer<T>.Default);
    }

    ///<summary>
    ///Merges two <see cref="IOrderedEnumerable{TElement}"/> sequences into a single ordered sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The first ordered sequence.</param>
    ///<param name="second">The second ordered sequence.</param>
    ///<param name="comparer">An optional comparer; defaults to the default comparer for <typeparamref name="T"/>.</param>
    ///<returns>A merged ordered sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IEnumerable<T> MergeOrdered<T>(this IOrderedEnumerable<T> source, IOrderedEnumerable<T> second, IComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return MergeOrderedIterator(source, second, comparer ?? Comparer<T>.Default);
    }

    ///<summary>
    ///Prepends one or more elements before a sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="items">The items to prepend.</param>
    ///<returns>A sequence with <paramref name="items"/> followed by the original elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<T> PrependMany<T>(this IEnumerable<T> source, params T[] items)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return items.Concat(source);
    }

    ///<summary>
    ///Produces a sequence of running aggregates by applying <paramref name="accumulator"/> to each element in turn. The
    ///first element of the result is <paramref name="seed"/>.
    ///</summary>
    ///<typeparam name="T">The element type of the source.</typeparam>
    ///<typeparam name="TAccumulate">The accumulator type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="seed">The initial accumulator value.</param>
    ///<param name="accumulator">A function that combines the current accumulator with the next element.</param>
    ///<returns>A sequence of intermediate accumulator values.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="accumulator"/> is <c>null</c>.</exception>
    public static IEnumerable<TAccumulate> Scan<T, TAccumulate>(this IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> accumulator)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(accumulator, nameof(accumulator));
        return ScanIterator(source, seed, accumulator);
    }

    ///<summary>
    ///Zips two sequences into tuples, padding the shorter sequence with its type's default value so that no elements
    ///are lost.
    ///</summary>
    ///<typeparam name="TFirst">The element type of the first sequence.</typeparam>
    ///<typeparam name="TSecond">The element type of the second sequence.</typeparam>
    ///<param name="source">The first sequence.</param>
    ///<param name="second">The second sequence.</param>
    ///<param name="defaultFirst">The default value for the first sequence when it is shorter.</param>
    ///<param name="defaultSecond">The default value for the second sequence when it is shorter.</param>
    ///<returns>A sequence of tuples with the length of the longer input.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IEnumerable<(TFirst? First, TSecond? Second)> ZipLongest<TFirst, TSecond>(this IEnumerable<TFirst> source, IEnumerable<TSecond> second, TFirst? defaultFirst = default, TSecond? defaultSecond = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return ZipLongestIterator(source, second, defaultFirst, defaultSecond);
    }

    ///<summary>
    ///Zips two sequences into a sequence of tuples. The resulting sequence has the length of the shorter input.
    ///Remaining elements from the longer sequence are discarded.
    ///</summary>
    ///<typeparam name="TFirst">The element type of the first sequence.</typeparam>
    ///<typeparam name="TSecond">The element type of the second sequence.</typeparam>
    ///<param name="source">The first sequence.</param>
    ///<param name="second">The second sequence.</param>
    ///<returns>A sequence of tuples.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IEnumerable<(TFirst First, TSecond Second)> ZipWith<TFirst, TSecond>(this IEnumerable<TFirst> source, IEnumerable<TSecond> second)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return source.Zip(second);
    }

    ///<summary>
    ///Zips two sequences, applying a result selector to corresponding element pairs. The resulting sequence has the
    ///length of the shorter input.
    ///</summary>
    ///<typeparam name="TFirst">The element type of the first sequence.</typeparam>
    ///<typeparam name="TSecond">The element type of the second sequence.</typeparam>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="source">The first sequence.</param>
    ///<param name="second">The second sequence.</param>
    ///<param name="resultSelector">A function that combines corresponding elements.</param>
    ///<returns>A sequence of combined results.</returns>
    ///<exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
    public static IEnumerable<TResult> ZipWith<TFirst, TSecond, TResult>(this IEnumerable<TFirst> source, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        ArgumentNullException.ThrowIfNull(resultSelector, nameof(resultSelector));
        return source.Zip(second, resultSelector);
    }
    #endregion
}
