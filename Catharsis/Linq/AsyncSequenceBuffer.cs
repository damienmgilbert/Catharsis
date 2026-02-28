using System.Runtime.CompilerServices;

namespace Catharsis.Linq;

///<summary>
///Provides extension methods for buffering, batching, and windowing <see cref="IAsyncEnumerable{T}"/> sequences. All
///methods support <see cref="CancellationToken"/> for cooperative cancellation.
///</summary>
public static class AsyncSequenceBuffer
{
    #region Private methods
    private static async IAsyncEnumerable<(TKey Key, IReadOnlyList<T> Elements)> BufferByKeyIterator<T, TKey>(IAsyncEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using IAsyncEnumerator<T> enumerator = source.GetAsyncEnumerator(cancellationToken);

        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            yield break;
        }

        TKey currentKey = keySelector(enumerator.Current);
        List<T> currentBucket = [enumerator.Current];

        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            TKey key = keySelector(enumerator.Current);

            if (comparer.Equals(key, currentKey))
            {
                currentBucket.Add(enumerator.Current);
            }
            else
            {
                yield return (currentKey, currentBucket.AsReadOnly());
                currentKey = key;
                currentBucket = [enumerator.Current];
            }
        }

        yield return (currentKey, currentBucket.AsReadOnly());
    }

    private static async IAsyncEnumerable<IReadOnlyList<T>> BufferIterator<T>(IAsyncEnumerable<T> source, int size, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        List<T> buffer = new(size);

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            buffer.Add(item);

            if (buffer.Count == size)
            {
                yield return buffer.AsReadOnly();
                buffer = new List<T>(size);
            }
        }

        if (buffer.Count > 0)
        {
            yield return buffer.AsReadOnly();
        }
    }

    private static async IAsyncEnumerable<IReadOnlyList<T>> BufferSkipIterator<T>(IAsyncEnumerable<T> source, int size, int skip, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        List<T> allItems = [];

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            allItems.Add(item);
        }

        for (int i = 0; i <= allItems.Count - size; i += skip)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return allItems.GetRange(i, size).AsReadOnly();
        }
    }

    private static async IAsyncEnumerable<(T Previous, T Current)> PairwiseIterator<T>(IAsyncEnumerable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using IAsyncEnumerator<T> enumerator = source.GetAsyncEnumerator(cancellationToken);

        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            yield break;
        }

        T previous = enumerator.Current;

        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            yield return (previous, enumerator.Current);
            previous = enumerator.Current;
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Collects elements into fixed-size buffers. The last buffer may contain fewer elements.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="size">The maximum number of elements per buffer. Must be at least 1.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of lists, each containing up to <paramref name="size"/> elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is less than 1.</exception>
    public static IAsyncEnumerable<IReadOnlyList<T>> Buffer<T>(this IAsyncEnumerable<T> source, int size, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(size));
        return BufferIterator(source, size, cancellationToken);
    }

    ///<summary>
    ///Collects elements into buffers of <paramref name="size"/> with a sliding step of <paramref name="skip"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="size">The window size. Must be at least 1.</param>
    ///<param name="skip">How many elements to advance between windows. Must be at least 1.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of read-only lists representing each window.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="size"/> or <paramref name="skip"/> is less than 1.</exception>
    public static IAsyncEnumerable<IReadOnlyList<T>> Buffer<T>(this IAsyncEnumerable<T> source, int size, int skip, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(size));
        ArgumentOutOfRangeException.ThrowIfLessThan(skip, 1, nameof(skip));
        return BufferSkipIterator(source, size, skip, cancellationToken);
    }

    ///<summary>
    ///Groups consecutive elements that share the same key into buffers. A new buffer starts when the key changes.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="keySelector">A function that extracts the key from each element.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of <c>(Key, Elements)</c> tuples.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<(TKey Key, IReadOnlyList<T> Elements)> BufferByKey<T, TKey>(this IAsyncEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return BufferByKeyIterator(source, keySelector, comparer ?? EqualityComparer<TKey>.Default, cancellationToken);
    }

    ///<summary>
    ///Produces overlapping pairs of consecutive elements: (e0, e1), (e1, e2), etc.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of tuple pairs.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<(T Previous, T Current)> Pairwise<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return PairwiseIterator(source, cancellationToken);
    }

    ///<summary>
    ///Materializes an async sequence into an array.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A task that resolves to an array of all elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static async ValueTask<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        List<T> list = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
        return [.. list];
    }

    ///<summary>
    ///Materializes an async sequence into a <see cref="List{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A task that resolves to a list of all elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static async ValueTask<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        List<T> result = [];

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            result.Add(item);
        }

        return result;
    }
    #endregion
}
