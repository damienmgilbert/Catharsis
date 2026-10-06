namespace Catharsis.Linq;

///<summary>
///Provides extension methods that chunk a sequence and then apply a mapping or aggregation function to each chunk,
///supporting both flat and grouped result shapes over <see cref="IEnumerable{T}"/>.
///</summary>
public static class SequenceChunkMap
{
    #region Private methods
    private static IEnumerable<TResult> ChunkAggregateIterator<T, TResult>(IEnumerable<T> source, int chunkSize, Func<IReadOnlyList<T>, TResult> mapper)
    {
        List<T> chunk = [ with(chunkSize) ];

        foreach(T item in source)
        {
            chunk.Add(item);

            if(chunk.Count == chunkSize)
            {
                yield return mapper(chunk.AsReadOnly());
                chunk = [ with(chunkSize) ];
            }
        }

        if(chunk.Count > 0)
        {
            yield return mapper(chunk.AsReadOnly());
        }
    }

    private static IEnumerable<TResult> ChunkMapByKeyIterator<T, TKey, TResult>(IEnumerable<T> source, Func<T, TKey> keySelector, Func<TKey, IReadOnlyList<T>, IEnumerable<TResult>> mapper, IEqualityComparer<TKey> comparer)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();

        if(!enumerator.MoveNext())
        {
            yield break;
        }

        TKey currentKey = keySelector(enumerator.Current);
        List<T> currentChunk = [ enumerator.Current ];

        while(enumerator.MoveNext())
        {
            TKey key = keySelector(enumerator.Current);

            if(comparer.Equals(key, currentKey))
            {
                currentChunk.Add(enumerator.Current);
            } else
            {
                foreach(TResult result in mapper(currentKey, currentChunk.AsReadOnly()))
                {
                    yield return result;
                }

                currentKey = key;
                currentChunk = [ enumerator.Current ];
            }
        }

        foreach(TResult result in mapper(currentKey, currentChunk.AsReadOnly()))
        {
            yield return result;
        }
    }

    private static IEnumerable<TResult> ChunkMapGroupsIterator<TKey, TElement, TResult>(IEnumerable<IGrouping<TKey, TElement>> source, int chunkSize, Func<TKey, IReadOnlyList<TElement>, IEnumerable<TResult>> mapper)
    {
        foreach(IGrouping<TKey, TElement> group in source)
        {
            List<TElement> chunk = [ with(chunkSize) ];

            foreach(TElement element in group)
            {
                chunk.Add(element);

                if(chunk.Count == chunkSize)
                {
                    foreach(TResult result in mapper(group.Key, chunk.AsReadOnly()))
                    {
                        yield return result;
                    }

                    chunk = [ with(chunkSize) ];
                }
            }

            if(chunk.Count > 0)
            {
                foreach(TResult result in mapper(group.Key, chunk.AsReadOnly()))
                {
                    yield return result;
                }
            }
        }
    }

    private static IEnumerable<TResult> ChunkMapIndexedIterator<T, TResult>(IEnumerable<T> source, int chunkSize, Func<int, IReadOnlyList<T>, IEnumerable<TResult>> mapper)
    {
        List<T> chunk = [ with(chunkSize) ];
        int chunkIndex = 0;

        foreach(T item in source)
        {
            chunk.Add(item);

            if(chunk.Count == chunkSize)
            {
                foreach(TResult result in mapper(chunkIndex, chunk.AsReadOnly()))
                {
                    yield return result;
                }

                chunk = [ with(chunkSize) ];
                chunkIndex++;
            }
        }

        if(chunk.Count > 0)
        {
            foreach(TResult result in mapper(chunkIndex, chunk.AsReadOnly()))
            {
                yield return result;
            }
        }
    }

    private static IEnumerable<TResult> ChunkMapIterator<T, TResult>(IEnumerable<T> source, int chunkSize, Func<IReadOnlyList<T>, IEnumerable<TResult>> mapper)
    {
        List<T> chunk = [ with(chunkSize) ];

        foreach(T item in source)
        {
            chunk.Add(item);

            if(chunk.Count == chunkSize)
            {
                foreach(TResult result in mapper(chunk.AsReadOnly()))
                {
                    yield return result;
                }

                chunk = [ with(chunkSize) ];
            }
        }

        if(chunk.Count > 0)
        {
            foreach(TResult result in mapper(chunk.AsReadOnly()))
            {
                yield return result;
            }
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Divides the source into chunks and projects each chunk into a single result value.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="chunkSize">The maximum number of elements per chunk. Must be at least 1.</param>
    ///<param name="mapper">A function that reduces a chunk to a single result.</param>
    ///<returns>A sequence containing one result per chunk.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="mapper"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="chunkSize"/> is less than 1.</exception>
    public static IEnumerable<TResult> ChunkAggregate<T, TResult>(this IEnumerable<T> source, int chunkSize, Func<IReadOnlyList<T>, TResult> mapper)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1, nameof(chunkSize));
        return ChunkAggregateIterator(source, chunkSize, mapper);
    }

    ///<summary>
    ///Divides the source into chunks of <paramref name="chunkSize"/> elements, applies <paramref name="mapper"/> to
    ///each chunk, and flattens the results into a single sequence.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="chunkSize">The maximum number of elements per chunk. Must be at least 1.</param>
    ///<param name="mapper">A function that transforms a chunk into a sequence of results.</param>
    ///<returns>A flat sequence of all mapped results from every chunk.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="mapper"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="chunkSize"/> is less than 1.</exception>
    public static IEnumerable<TResult> ChunkMap<T, TResult>(this IEnumerable<T> source, int chunkSize, Func<IReadOnlyList<T>, IEnumerable<TResult>> mapper)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1, nameof(chunkSize));
        return ChunkMapIterator(source, chunkSize, mapper);
    }

    ///<summary>
    ///Chunks consecutive elements that share the same key (as determined by <paramref name="keySelector"/>) and applies
    public static IEnumerable<TResult> ChunkMapByKey<T, TKey, TResult>(this IEnumerable<T> source, Func<T, TKey> keySelector, Func<TKey, IReadOnlyList<T>, IEnumerable<TResult>> mapper, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        return ChunkMapByKeyIterator(source, keySelector, mapper, comparer ?? EqualityComparer<TKey>.Default);
    }

    ///<summary>
    ///Applies <paramref name="mapper"/> to chunks of elements within each group of a grouped sequence.
    ///</summary>
    ///<typeparam name="TKey">The group key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The grouped sequence.</param>
    ///<param name="chunkSize">The maximum number of elements per chunk. Must be at least 1.</param>
    ///<param name="mapper">A function that receives the key and a chunk and returns a result sequence.</param>
    ///<returns>A flat sequence of all mapped results across all groups and chunks.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="mapper"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="chunkSize"/> is less than 1.</exception>
    public static IEnumerable<TResult> ChunkMapGroups<TKey, TElement, TResult>(this IEnumerable<IGrouping<TKey, TElement>> source, int chunkSize, Func<TKey, IReadOnlyList<TElement>, IEnumerable<TResult>> mapper)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1, nameof(chunkSize));
        return ChunkMapGroupsIterator(source, chunkSize, mapper);
    }

    ///<summary>
    ///Divides the source into chunks and transforms each chunk with its zero-based chunk index, flattening results.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="chunkSize">The maximum number of elements per chunk. Must be at least 1.</param>
    ///<param name="mapper">A function that receives the chunk index and chunk and returns a result sequence.</param>
    ///<returns>A flat sequence of all mapped results.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="mapper"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="chunkSize"/> is less than 1.</exception>
    public static IEnumerable<TResult> ChunkMapIndexed<T, TResult>(this IEnumerable<T> source, int chunkSize, Func<int, IReadOnlyList<T>, IEnumerable<TResult>> mapper)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1, nameof(chunkSize));
        return ChunkMapIndexedIterator(source, chunkSize, mapper);
    }
    #endregion
}
