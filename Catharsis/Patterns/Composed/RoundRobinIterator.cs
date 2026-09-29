namespace Catharsis.Patterns.Composed;

///<summary>
///Round-robin iteration: taking one element from each source in turn (<see cref="Interleave{T}"/>) or looping over a
///list forever (<see cref="Cycle{T}"/>). Both are lazy.
///</summary>
public static class RoundRobinIterator
{
    #region Public methods
    ///<summary>
    ///Interleaves several sequences, yielding the first element of each, then the second of each, and so on. A source
    ///that runs out is skipped, so longer sources keep going alone.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="sources">The sequences to interleave.</param>
    ///<returns>The interleaved elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="sources"/> or one of its elements is <c>null</c>.</exception>
    public static IEnumerable<T> Interleave<T>(params IEnumerable<T>[] sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        foreach(IEnumerable<T> source in sources)
        {
            ArgumentNullException.ThrowIfNull(source);
        }

        return InterleaveIterator(sources);
    }

    ///<summary>
    ///Repeats a list endlessly, in order. Combine with <c>Take</c> to bound it.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="items">The items to loop over. Read afresh on each pass.</param>
    ///<returns>An infinite sequence, or an empty one if <paramref name="items"/> is empty.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="items"/> is <c>null</c>.</exception>
    public static IEnumerable<T> Cycle<T>(IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return CycleIterator(items);
    }
    #endregion

    #region Private methods
    static IEnumerable<T> InterleaveIterator<T>(IEnumerable<T>[] sources)
    {
        List<IEnumerator<T>> active = [];

        try
        {
            foreach(IEnumerable<T> source in sources)
            {
                active.Add(source.GetEnumerator());
            }

            while(active.Count > 0)
            {
                for(int i = 0; i < active.Count;)
                {
                    if(active[i].MoveNext())
                    {
                        yield return active[i].Current;
                        i++;
                    }
                    else
                    {
                        active[i].Dispose();
                        active.RemoveAt(i);
                    }
                }
            }
        }
        finally
        {
            foreach(IEnumerator<T> enumerator in active)
            {
                enumerator.Dispose();
            }
        }
    }

    static IEnumerable<T> CycleIterator<T>(IReadOnlyList<T> items)
    {
        while(items.Count > 0)
        {
            for(int i = 0; i < items.Count; i++)
            {
                yield return items[i];
            }
        }
    }
    #endregion
}
