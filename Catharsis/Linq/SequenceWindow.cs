namespace Catharsis.Linq;

///<summary>
///Provides extension methods for producing sliding, tumbling, and buffered windows over <see cref="IEnumerable{T}"/>,
public static class SequenceWindow
{
    #region Private methods
    private static IEnumerable<IGrouping<TKey, T>> BufferByIterator<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();

        if(!enumerator.MoveNext())
        {
            yield break;
        }

        EqualityComparer<TKey> comparer = EqualityComparer<TKey>.Default;
        TKey currentKey = keySelector(enumerator.Current);
        List<T> currentBucket = [ enumerator.Current ];

        while(enumerator.MoveNext())
        {
            TKey key = keySelector(enumerator.Current);

            if(comparer.Equals(key, currentKey))
            {
                currentBucket.Add(enumerator.Current);
            } else
            {
                yield return SequenceFactory.Grouping(currentKey, (IEnumerable<T>)currentBucket);
                currentKey = key;
                currentBucket = [ enumerator.Current ];
            }
        }

        yield return SequenceFactory.Grouping(currentKey, (IEnumerable<T>)currentBucket);
    }

    private static IEnumerable<List<T>> BufferIterator<T>(IEnumerable<T> source, int size)
    {
        List<T> buffer = [ with(size) ];

        foreach(T item in source)
        {
            buffer.Add(item);

            if(buffer.Count == size)
            {
                yield return buffer;
                buffer = [ with(size) ];
            }
        }

        if(buffer.Count > 0)
        {
            yield return buffer;
        }
    }

    private static IEnumerable<(T Previous, T Current)> PairwiseIterator<T>(IEnumerable<T> source)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();

        if(!enumerator.MoveNext())
        {
            yield break;
        }

        T previous = enumerator.Current;

        while(enumerator.MoveNext())
        {
            yield return (previous, enumerator.Current);
            previous = enumerator.Current;
        }
    }

    private static IEnumerable<TResult> PairwiseIterator<T, TResult>(IEnumerable<T> source, Func<T, T, TResult> resultSelector)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();

        if(!enumerator.MoveNext())
        {
            yield break;
        }

        T previous = enumerator.Current;

        while(enumerator.MoveNext())
        {
            yield return resultSelector(previous, enumerator.Current);
            previous = enumerator.Current;
        }
    }

    private static IEnumerable<IReadOnlyList<T>> SlidingIterator<T>(IEnumerable<T> source, int size, int step)
    {
        List<T> buffer = [ with(size) ];
        int skip = 0;

        foreach(T item in source)
        {
            if(skip > 0)
            {
                skip--;

                if(buffer.Count > 0)
                {
                    int remove = Math.Min(1, buffer.Count);
                    buffer.RemoveRange(0, remove);
                }

                buffer.Add(item);
                continue;
            }

            buffer.Add(item);

            if(buffer.Count == size)
            {
                yield return buffer.ToList().AsReadOnly();
                skip = step - 1;

                if(step < size)
                {
                    buffer.RemoveRange(0, step);
                } else
                {
                    buffer.Clear();
                }
            }
        }
    }

    private static IEnumerable<IGrouping<TKey, IReadOnlyList<TElement>>> SlidingPerGroupIterator<TKey, TElement>(IEnumerable<IGrouping<TKey, TElement>> source, int size)
    {
        foreach(IGrouping<TKey, TElement> group in source)
        {
            IEnumerable<IReadOnlyList<TElement>> windows = SlidingIterator(group, size, step: 1);
            yield return SequenceFactory.Grouping(group.Key, windows);
        }
    }

    private static IEnumerable<IReadOnlyList<T>> TumblingIterator<T>(IEnumerable<T> source, int size)
    {
        List<T> buffer = [ with(size) ];

        foreach(T item in source)
        {
            buffer.Add(item);

            if(buffer.Count == size)
            {
                yield return buffer.AsReadOnly();
                buffer = [ with(size) ];
            }
        }

        if(buffer.Count > 0)
        {
            yield return buffer.AsReadOnly();
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Buffers elements into groups of up to <paramref name="size"/>. Unlike <see cref="Tumbling{T}(IEnumerable{T},
    ///int)"/>, this method materializes each buffer as a <see cref="List{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="size">The maximum number of elements per buffer. Must be at least 1.</param>
    ///<returns>A sequence of buffers.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is less than 1.</exception>
    public static IEnumerable<List<T>> Buffer<T>(this IEnumerable<T> source, int size)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(size));
        return BufferIterator(source, size);
    }

    ///<summary>
    ///Buffers elements into groups separated by a time span. Elements are collected from the source and grouped by the
    ///specified <paramref name="keySelector"/> time bucket.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The bucket key type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="keySelector">A function that maps each element to a bucket key.</param>
    ///<returns>A sequence of groupings representing each buffer.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IEnumerable<IGrouping<TKey, T>> BufferBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return BufferByIterator(source, keySelector);
    }

    ///<summary>
    ///Produces a sequence of overlapping pairs from consecutive elements: (e0, e1), (e1, e2), (e2, e3), etc.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<returns>A sequence of tuples pairing consecutive elements. Empty if the source has fewer than 2 elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<(T Previous, T Current)> Pairwise<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return PairwiseIterator(source);
    }

    ///<summary>
    ///Produces a sequence by applying <paramref name="resultSelector"/> to each pair of consecutive elements.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="resultSelector">A function that combines two consecutive elements.</param>
    ///<returns>A sequence of projected pair results.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="resultSelector"/> is <c>null</c>.</exception>
    public static IEnumerable<TResult> Pairwise<T, TResult>(this IEnumerable<T> source, Func<T, T, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(resultSelector, nameof(resultSelector));
        return PairwiseIterator(source, resultSelector);
    }

    ///<summary>
    ///Produces a sliding window of <paramref name="size"/> elements that advances by one element at a time. Each window
    ///is returned as a read-only list.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="size">The window size. Must be at least 1.</param>
    ///<returns>A sequence of windows, each containing <paramref name="size"/> elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is less than 1.</exception>
    public static IEnumerable<IReadOnlyList<T>> Sliding<T>(this IEnumerable<T> source, int size)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(size));
        return SlidingIterator(source, size, step: 1);
    }

    ///<summary>
    ///Produces a sliding window over an <see cref="IOrderedEnumerable{TElement}"/> preserving the existing order.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The ordered source sequence.</param>
    ///<param name="size">The window size. Must be at least 1.</param>
    ///<returns>A sequence of windows.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is less than 1.</exception>
    public static IEnumerable<IReadOnlyList<T>> Sliding<T>(this IOrderedEnumerable<T> source, int size)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(size));
        return SlidingIterator(source, size, step: 1);
    }

    ///<summary>
    ///Produces a sliding window of <paramref name="size"/> elements that advances by <paramref name="step"/> elements
    ///at a time.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="size">The window size. Must be at least 1.</param>
    ///<param name="step">The number of elements to advance between windows. Must be at least 1.</param>
    ///<returns>A sequence of windows.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="size"/> or <paramref name="step"/> is less than 1.</exception>
    public static IEnumerable<IReadOnlyList<T>> Sliding<T>(this IEnumerable<T> source, int size, int step)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(size));
        ArgumentOutOfRangeException.ThrowIfLessThan(step, 1, nameof(step));
        return SlidingIterator(source, size, step);
    }

    ///<summary>
    ///Applies a sliding window to the elements within each group of a grouped sequence.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="source">The grouped sequence.</param>
    ///<param name="size">The window size. Must be at least 1.</param>
    ///<returns>A sequence of groupings, each containing windows of the original group's elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is less than 1.</exception>
    public static IEnumerable<IGrouping<TKey, IReadOnlyList<TElement>>> SlidingPerGroup<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> source, int size)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(size));
        return SlidingPerGroupIterator(source, size);
    }

    ///<summary>
    ///Produces non-overlapping tumbling windows of exactly <paramref name="size"/> elements. The final window may
    ///contain fewer elements if the source length is not evenly divisible.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="size">The window size. Must be at least 1.</param>
    ///<returns>A sequence of non-overlapping windows.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is less than 1.</exception>
    public static IEnumerable<IReadOnlyList<T>> Tumbling<T>(this IEnumerable<T> source, int size)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(size));
        return TumblingIterator(source, size);
    }

    ///<summary>
    ///Produces non-overlapping tumbling windows over an <see cref="IOrderedEnumerable{TElement}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The ordered source sequence.</param>
    ///<param name="size">The window size. Must be at least 1.</param>
    ///<returns>A sequence of non-overlapping windows.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is less than 1.</exception>
    public static IEnumerable<IReadOnlyList<T>> Tumbling<T>(this IOrderedEnumerable<T> source, int size)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(size));
        return TumblingIterator(source, size);
    }
    #endregion
}
