namespace Catharsis.Linq;

///<summary>
///Provides extension methods for generating <see cref="ILookup{TKey,TElement}"/> and ///<see
///cref="IGrouping{TKey,TElement}"/> structures from sliding windows, key-based windows, and progressive accumulation
///over <see cref="IEnumerable{T}"/> sequences.
///</summary>
public static class SlidingLookup
{
    #region Private methods
    private static IEnumerable<(TKey Key, T Element)> KeyedSlidingPairs<T, TKey>(IEnumerable<T> source, int windowSize, Func<T, TKey> keySelector)
    {
        List<T> buffer = [.. source];

        for (int w = 0; w <= buffer.Count - windowSize; w++)
        {
            TKey key = keySelector(buffer[w]);

            for (int i = 0; i < windowSize; i++)
            {
                yield return (key, buffer[w + i]);
            }
        }
    }

    private static IEnumerable<(int StepIndex, T Element)> ProgressivePairs<T>(IEnumerable<T> source)
    {
        List<T> accumulated = [];
        int step = 0;

        foreach (T item in source)
        {
            accumulated.Add(item);

            foreach (T element in accumulated)
            {
                yield return (step, element);
            }

            step++;
        }
    }

    private static IEnumerable<(int WindowIndex, T Element)> SlidingWindowPairs<T>(IEnumerable<T> source, int windowSize)
    {
        List<T> buffer = [.. source];

        for (int w = 0; w <= buffer.Count - windowSize; w++)
        {
            for (int i = 0; i < windowSize; i++)
            {
                yield return (w, buffer[w + i]);
            }
        }
    }

    private static IEnumerable<IGrouping<int, T>> ToProgressiveGroupingsIterator<T>(IEnumerable<T> source)
    {
        List<T> accumulated = [];
        int step = 0;

        foreach (T item in source)
        {
            accumulated.Add(item);
            yield return SequenceFactory.Grouping(step, (IEnumerable<T>)[.. accumulated]);
            step++;
        }
    }

    private static IEnumerable<IGrouping<TKey, T>> ToSlidingGroupingsByIterator<T, TKey>(IEnumerable<T> source, int windowSize, Func<T, TKey> keySelector)
    {
        List<T> buffer = [.. source];

        for (int w = 0; w <= buffer.Count - windowSize; w++)
        {
            TKey key = keySelector(buffer[w]);
            IEnumerable<T> window = buffer.GetRange(w, windowSize);
            yield return SequenceFactory.Grouping(key, window);
        }
    }

    private static IEnumerable<IGrouping<int, T>> ToSlidingGroupingsIterator<T>(IEnumerable<T> source, int windowSize)
    {
        List<T> buffer = [.. source];

        for (int w = 0; w <= buffer.Count - windowSize; w++)
        {
            IEnumerable<T> window = buffer.GetRange(w, windowSize);
            yield return SequenceFactory.Grouping(w, window);
        }
    }

    private static IEnumerable<IGrouping<int, T>> ToTumblingGroupingsIterator<T>(IEnumerable<T> source, int windowSize)
    {
        List<T> buffer = new(windowSize);
        int windowIndex = 0;

        foreach (T item in source)
        {
            buffer.Add(item);

            if (buffer.Count == windowSize)
            {
                yield return SequenceFactory.Grouping(windowIndex, (IEnumerable<T>)buffer);
                buffer = new(windowSize);
                windowIndex++;
            }
        }

        if (buffer.Count > 0)
        {
            yield return SequenceFactory.Grouping(windowIndex, (IEnumerable<T>)buffer);
        }
    }

    private static IEnumerable<(int WindowIndex, T Element)> TumblingPairs<T>(IEnumerable<T> source, int windowSize)
    {
        int windowIndex = 0;
        int count = 0;

        foreach (T item in source)
        {
            yield return (windowIndex, item);
            count++;

            if (count == windowSize)
            {
                windowIndex++;
                count = 0;
            }
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Produces groupings representing progressive accumulation snapshots.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<returns>A sequence of <see cref="IGrouping{TKey,TElement}"/> keyed by step index.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<IGrouping<int, T>> ToProgressiveGroupings<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return ToProgressiveGroupingsIterator(source);
    }

    ///<summary>
    ///Produces a lookup where each key is the zero-based step index and each group contains all elements up to and
    ///including that step. Useful for building running/cumulative snapshots.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/> keyed by step index.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static ILookup<int, T> ToProgressiveLookup<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return ProgressivePairs(source).ToLookup(static p => p.StepIndex, static p => p.Element);
    }

    ///<summary>
    ///Produces groupings for sliding windows of <paramref name="windowSize"/> elements.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="windowSize">The size of each window. Must be at least 1.</param>
    ///<returns>A sequence of <see cref="IGrouping{TKey,TElement}"/> keyed by window index.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="windowSize"/> is less than 1.</exception>
    public static IEnumerable<IGrouping<int, T>> ToSlidingGroupings<T>(this IEnumerable<T> source, int windowSize)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1, nameof(windowSize));
        return ToSlidingGroupingsIterator(source, windowSize);
    }

    ///<summary>
    ///Produces groupings of sliding windows keyed by the first element's projected key.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="windowSize">The size of each window. Must be at least 1.</param>
    ///<param name="keySelector">A function that extracts a key from the first element of each window.</param>
    ///<returns>A sequence of <see cref="IGrouping{TKey,TElement}"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="windowSize"/> is less than 1.</exception>
    public static IEnumerable<IGrouping<TKey, T>> ToSlidingGroupingsBy<T, TKey>(this IEnumerable<T> source, int windowSize, Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1, nameof(windowSize));
        return ToSlidingGroupingsByIterator(source, windowSize, keySelector);
    }

    ///<summary>
    ///Produces a lookup where each key is the zero-based window index and each group contains the elements of one
    ///sliding window of <paramref name="windowSize"/> elements advancing by one position at a time.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="windowSize">The size of each window. Must be at least 1.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/> keyed by window index.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="windowSize"/> is less than 1.</exception>
    public static ILookup<int, T> ToSlidingLookup<T>(this IEnumerable<T> source, int windowSize)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1, nameof(windowSize));
        return SlidingWindowPairs(source, windowSize).ToLookup(static p => p.WindowIndex, static p => p.Element);
    }

    ///<summary>
    ///Produces a lookup of sliding windows where each window is keyed by the value extracted from the first element of
    ///that window via <paramref name="keySelector"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="windowSize">The size of each window. Must be at least 1.</param>
    ///<param name="keySelector">A function that extracts a key from the first element of each window.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/> keyed by the first element's key value.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="windowSize"/> is less than 1.</exception>
    public static ILookup<TKey, T> ToSlidingLookupBy<T, TKey>(this IEnumerable<T> source, int windowSize, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1, nameof(windowSize));
        return KeyedSlidingPairs(source, windowSize, keySelector).ToLookup(static p => p.Key, static p => p.Element, comparer);
    }

    ///<summary>
    ///Produces groupings of non-overlapping tumbling windows keyed by window index.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="windowSize">The size of each window. Must be at least 1.</param>
    ///<returns>A sequence of <see cref="IGrouping{TKey,TElement}"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="windowSize"/> is less than 1.</exception>
    public static IEnumerable<IGrouping<int, T>> ToTumblingGroupings<T>(this IEnumerable<T> source, int windowSize)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1, nameof(windowSize));
        return ToTumblingGroupingsIterator(source, windowSize);
    }

    ///<summary>
    ///Produces a lookup of non-overlapping tumbling windows keyed by zero-based window index.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="windowSize">The size of each window. Must be at least 1.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/> keyed by window index.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="windowSize"/> is less than 1.</exception>
    public static ILookup<int, T> ToTumblingLookup<T>(this IEnumerable<T> source, int windowSize)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1, nameof(windowSize));
        return TumblingPairs(source, windowSize).ToLookup(static p => p.WindowIndex, static p => p.Element);
    }
    #endregion
}
