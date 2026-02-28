using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Catharsis.Linq;

///<summary>
///Provides extension methods for merging, interleaving, zipping, and concatenating multiple ///<see
///cref="IAsyncEnumerable{T}"/> sources into a single async sequence.
///</summary>
public static class AsyncSequenceMerger
{
    #region Private methods
    private static async IAsyncEnumerable<T> AppendIterator<T>(IAsyncEnumerable<T> source, T value, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }

        yield return value;
    }

    private static async IAsyncEnumerable<T> ConcatIterator<T>(IAsyncEnumerable<T> first, IAsyncEnumerable<T> second, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (T item in first.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }

        await foreach (T item in second.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> ConcatManyIterator<T>(IAsyncEnumerable<T> source, IAsyncEnumerable<T>[] others, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }

        foreach (IAsyncEnumerable<T> other in others)
        {
            await foreach (T item in other.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }

    private static async IAsyncEnumerable<T> InterleaveIterator<T>(IAsyncEnumerable<T> first, IAsyncEnumerable<T> second, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using IAsyncEnumerator<T> e1 = first.GetAsyncEnumerator(cancellationToken);
        await using IAsyncEnumerator<T> e2 = second.GetAsyncEnumerator(cancellationToken);

        bool has1 = await e1.MoveNextAsync().ConfigureAwait(false);
        bool has2 = await e2.MoveNextAsync().ConfigureAwait(false);

        while (has1 && has2)
        {
            yield return e1.Current;
            yield return e2.Current;
            has1 = await e1.MoveNextAsync().ConfigureAwait(false);
            has2 = await e2.MoveNextAsync().ConfigureAwait(false);
        }

        while (has1)
        {
            yield return e1.Current;
            has1 = await e1.MoveNextAsync().ConfigureAwait(false);
        }

        while (has2)
        {
            yield return e2.Current;
            has2 = await e2.MoveNextAsync().ConfigureAwait(false);
        }
    }

    private static async IAsyncEnumerable<T> MergeIterator<T>(IEnumerable<IAsyncEnumerable<T>> sources, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Channel<T> channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

        List<IAsyncEnumerable<T>> sourceList = sources as List<IAsyncEnumerable<T>> ?? sources.ToList();

        Task[] producers = new Task[sourceList.Count];

        for (int i = 0; i < sourceList.Count; i++)
        {
            IAsyncEnumerable<T> src = sourceList[i];
            producers[i] = Task.Run(
                           async () =>
                           {
                               await foreach (T item in src.WithCancellation(cancellationToken).ConfigureAwait(false))
                               {
                                   await channel.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
                               }
                           },
                           cancellationToken);
        }

        _ = Task.Run(
            async () =>
            {
                try
                {
                    await Task.WhenAll(producers).ConfigureAwait(false);
                }
                finally
                {
                    channel.Writer.Complete();
                }
            },
            cancellationToken);

        await foreach (T item in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> PrependIterator<T>(IAsyncEnumerable<T> source, T value, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return value;

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    private static async IAsyncEnumerable<(TFirst First, TSecond Second)> ZipIterator<TFirst, TSecond>(IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using IAsyncEnumerator<TFirst> e1 = first.GetAsyncEnumerator(cancellationToken);
        await using IAsyncEnumerator<TSecond> e2 = second.GetAsyncEnumerator(cancellationToken);

        while (await e1.MoveNextAsync().ConfigureAwait(false) && await e2.MoveNextAsync().ConfigureAwait(false))
        {
            yield return (e1.Current, e2.Current);
        }
    }

    private static async IAsyncEnumerable<(TFirst First, TSecond Second)> ZipLongestIterator<TFirst, TSecond>(IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, TFirst? defaultFirst, TSecond? defaultSecond, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using IAsyncEnumerator<TFirst> e1 = first.GetAsyncEnumerator(cancellationToken);
        await using IAsyncEnumerator<TSecond> e2 = second.GetAsyncEnumerator(cancellationToken);

        bool has1 = await e1.MoveNextAsync().ConfigureAwait(false);
        bool has2 = await e2.MoveNextAsync().ConfigureAwait(false);

        while (has1 || has2)
        {
            TFirst? v1 = has1 ? e1.Current : defaultFirst;
            TSecond? v2 = has2 ? e2.Current : defaultSecond;
            yield return (v1!, v2!);

            if (has1)
            {
                has1 = await e1.MoveNextAsync().ConfigureAwait(false);
            }

            if (has2)
            {
                has2 = await e2.MoveNextAsync().ConfigureAwait(false);
            }
        }
    }

    private static async IAsyncEnumerable<TResult> ZipWithSelectorIterator<TFirst, TSecond, TResult>(IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using IAsyncEnumerator<TFirst> e1 = first.GetAsyncEnumerator(cancellationToken);
        await using IAsyncEnumerator<TSecond> e2 = second.GetAsyncEnumerator(cancellationToken);

        while (await e1.MoveNextAsync().ConfigureAwait(false) && await e2.MoveNextAsync().ConfigureAwait(false))
        {
            yield return resultSelector(e1.Current, e2.Current);
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Appends a value to an async sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="value">The value to append.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence with elements of <paramref name="source"/> followed by <paramref name="value"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Append<T>(this IAsyncEnumerable<T> source, T value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return AppendIterator(source, value, cancellationToken);
    }

    ///<summary>
    ///Concatenates multiple async sequences in order.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The first async sequence.</param>
    ///<param name="others">Additional async sequences to append.</param>
    ///<returns>A concatenated async sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="others"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> source, params IAsyncEnumerable<T>[] others)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(others, nameof(others));
        return ConcatManyIterator(source, others);
    }

    ///<summary>
    ///Concatenates two async sequences, yielding all elements of <paramref name="source"/> followed by all elements of
    ///<paramref name="second"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The first async sequence.</param>
    ///<param name="second">The second async sequence.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A concatenated async sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> source, IAsyncEnumerable<T> second, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return ConcatIterator(source, second, cancellationToken);
    }

    ///<summary>
    ///Interleaves two async sequences element-by-element. When one sequence is exhausted, the remaining elements of the
    ///other are yielded.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The first async sequence.</param>
    ///<param name="second">The second async sequence.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An interleaved async sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Interleave<T>(this IAsyncEnumerable<T> source, IAsyncEnumerable<T> second, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return InterleaveIterator(source, second, cancellationToken);
    }

    ///<summary>
    ///Merges multiple async sequences concurrently, yielding elements as they become available from any source. Uses
    ///<see cref="Channel{T}"/> for safe concurrent aggregation.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="sources">The async sequences to merge.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence containing elements from all sources in arrival order.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="sources"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Merge<T>(IEnumerable<IAsyncEnumerable<T>> sources, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources, nameof(sources));
        return MergeIterator(sources, cancellationToken);
    }

    ///<summary>
    ///Merges two async sequences concurrently, yielding elements as they become available.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The first async sequence.</param>
    ///<param name="second">The second async sequence.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence containing elements from both sources in arrival order.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Merge<T>(this IAsyncEnumerable<T> source, IAsyncEnumerable<T> second, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return MergeIterator([source, second], cancellationToken);
    }

    ///<summary>
    ///Prepends a value to an async sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The async source sequence.</param>
    ///<param name="value">The value to prepend.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence with <paramref name="value"/> followed by elements of <paramref name="source"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Prepend<T>(this IAsyncEnumerable<T> source, T value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return PrependIterator(source, value, cancellationToken);
    }

    ///<summary>
    ///Zips two async sequences into tuples. The result has the length of the shorter sequence.
    ///</summary>
    ///<typeparam name="TFirst">The first sequence element type.</typeparam>
    ///<typeparam name="TSecond">The second sequence element type.</typeparam>
    ///<param name="source">The first async sequence.</param>
    ///<param name="second">The second async sequence.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of tuples.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IAsyncEnumerable<TFirst> source, IAsyncEnumerable<TSecond> second, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return ZipIterator(source, second, cancellationToken);
    }

    ///<summary>
    ///Zips two async sequences using a result selector.
    ///</summary>
    ///<typeparam name="TFirst">The first sequence element type.</typeparam>
    ///<typeparam name="TSecond">The second sequence element type.</typeparam>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="source">The first async sequence.</param>
    ///<param name="second">The second async sequence.</param>
    ///<param name="resultSelector">A function that combines elements from both sequences.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of projected results.</returns>
    ///<exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
    public static IAsyncEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IAsyncEnumerable<TFirst> source, IAsyncEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        ArgumentNullException.ThrowIfNull(resultSelector, nameof(resultSelector));
        return ZipWithSelectorIterator(source, second, resultSelector, cancellationToken);
    }

    ///<summary>
    ///Zips two async sequences into tuples, padding the shorter sequence with default values so that no elements are
    ///lost.
    ///</summary>
    ///<typeparam name="TFirst">The first sequence element type.</typeparam>
    ///<typeparam name="TSecond">The second sequence element type.</typeparam>
    ///<param name="source">The first async sequence.</param>
    ///<param name="second">The second async sequence.</param>
    ///<param name="defaultFirst">Default value for the first sequence when exhausted.</param>
    ///<param name="defaultSecond">Default value for the second sequence when exhausted.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of tuples with the length of the longer input.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<(TFirst First, TSecond Second)> ZipLongest<TFirst, TSecond>(this IAsyncEnumerable<TFirst> source, IAsyncEnumerable<TSecond> second, TFirst? defaultFirst = default, TSecond? defaultSecond = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return ZipLongestIterator(source, second, defaultFirst, defaultSecond, cancellationToken);
    }
    #endregion
}
