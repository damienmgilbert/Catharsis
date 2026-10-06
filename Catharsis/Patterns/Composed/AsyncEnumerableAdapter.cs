using System.Runtime.CompilerServices;

namespace Catharsis.Patterns.Composed;

///<summary>
///Adapts between <see cref="IEnumerable{T}"/> and <see cref="IAsyncEnumerable{T}"/>, so a synchronous source can feed
///an asynchronous consumer and the reverse.
///</summary>
public static class AsyncEnumerableAdapter
{
    #region Private methods
    private static async IAsyncEnumerable<T> Iterate<T>(IEnumerable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach(T item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private static IEnumerable<T> IterateBlocking<T>(IAsyncEnumerable<T> source, CancellationToken cancellationToken)
    {
        IAsyncEnumerator<T> enumerator = source.GetAsyncEnumerator(cancellationToken);

        try
        {
            while(enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult())
            {
                yield return enumerator.Current;
            }
        } finally
        {
            enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Presents a synchronous sequence as an asynchronous one. The source is read lazily, one element per step, and
    ///cancellation is checked before each.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The sequence to adapt.</param>
    ///<param name="cancellationToken">A token that stops enumeration between elements.</param>
    ///<returns>An asynchronous view of <paramref name="source"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        return Iterate(source, cancellationToken);
    }

    ///<summary>
    ///Presents an asynchronous sequence as a synchronous one by <em>blocking</em> the calling thread on each element.
    ///</summary>
    ///<remarks>
    ///Blocking on asynchronous work can deadlock under a single-threaded synchronization context and ties up a thread
    ///while waiting; prefer consuming the asynchronous sequence with <c>await foreach</c> where you can. This exists
    ///for code that genuinely cannot be asynchronous, such as a synchronous interface implementation.
    ///</remarks>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The asynchronous sequence to adapt.</param>
    ///<param name="cancellationToken">A token that stops enumeration.</param>
    ///<returns>A synchronous view of <paramref name="source"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<T> ToEnumerable<T>(IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        return IterateBlocking(source, cancellationToken);
    }
    #endregion
}
