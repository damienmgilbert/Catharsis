using System.Runtime.CompilerServices;

namespace Catharsis.Linq;

///<summary>
///Provides factory methods for creating <see cref="IAsyncEnumerable{T}"/> sequences from synchronous sources, async
///generators, repetition patterns, and deferred factories.
///</summary>
public static class AsyncEnumerableFactory
{
    #region Private methods
    private static async IAsyncEnumerable<T> CreateAsyncIterator<T>(int count, Func<int, CancellationToken, ValueTask<T>> factory, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for(int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return await factory(i, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async IAsyncEnumerable<T> CreateIterator<T>(int count, Func<int, T> factory, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for(int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return factory(i);
        }
    }

    private static async IAsyncEnumerable<T> DeferIterator<T>(Func<CancellationToken, IAsyncEnumerable<T>> factory, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach(T item in factory(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> EmptyIterator<T>([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    private static async IAsyncEnumerable<T> FromEnumerableIterator<T>(IEnumerable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach(T item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> GenerateAsyncIterator<T>(T seed, Func<T, bool> predicate, Func<T, CancellationToken, ValueTask<T>> generator, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        T current = seed;

        while(predicate(current))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return current;
            current = await generator(current, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async IAsyncEnumerable<T> GenerateIterator<T>(T seed, Func<T, bool> predicate, Func<T, T> generator, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for(T current = seed; predicate(current); current = generator(current))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return current;
        }
    }

    private static async IAsyncEnumerable<int> RangeIterator(int start, int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for(int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return start + i;
        }
    }

    private static async IAsyncEnumerable<T> RepeatIterator<T>(T value, int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for(int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return value;
        }
    }

    private static async IAsyncEnumerable<T> ReturnIterator<T>(T value, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield return value;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Generates an async sequence using a factory function that receives the zero-based index.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="count">The number of elements to generate.</param>
    ///<param name="factory">A function that produces an element given its index.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A lazily-evaluated async sequence.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Create<T>(int count, Func<int, T> factory, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        return CreateIterator(count, factory, cancellationToken);
    }

    ///<summary>
    ///Generates an async sequence using an async factory function.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="count">The number of elements to generate.</param>
    ///<param name="factory">An async function that produces an element given its index.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A lazily-evaluated async sequence.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> CreateAsync<T>(int count, Func<int, CancellationToken, ValueTask<T>> factory, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        return CreateAsyncIterator(count, factory, cancellationToken);
    }

    ///<summary>
    ///Defers the creation of an <see cref="IAsyncEnumerable{T}"/> until iteration begins.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="factory">A function that creates the async sequence when enumeration starts.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A deferred async sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Defer<T>(Func<CancellationToken, IAsyncEnumerable<T>> factory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        return DeferIterator(factory, cancellationToken);
    }

    ///<summary>
    ///Returns an empty <see cref="IAsyncEnumerable{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<returns>An async sequence with no elements.</returns>
    public static IAsyncEnumerable<T> Empty<T>() => EmptyIterator<T>();

    ///<summary>
    ///Wraps a synchronous <see cref="IEnumerable{T}"/> as an <see cref="IAsyncEnumerable{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The synchronous source sequence.</param>
    ///<returns>An async sequence that yields the same elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> FromEnumerable<T>(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return FromEnumerableIterator(source);
    }

    ///<summary>
    ///Generates an async sequence by repeatedly applying <paramref name="generator"/> starting from ///<paramref
    ///name="seed"/> while <paramref name="predicate"/> returns <c>true</c>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="seed">The initial value.</param>
    ///<param name="predicate">A function that determines whether to continue generating.</param>
    ///<param name="generator">A function that produces the next element from the current one.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A lazily-evaluated async sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="generator"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> Generate<T>(T seed, Func<T, bool> predicate, Func<T, T> generator, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(generator, nameof(generator));
        return GenerateIterator(seed, predicate, generator, cancellationToken);
    }

    ///<summary>
    ///Generates an async sequence by repeatedly applying an async <paramref name="generator"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="seed">The initial value.</param>
    ///<param name="predicate">A function that determines whether to continue generating.</param>
    ///<param name="generator">An async function that produces the next element.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>A lazily-evaluated async sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="generator"/> is <c>null</c>.</exception>
    public static IAsyncEnumerable<T> GenerateAsync<T>(T seed, Func<T, bool> predicate, Func<T, CancellationToken, ValueTask<T>> generator, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(generator, nameof(generator));
        return GenerateAsyncIterator(seed, predicate, generator, cancellationToken);
    }

    ///<summary>
    ///Creates an async sequence of consecutive integers.
    ///</summary>
    ///<param name="start">The first integer in the sequence.</param>
    ///<param name="count">The number of integers to generate.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of integers.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static IAsyncEnumerable<int> Range(int start, int count, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        return RangeIterator(start, count, cancellationToken);
    }

    ///<summary>
    ///Creates an async sequence that repeats <paramref name="value"/> a specified number of times.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="value">The value to repeat.</param>
    ///<param name="count">The number of times to repeat.</param>
    ///<param name="cancellationToken">A token to cancel the iteration.</param>
    ///<returns>An async sequence of repeated values.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static IAsyncEnumerable<T> Repeat<T>(T value, int count, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        return RepeatIterator(value, count, cancellationToken);
    }

    ///<summary>
    ///Creates an <see cref="IAsyncEnumerable{T}"/> containing a single element.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="value">The single element to yield.</param>
    ///<returns>An async sequence containing one element.</returns>
    public static IAsyncEnumerable<T> Return<T>(T value) => ReturnIterator(value);
    #endregion
}
