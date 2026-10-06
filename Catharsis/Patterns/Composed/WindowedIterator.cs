namespace Catharsis.Patterns.Composed;

///<summary>
///Sliding and tumbling windows over a sequence, produced lazily in a single pass without buffering the whole source.
///</summary>
public static class WindowedIterator
{
    #region Private methods
    private static IEnumerable<(T Previous, T Current)> PairwiseIterator<T>(IEnumerable<T> source)
    {
        bool hasPrevious = false;
        T previous = default!;

        foreach(T item in source)
        {
            if(hasPrevious)
            {
                yield return (previous, item);
            }

            previous = item;
            hasPrevious = true;
        }
    }

    // Window i covers source positions [i * step, i * step + size). The buffer holds only positions not yet dropped, and
    // "consumed" is the source position of buffer[0], so memory stays bounded by roughly size + step elements.
    private static IEnumerable<IReadOnlyList<T>> WindowsIterator<T>(IEnumerable<T> source, int size, int step, bool includePartial)
    {
        List<T> buffer = [];
        long consumed = 0;
        long nextStart = 0;
        long total = 0;

        foreach(T item in source)
        {
            buffer.Add(item);
            total++;

            while(nextStart + size <= total)
            {
                yield return[ .. buffer.GetRange((int)(nextStart - consumed), size) ];

                nextStart += step;

                int drop = (int)Math.Min(nextStart - consumed, buffer.Count);
                buffer.RemoveRange(0, drop);
                consumed += drop;
            }
        }

        if(includePartial)
        {
            while(nextStart < total)
            {
                yield return[ .. buffer.GetRange((int)(nextStart - consumed), (int)(total - nextStart)) ];

                nextStart += step;
            }
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Pairs each element with the one after it, so <c>[1, 2, 3]</c> gives <c>(1, 2)</c> and <c>(2, 3)</c>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The sequence to pair up.</param>
    ///<returns>One pair per adjacent elements; empty when the source has fewer than two elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<(T Previous, T Current)> Pairwise<T>(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return PairwiseIterator(source);
    }

    ///<summary>
    ///Groups a sequence into windows of <paramref name="size"/> elements, moving forward <paramref name="step"/>
    ///elements each time. A step of 1 gives an overlapping sliding window; a step equal to the size gives non-
    ///overlapping chunks.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The sequence to window.</param>
    ///<param name="size">Elements per window. Must be positive.</param>
    ///<param name="step">Elements to advance between windows. Must be positive.</param>
    ///<param name="includePartial">
    ///When <c>true</c>, a final window shorter than <paramref name="size"/> is also returned; otherwise only full
    ///windows are.
    ///</param>
    ///<returns>Each window as a new list, so callers may keep it.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="size"/> or <paramref name="step"/> is not positive.</exception>
    public static IEnumerable<IReadOnlyList<T>> Windows<T>(IEnumerable<T> source, int size, int step = 1, bool includePartial = false)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(step);

        return WindowsIterator(source, size, step, includePartial);
    }
    #endregion
}
