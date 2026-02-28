namespace Catharsis.Linq;

///<summary>
///Provides factory methods for generating <see cref="IEnumerable{T}"/>, <see cref="IOrderedEnumerable{TElement}"/>, and
public static class SequenceFactory
{
    #region Private methods
    private static IEnumerable<T> CreateIterator<T>(int count, Func<int, T> factory)
    {
        for (int i = 0; i < count; i++)
        {
            yield return factory(i);
        }
    }

    private static IEnumerable<T> CycleIterator<T>(IEnumerable<T> source)
    {
        List<T> buffer = source.ToList();

        if (buffer.Count == 0)
        {
            throw new InvalidOperationException("Source sequence must contain at least one element to cycle.");
        }

        while (true)
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                yield return buffer[i];
            }
        }
    }

    private static IEnumerable<T> GenerateIterator<T>(T seed, Func<T, bool> predicate, Func<T, T> generator)
    {
        for (T current = seed; predicate(current); current = generator(current))
        {
            yield return current;
        }
    }

    private static IEnumerable<TResult> GenerateIterator<TState, TResult>(TState seed, Func<TState, bool> predicate, Func<TState, TState> generator, Func<TState, TResult> resultSelector)
    {
        for (TState current = seed; predicate(current); current = generator(current))
        {
            yield return resultSelector(current);
        }
    }

    private static IEnumerable<T> InfiniteIterator<T>(T seed, Func<T, T> generator)
    {
        T current = seed;

        while (true)
        {
            yield return current;
            current = generator(current);
        }
    }

    private static IEnumerable<T> RandomIterator<T>(Func<Random, T> factory, Random random)
    {
        while (true)
        {
            yield return factory(random);
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Generates a sequence using a factory function that receives the zero-based index of the element being created.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="count">The number of elements to generate.</param>
    ///<param name="factory">A function that produces an element given its index.</param>
    ///<returns>A lazily-evaluated sequence of <paramref name="count"/> elements.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
    public static IEnumerable<T> Create<T>(int count, Func<int, T> factory)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        return CreateIterator(count, factory);
    }

    ///<summary>
    ///Creates an infinite sequence that repeats the elements of <paramref name="source"/> cyclically. The caller is
    ///responsible for limiting the sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence to cycle through. Must contain at least one element.</param>
    ///<returns>An infinite lazily-evaluated cyclic sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException"><paramref name="source"/> is empty.</exception>
    public static IEnumerable<T> Cycle<T>(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return CycleIterator(source);
    }

    ///<summary>
    ///Returns an empty <see cref="IEnumerable{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<returns>An empty sequence.</returns>
    public static IEnumerable<T> Empty<T>() { return Enumerable.Empty<T>(); }

    ///<summary>
    ///Generates a sequence by repeatedly applying <paramref name="generator"/> to produce successive elements, starting
    ///from <paramref name="seed"/>. The sequence continues while <paramref name="predicate"/> returns ///<c>true</c>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="seed">The initial value.</param>
    ///<param name="predicate">A function that determines whether to continue generating.</param>
    ///<param name="generator">A function that produces the next element from the current one.</param>
    ///<returns>A lazily-evaluated sequence of generated elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="generator"/> is <c>null</c>.</exception>
    public static IEnumerable<T> Generate<T>(T seed, Func<T, bool> predicate, Func<T, T> generator)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(generator, nameof(generator));
        return GenerateIterator(seed, predicate, generator);
    }

    ///<summary>
    ///Generates a sequence by repeatedly applying <paramref name="generator"/> to produce successive elements, starting
    ///from <paramref name="seed"/>, selecting the result with <paramref name="resultSelector"/>.
    ///</summary>
    ///<typeparam name="TState">The state type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="seed">The initial state.</param>
    ///<param name="predicate">A function that determines whether to continue generating.</param>
    ///<param name="generator">A function that produces the next state from the current state.</param>
    ///<param name="resultSelector">A function that projects the state into a result element.</param>
    ///<returns>A lazily-evaluated sequence of projected elements.</returns>
    ///<exception cref="ArgumentNullException">Any delegate argument is <c>null</c>.</exception>
    public static IEnumerable<TResult> Generate<TState, TResult>(TState seed, Func<TState, bool> predicate, Func<TState, TState> generator, Func<TState, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(generator, nameof(generator));
        ArgumentNullException.ThrowIfNull(resultSelector, nameof(resultSelector));
        return GenerateIterator(seed, predicate, generator, resultSelector);
    }

    ///<summary>
    ///Creates an <see cref="IGrouping{TKey, TElement}"/> from a key and a sequence of elements.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="key">The key of the grouping.</param>
    ///<param name="elements">The elements in the group.</param>
    ///<returns>An <see cref="IGrouping{TKey, TElement}"/> containing the specified elements under the specified key.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="elements"/> is <c>null</c>.</exception>
    public static IGrouping<TKey, TElement> Grouping<TKey, TElement>(TKey key, IEnumerable<TElement> elements)
    {
        ArgumentNullException.ThrowIfNull(elements, nameof(elements));
        return new SimpleGrouping<TKey, TElement>(key, elements);
    }

    ///<summary>
    ///Creates an <see cref="IGrouping{TKey, TElement}"/> from a key and explicit element values.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="key">The key of the grouping.</param>
    ///<param name="elements">The elements in the group.</param>
    ///<returns>An <see cref="IGrouping{TKey, TElement}"/> containing the specified elements under the specified key.</returns>
    public static IGrouping<TKey, TElement> Grouping<TKey, TElement>(TKey key, params TElement[] elements) { return new SimpleGrouping<TKey, TElement>(key, elements); }

    ///<summary>
    ///Creates multiple <see cref="IGrouping{TKey,TElement}"/> instances by grouping the elements with the supplied key
    ///selector.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="source">The source elements to group.</param>
    ///<param name="keySelector">A function that extracts the key from each element.</param>
    ///<returns>A sequence of groupings.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IEnumerable<IGrouping<TKey, TElement>> Groupings<TKey, TElement>(IEnumerable<TElement> source, Func<TElement, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return source.GroupBy(keySelector);
    }

    ///<summary>
    ///Generates an infinite sequence by repeatedly applying <paramref name="generator"/> starting from ///<paramref
    ///name="seed"/>. The caller is responsible for limiting the sequence (e.g. via <c>Take</c>).
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="seed">The initial value.</param>
    ///<param name="generator">A function that produces the next element from the current one.</param>
    ///<returns>An infinite lazily-evaluated sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="generator"/> is <c>null</c>.</exception>
    public static IEnumerable<T> Infinite<T>(T seed, Func<T, T> generator)
    {
        ArgumentNullException.ThrowIfNull(generator, nameof(generator));
        return InfiniteIterator(seed, generator);
    }

    ///<summary>
    ///Creates an <see cref="IOrderedEnumerable{TElement}"/> by ordering the source sequence with the default comparer.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence to order.</param>
    ///<returns>An ordered sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IOrderedEnumerable<T> Ordered<T>(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.Order();
    }

    ///<summary>
    ///Creates an <see cref="IOrderedEnumerable{TElement}"/> by ordering the source sequence using the specified key
    ///selector.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="source">The source sequence to order.</param>
    ///<param name="keySelector">A function to extract a key from each element.</param>
    ///<param name="comparer">An optional comparer; defaults to the default comparer.</param>
    ///<returns>An ordered sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IOrderedEnumerable<T> OrderedBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return source.OrderBy(keySelector, comparer);
    }

    ///<summary>
    ///Creates an <see cref="IOrderedEnumerable{TElement}"/> by ordering the source sequence in descending order using
    ///the specified key selector.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<param name="source">The source sequence to order.</param>
    ///<param name="keySelector">A function to extract a key from each element.</param>
    ///<param name="comparer">An optional comparer; defaults to the default comparer.</param>
    ///<returns>An ordered sequence in descending order.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
    public static IOrderedEnumerable<T> OrderedByDescending<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        return source.OrderByDescending(keySelector, comparer);
    }

    ///<summary>
    ///Generates a sequence of random elements using the supplied factory. The caller is responsible for limiting the
    ///sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="factory">A function that produces a random element given a <see cref="Random"/> instance.</param>
    ///<param name="random">An optional <see cref="Random"/> instance; defaults to <see cref="Random.Shared"/>.</param>
    ///<returns>An infinite lazily-evaluated sequence of random elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="factory"/> is <c>null</c>.</exception>
    public static IEnumerable<T> Random<T>(Func<Random, T> factory, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        return RandomIterator(factory, random ?? System.Random.Shared);
    }

    ///<summary>
    ///Returns a sequence of consecutive integers starting at <paramref name="start"/>.
    ///</summary>
    ///<param name="start">The first integer in the sequence.</param>
    ///<param name="count">The number of integers to generate.</param>
    ///<returns>A sequence of <paramref name="count"/> consecutive integers.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static IEnumerable<int> Range(int start, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        return Enumerable.Range(start, count);
    }

    ///<summary>
    ///Returns a sequence that repeats <paramref name="element"/> the specified number of times.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="element">The element to repeat.</param>
    ///<param name="count">The number of times to repeat the element.</param>
    ///<returns>A sequence of <paramref name="count"/> copies of <paramref name="element"/>.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static IEnumerable<T> Repeat<T>(T element, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        return Enumerable.Repeat(element, count);
    }

    ///<summary>
    ///Returns a sequence containing a single element.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="element">The element to wrap.</param>
    ///<returns>A sequence containing only <paramref name="element"/>.</returns>
    public static IEnumerable<T> Singleton<T>(T element) { return [element]; }
    #endregion

    private sealed class SimpleGrouping<TKey, TElement>(TKey key, IEnumerable<TElement> elements) : IGrouping<TKey, TElement>
    {
        #region Fields
        private readonly List<TElement> _elements = elements.ToList();
        #endregion

        #region Explicit interface implementations
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
        #endregion

        #region Public methods
        public IEnumerator<TElement> GetEnumerator() { return _elements.GetEnumerator(); }
        #endregion

        #region Public properties
        public TKey Key => key;
        #endregion
    }
}
