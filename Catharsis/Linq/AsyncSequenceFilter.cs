using System.Runtime.CompilerServices;

namespace Catharsis.Linq;

///<summary>
///Provides extension methods for filtering <see cref="IAsyncEnumerable{T}"/> sequences. Includes synchronous and
///asynchronous predicate filtering, distinct-by-key, take/skip-while, and throttling operators.
///</summary>
public static class AsyncSequenceFilter
{
    #region Private methods
    private static async IAsyncEnumerable<T> DistinctByIterator<T, TKey>(IAsyncEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        HashSet<TKey> seen = [with(comparer)];

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (seen.Add(keySelector(item)))
            {
                yield return item;
            }
        }
    }

    private static async IAsyncEnumerable<T> SkipIterator<T>(IAsyncEnumerable<T> source, int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int skipped = 0;

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (skipped < count)
            {
                skipped++;
                continue;
            }

            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> SkipWhileIterator<T>(IAsyncEnumerable<T> source, Func<T, bool> predicate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        bool skipping = true;

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (skipping && predicate(item))
            {
                continue;
            }

            skipping = false;
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> TakeIterator<T>(IAsyncEnumerable<T> source, int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int taken = 0;

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (taken >= count)
            {
                yield break;
            }

            yield return item;
            taken++;
        }
    }

    private static async IAsyncEnumerable<T> TakeWhileIterator<T>(IAsyncEnumerable<T> source, Func<T, bool> predicate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (!predicate(item))
            {
                yield break;
            }

            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> ThrottleIterator<T>(IAsyncEnumerable<T> source, TimeSpan delay, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        bool first = true;

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (!first)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            first = false;
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> WhereAsyncIterator<T>(IAsyncEnumerable<T> source, Func<T, CancellationToken, ValueTask<bool>> predicate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (await predicate(item, cancellationToken).ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }

    private static async IAsyncEnumerable<T> WhereIndexedIterator<T>(IAsyncEnumerable<T> source, Func<T, int, bool> predicate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int index = 0;

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item, index))
            {
                yield return item;
            }

            index++;
        }
    }

    private static async IAsyncEnumerable<T> WhereIterator<T>(IAsyncEnumerable<T> source, Func<T, bool> predicate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Returns distinct elements from an async sequence based on a projected key. The first element with each unique key
    ///is retained.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="keySelector">A function that extracts the comparison key.</param>
    ///<param name="comparer">An optional equality comparer for keys.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of distinct elements by key.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> DistinctBy<T, TKey>(this IAsyncEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return DistinctByIterator(source, keySelector, comparer ?? EqualityComparer<TKey>.Default, cancellationToken);
    }

    ///<summary>
    ///Skips the first <paramref name="count"/> elements from an async sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="count">The number of elements to skip.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence with the first <paramref name="count"/> elements removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static IAsyncEnumerable<T> Skip<T>(this IAsyncEnumerable<T> source, int count, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        return SkipIterator(source, count, cancellationToken);
    }

    ///<summary>
    ///Skips elements from an async sequence while <paramref name="predicate"/> returns <c>true</c>, then yields the
    ///remainder.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="predicate">A function that tests each element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence with leading matching elements removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> SkipWhile<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return SkipWhileIterator(source, predicate, cancellationToken);
    }

    ///<summary>
    ///Takes the first <paramref name="count"/> elements from an async sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="count">The maximum number of elements to take.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of at most <paramref name="count"/> elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static IAsyncEnumerable<T> Take<T>(this IAsyncEnumerable<T> source, int count, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        return TakeIterator(source, count, cancellationToken);
    }

    ///<summary>
    ///Yields elements from an async sequence while <paramref name="predicate"/> returns <c>true</c>, then stops.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="predicate">A function that tests each element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of leading elements that satisfy the predicate.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> TakeWhile<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return TakeWhileIterator(source, predicate, cancellationToken);
    }

    ///<summary>
    ///Throttles an async sequence by introducing a minimum <paramref name="delay"/> between yielded elements.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="delay">The minimum time between consecutive elements.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence with a delay between elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> is negative.</exception>
    public static IAsyncEnumerable<T> Throttle<T>(this IAsyncEnumerable<T> source, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(delay, TimeSpan.Zero, nameof(delay));
        return ThrottleIterator(source, delay, cancellationToken);
    }

    ///<summary>
    ///Filters an async sequence using a synchronous predicate.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="predicate">A function that tests each element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of elements that satisfy the predicate.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return WhereIterator(source, predicate, cancellationToken);
    }

    ///<summary>
    ///Filters an async sequence using a synchronous indexed predicate.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="predicate">A function that receives (element, index) and returns whether to include the element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of elements that satisfy the predicate.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> source, Func<T, int, bool> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return WhereIndexedIterator(source, predicate, cancellationToken);
    }

    ///<summary>
    ///Filters an async sequence using an asynchronous predicate.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="predicate">An async function that tests each element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of elements that satisfy the predicate.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> WhereAsync<T>(this IAsyncEnumerable<T> source, Func<T, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return WhereAsyncIterator(source, predicate, cancellationToken);
    }
    #endregion
}
