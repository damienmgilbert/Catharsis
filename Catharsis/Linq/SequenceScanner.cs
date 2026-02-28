namespace Catharsis.Linq;

///<summary>
///Provides additional <c>Scan</c> and <c>Pairwise</c> overloads that complement the existing implementations in ///<see
///cref="SequenceComposer"/> and <see cref="SequenceWindow"/>. Includes seedless scan, indexed pairwise, and pairwise
///filtering.
///</summary>
public static class SequenceScanner
{
    #region Private methods
    private static IEnumerable<(int Index, T Previous, T Current)> PairwiseIndexedIterator<T>(IEnumerable<T> source)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            yield break;
        }

        T previous = enumerator.Current;
        int index = 0;

        while (enumerator.MoveNext())
        {
            yield return (index, previous, enumerator.Current);
            previous = enumerator.Current;
            index++;
        }
    }

    private static IEnumerable<(T Previous, T Current)> PairwiseWhereIterator<T>(IEnumerable<T> source, Func<T, T, bool> predicate)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            yield break;
        }

        T previous = enumerator.Current;

        while (enumerator.MoveNext())
        {
            if (predicate(previous, enumerator.Current))
            {
                yield return (previous, enumerator.Current);
            }

            previous = enumerator.Current;
        }
    }

    private static IEnumerable<T> ScanSeedlessIterator<T>(IEnumerable<T> source, Func<T, T, T> accumulator)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            throw new InvalidOperationException("Source sequence must contain at least one element for seedless scan.");
        }

        T current = enumerator.Current;
        yield return current;

        while (enumerator.MoveNext())
        {
            current = accumulator(current, enumerator.Current);
            yield return current;
        }
    }

    private static IEnumerable<TResult> ScanSelectIterator<T, TAccumulate, TResult>(IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector)
    {
        TAccumulate current = seed;
        yield return resultSelector(current);

        foreach (T element in source)
        {
            current = accumulator(current, element);
            yield return resultSelector(current);
        }
    }

    private static IEnumerable<TAccumulate> ScanWhileIterator<T, TAccumulate>(IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> accumulator, Func<TAccumulate, bool> predicate)
    {
        TAccumulate current = seed;

        if (!predicate(current))
        {
            yield break;
        }

        yield return current;

        foreach (T element in source)
        {
            current = accumulator(current, element);

            if (!predicate(current))
            {
                yield break;
            }

            yield return current;
        }
    }

    private static IEnumerable<(T First, T Second, T Third)> TriplewiseIterator<T>(IEnumerable<T> source)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            yield break;
        }

        T first = enumerator.Current;

        if (!enumerator.MoveNext())
        {
            yield break;
        }

        T second = enumerator.Current;

        while (enumerator.MoveNext())
        {
            yield return (first, second, enumerator.Current);
            first = second;
            second = enumerator.Current;
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Produces a sequence of overlapping pairs of consecutive elements, each annotated with the zero-based index of the
    ///first element in the pair.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<returns>A sequence of <c>(Index, Previous, Current)</c> tuples. Empty if the source has fewer than 2 elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<(int Index, T Previous, T Current)> PairwiseIndexed<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return PairwiseIndexedIterator(source);
    }

    ///<summary>
    ///Produces a sequence of overlapping pairs of consecutive elements, filtered by <paramref name="predicate"/>. Only
    ///pairs for which the predicate returns <c>true</c> are yielded.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="predicate">A function that tests each pair; receives (previous, current).</param>
    ///<returns>A sequence of pairs that satisfy <paramref name="predicate"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static IEnumerable<(T Previous, T Current)> PairwiseWhere<T>(this IEnumerable<T> source, Func<T, T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return PairwiseWhereIterator(source, predicate);
    }

    ///<summary>
    ///Produces a sequence of running aggregates without an explicit seed. The first element of the source becomes the
    ///initial accumulator value.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence. Must contain at least one element.</param>
    ///<param name="accumulator">A function that combines the current accumulator with the next element.</param>
    ///<returns>A sequence of intermediate accumulator values starting with the first element.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="accumulator"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException"><paramref name="source"/> is empty.</exception>
    public static IEnumerable<T> ScanSeedless<T>(this IEnumerable<T> source, Func<T, T, T> accumulator)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(accumulator, nameof(accumulator));
        return ScanSeedlessIterator(source, accumulator);
    }

    ///<summary>
    ///Produces a sequence of running aggregates and projects each intermediate result using ///<paramref
    ///name="resultSelector"/>.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TAccumulate">The accumulator type.</typeparam>
    ///<typeparam name="TResult">The projected result type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="seed">The initial accumulator value.</param>
    ///<param name="accumulator">A function that combines the current accumulator with the next element.</param>
    ///<param name="resultSelector">A function that projects each accumulator value to a result.</param>
    ///<returns>A sequence of projected intermediate accumulator values.</returns>
    ///<exception cref="ArgumentNullException">Any delegate argument is <c>null</c>.</exception>
    public static IEnumerable<TResult> ScanSelect<T, TAccumulate, TResult>(this IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(accumulator, nameof(accumulator));
        ArgumentNullException.ThrowIfNull(resultSelector, nameof(resultSelector));
        return ScanSelectIterator(source, seed, accumulator, resultSelector);
    }

    ///<summary>
    ///Produces a sequence of running aggregates while a condition holds. Once <paramref name="predicate"/> returns
    public static IEnumerable<TAccumulate> ScanWhile<T, TAccumulate>(this IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> accumulator, Func<TAccumulate, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(accumulator, nameof(accumulator));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return ScanWhileIterator(source, seed, accumulator, predicate);
    }

    ///<summary>
    ///Produces a sequence of overlapping triples of consecutive elements: (e0, e1, e2), (e1, e2, e3), etc.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<returns>A sequence of three-element tuples. Empty if the source has fewer than 3 elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<(T First, T Second, T Third)> Triplewise<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return TriplewiseIterator(source);
    }
    #endregion
}
