using System.Runtime.CompilerServices;

namespace Catharsis.Linq;

///<summary>
///Provides extension methods for projecting, scanning, choosing, and aggregating ///<see cref="IAsyncEnumerable{T}"/>
///sequences. All methods support <see cref="CancellationToken"/> for cooperative cancellation.
///</summary>
public static class AsyncSequenceProjector
{
    #region Private methods
    private static async IAsyncEnumerable<TResult> ChooseRefIterator<T, TResult>(IAsyncEnumerable<T> source, Func<T, TResult?> chooser, [EnumeratorCancellation] CancellationToken cancellationToken = default) where TResult : class
    {
        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            TResult? result = chooser(item);

            if (result is not null)
            {
                yield return result;
            }
        }
    }

    private static async IAsyncEnumerable<TResult> ChooseValueIterator<T, TResult>(IAsyncEnumerable<T> source, Func<T, TResult?> chooser, [EnumeratorCancellation] CancellationToken cancellationToken = default) where TResult : struct
    {
        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            TResult? result = chooser(item);

            if (result.HasValue)
            {
                yield return result.Value;
            }
        }
    }

    private static async IAsyncEnumerable<TAccumulate> ScanAsyncIterator<T, TAccumulate>(IAsyncEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, CancellationToken, ValueTask<TAccumulate>> accumulator, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        TAccumulate current = seed;
        yield return current;

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            current = await accumulator(current, item, cancellationToken).ConfigureAwait(false);
            yield return current;
        }
    }

    private static async IAsyncEnumerable<TAccumulate> ScanIterator<T, TAccumulate>(IAsyncEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> accumulator, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        TAccumulate current = seed;
        yield return current;

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            current = accumulator(current, item);
            yield return current;
        }
    }

    private static async IAsyncEnumerable<TResult> SelectAsyncIterator<T, TResult>(IAsyncEnumerable<T> source, Func<T, CancellationToken, ValueTask<TResult>> selector, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return await selector(item, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async IAsyncEnumerable<TResult> SelectIndexedIterator<T, TResult>(IAsyncEnumerable<T> source, Func<T, int, TResult> selector, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int index = 0;

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return selector(item, index);
            index++;
        }
    }

    private static async IAsyncEnumerable<TResult> SelectIterator<T, TResult>(IAsyncEnumerable<T> source, Func<T, TResult> selector, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return selector(item);
        }
    }

    private static async IAsyncEnumerable<TResult> SelectManyIterator<T, TResult>(IAsyncEnumerable<T> source, Func<T, IAsyncEnumerable<TResult>> selector, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            await foreach (TResult result in selector(item).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return result;
            }
        }
    }

    private static async IAsyncEnumerable<TResult> SelectManySyncIterator<T, TResult>(IAsyncEnumerable<T> source, Func<T, IEnumerable<TResult>> selector, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            foreach (TResult result in selector(item))
            {
                yield return result;
            }
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Aggregates all elements using a synchronous accumulator.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TAccumulate">The accumulator type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="seed">The initial accumulator value.</param>
    ///<param name="accumulator">A function that combines the accumulator with each element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A task that resolves to the final accumulated value.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="accumulator"/> is <c>null</c>.</exception>
    public static async ValueTask<TAccumulate> AggregateAsync<T, TAccumulate>(this IAsyncEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> accumulator, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(accumulator, nameof(accumulator));

        TAccumulate result = seed;

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            result = accumulator(result, item);
        }

        return result;
    }

    ///<summary>
    ///Determines whether all elements of an async sequence satisfy a predicate.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="predicate">A function that tests each element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns><c>true</c> if all elements match; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static async ValueTask<bool> AllAsync<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (!predicate(item))
            {
                return false;
            }
        }

        return true;
    }

    ///<summary>
    ///Determines whether any element of an async sequence satisfies a predicate.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="predicate">A function that tests each element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns><c>true</c> if any element matches; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static async ValueTask<bool> AnyAsync<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item))
            {
                return true;
            }
        }

        return false;
    }

    ///<summary>
    ///Projects each element and yields only non-null results. Combines filter and map in a single pass.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="chooser">A function that maps an element to a nullable result.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of non-null projected results.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="chooser"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TResult> Choose<T, TResult>(this IAsyncEnumerable<T> source, Func<T, TResult?> chooser, CancellationToken cancellationToken = default) where TResult : class
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(chooser, nameof(chooser));
        return ChooseRefIterator(source, chooser, cancellationToken);
    }

    ///<summary>
    ///Projects each element and yields only results that have a value.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result value type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="chooser">A function that maps an element to a nullable value.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of non-null projected values.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="chooser"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TResult> Choose<T, TResult>(this IAsyncEnumerable<T> source, Func<T, TResult?> chooser, CancellationToken cancellationToken = default) where TResult : struct
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(chooser, nameof(chooser));
        return ChooseValueIterator(source, chooser, cancellationToken);
    }

    ///<summary>
    ///Counts the elements in an async sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A task that resolves to the number of elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static async ValueTask<int> CountAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        int count = 0;

        await foreach (T _ in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }

    ///<summary>
    ///Returns the first element of an async sequence, or a default value if the sequence is empty.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A task that resolves to the first element or the default value.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static async ValueTask<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            return item;
        }

        return default;
    }

    ///<summary>
    ///Applies an arbitrary async-to-async transformation within a fluent pipeline.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="transform">A function that transforms the async sequence.</param>
    ///<returns>The transformed async sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="transform"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TResult> Pipe<T, TResult>(this IAsyncEnumerable<T> source, Func<IAsyncEnumerable<T>, IAsyncEnumerable<TResult>> transform)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(transform, nameof(transform));
        return transform(source);
    }

    ///<summary>
    ///Produces a sequence of running aggregates.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TAccumulate">The accumulator type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="seed">The initial accumulator value.</param>
    ///<param name="accumulator">A function that combines the accumulator with each element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of intermediate accumulator values.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="accumulator"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TAccumulate> Scan<T, TAccumulate>(this IAsyncEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> accumulator, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(accumulator, nameof(accumulator));
        return ScanIterator(source, seed, accumulator, cancellationToken);
    }

    ///<summary>
    ///Produces a sequence of running aggregates using an asynchronous accumulator.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TAccumulate">The accumulator type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="seed">The initial accumulator value.</param>
    ///<param name="accumulator">An async function that combines the accumulator with each element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of intermediate accumulator values.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="accumulator"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TAccumulate> ScanAsync<T, TAccumulate>(this IAsyncEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, CancellationToken, ValueTask<TAccumulate>> accumulator, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(accumulator, nameof(accumulator));
        return ScanAsyncIterator(source, seed, accumulator, cancellationToken);
    }

    ///<summary>
    ///Projects each element of an async sequence using a synchronous selector.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The projected element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="selector">A function that transforms each element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of projected elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TResult> Select<T, TResult>(this IAsyncEnumerable<T> source, Func<T, TResult> selector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return SelectIterator(source, selector, cancellationToken);
    }

    ///<summary>
    ///Projects each element with its zero-based index.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The projected element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="selector">A function that receives (element, index) and produces a result.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of projected elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TResult> Select<T, TResult>(this IAsyncEnumerable<T> source, Func<T, int, TResult> selector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return SelectIndexedIterator(source, selector, cancellationToken);
    }

    ///<summary>
    ///Projects each element using an asynchronous selector.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The projected element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="selector">An async function that transforms each element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of projected elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TResult> SelectAsync<T, TResult>(this IAsyncEnumerable<T> source, Func<T, CancellationToken, ValueTask<TResult>> selector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return SelectAsyncIterator(source, selector, cancellationToken);
    }

    ///<summary>
    ///Projects each element to an async sub-sequence and flattens the results.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="selector">A function that projects each element to an async sub-sequence.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A flattened async sequence of results.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TResult> SelectMany<T, TResult>(this IAsyncEnumerable<T> source, Func<T, IAsyncEnumerable<TResult>> selector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return SelectManyIterator(source, selector, cancellationToken);
    }

    ///<summary>
    ///Projects each element to a synchronous sub-sequence and flattens the results.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="selector">A function that projects each element to a synchronous collection.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A flattened async sequence of results.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TResult> SelectMany<T, TResult>(this IAsyncEnumerable<T> source, Func<T, IEnumerable<TResult>> selector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return SelectManySyncIterator(source, selector, cancellationToken);
    }
    #endregion
}
