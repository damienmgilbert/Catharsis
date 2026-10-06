namespace Catharsis.Linq;

///<summary>
///Provides advanced fluent pipeline extension methods for <see cref="IEnumerable{T}"/>: <c>Pipe</c> for applying
///arbitrary transformations, <c>Let</c> for introducing intermediate materialized variables, and <c>Choose</c> for
///combined filter-and-map operations.
///</summary>
public static class SequenceOperators
{
    #region Private methods
    private static IEnumerable<TResult> ChooseRefIndexedIterator<T, TResult>(IEnumerable<T> source, Func<T, int, TResult?> chooser) where TResult : class
    {
        int index = 0;

        foreach(T item in source)
        {
            TResult? result = chooser(item, index);

            if(result is not null)
            {
                yield return result;
            }

            index++;
        }
    }

    private static IEnumerable<TResult> ChooseRefIterator<T, TResult>(IEnumerable<T> source, Func<T, TResult?> chooser) where TResult : class
    {
        foreach(T item in source)
        {
            TResult? result = chooser(item);

            if(result is not null)
            {
                yield return result;
            }
        }
    }

    private static IEnumerable<TResult> ChooseValueIterator<T, TResult>(IEnumerable<T> source, Func<T, TResult?> chooser) where TResult : struct
    {
        foreach(T item in source)
        {
            TResult? result = chooser(item);

            if(result.HasValue)
            {
                yield return result.Value;
            }
        }
    }

    private static IEnumerable<T> TapEachIterator<T>(IEnumerable<T> source, Action<T> action)
    {
        foreach(T item in source)
        {
            action(item);
            yield return item;
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Projects each element using <paramref name="chooser"/> and yields only those results that are not <c>null</c>.
    ///Equivalent to <c>Select(chooser).Where(x => x is not null)</c> but in a single pass.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="chooser">A function that maps an element to a nullable result; <c>null</c> indicates the element should be skipped.</param>
    ///<returns>A sequence of non-null projected results.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="chooser"/> is <c>null</c>.</exception>
    public static IEnumerable<TResult> Choose<T, TResult>(this IEnumerable<T> source, Func<T, TResult?> chooser) where TResult : class
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(chooser, nameof(chooser));
        return ChooseRefIterator(source, chooser);
    }

    ///<summary>
    ///Projects each element using <paramref name="chooser"/> and yields only those results that have a value.
    ///Equivalent to <c>Select(chooser).Where(x => x.HasValue).Select(x => x!.Value)</c> but in a single pass.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result value type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="chooser">A function that maps an element to a nullable value; <c>null</c> indicates the element should be skipped.</param>
    ///<returns>A sequence of non-null projected values.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="chooser"/> is <c>null</c>.</exception>
    public static IEnumerable<TResult> Choose<T, TResult>(this IEnumerable<T> source, Func<T, TResult?> chooser) where TResult : struct
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(chooser, nameof(chooser));
        return ChooseValueIterator(source, chooser);
    }

    ///<summary>
    ///Projects each element using <paramref name="chooser"/> with its zero-based index and yields only non-null
    ///results.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="chooser">A function that receives an element and its index and returns a nullable result.</param>
    ///<returns>A sequence of non-null projected results.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="chooser"/> is <c>null</c>.</exception>
    public static IEnumerable<TResult> Choose<T, TResult>(this IEnumerable<T> source, Func<T, int, TResult?> chooser) where TResult : class
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(chooser, nameof(chooser));
        return ChooseRefIndexedIterator(source, chooser);
    }

    ///<summary>
    ///Materializes the source sequence into a <see cref="List{T}"/> and passes it to <paramref name="selector"/>,
    ///allowing the materialized collection to be referenced multiple times without re-enumeration.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="selector">A function that receives the materialized list and returns a result.</param>
    ///<returns>The result of applying <paramref name="selector"/> to the materialized collection.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public static TResult Let<T, TResult>(this IEnumerable<T> source, Func<IReadOnlyList<T>, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));

        IReadOnlyList<T> materialized = source as IReadOnlyList<T> ?? source.ToList().AsReadOnly();
        return selector(materialized);
    }

    ///<summary>
    ///Materializes the source sequence and passes it to <paramref name="selector"/>, returning an ///<see
    ///cref="IEnumerable{TResult}"/> for continued chaining.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="selector">A function that receives the materialized list and returns a new sequence.</param>
    ///<returns>The sequence returned by <paramref name="selector"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public static IEnumerable<TResult> Let<T, TResult>(this IEnumerable<T> source, Func<IReadOnlyList<T>, IEnumerable<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));

        IReadOnlyList<T> materialized = source as IReadOnlyList<T> ?? source.ToList().AsReadOnly();
        return selector(materialized);
    }

    ///<summary>
    ///Materializes the source sequence into a variable, applies a <paramref name="filter"/> that can reference the
    ///whole collection (e.g. to compare against the average), and returns the filtered elements.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="filter">A function that receives the materialized list and returns a filtered sequence.</param>
    ///<returns>The filtered sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="filter"/> is <c>null</c>.</exception>
    public static IEnumerable<T> LetWhere<T>(this IEnumerable<T> source, Func<IReadOnlyList<T>, IEnumerable<T>> filter)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(filter, nameof(filter));

        IReadOnlyList<T> materialized = source as IReadOnlyList<T> ?? source.ToList().AsReadOnly();
        return filter(materialized);
    }

    ///<summary>
    ///Applies an arbitrary transformation function to the entire sequence, enabling inline pipeline composition. The
    ///source sequence is passed as-is to <paramref name="transform"/>.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="transform">A function that receives the sequence and returns a transformed result.</param>
    ///<returns>The result of applying <paramref name="transform"/> to <paramref name="source"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="transform"/> is <c>null</c>.</exception>
    public static TResult Pipe<T, TResult>(this IEnumerable<T> source, Func<IEnumerable<T>, TResult> transform)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(transform, nameof(transform));
        return transform(source);
    }

    ///<summary>
    ///Applies an arbitrary sequence-to-sequence transformation, returning the result as a lazy ///<see
    ///cref="IEnumerable{T}"/> for continued chaining.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="transform">A function that receives the sequence and returns a new sequence.</param>
    ///<returns>The sequence returned by <paramref name="transform"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="transform"/> is <c>null</c>.</exception>
    public static IEnumerable<TResult> Pipe<T, TResult>(this IEnumerable<T> source, Func<IEnumerable<T>, IEnumerable<TResult>> transform)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(transform, nameof(transform));
        return transform(source);
    }

    ///<summary>
    ///Executes a side-effect action on the entire sequence without modifying it, then returns the original sequence.
    ///Useful for logging or diagnostics within a pipeline.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="action">A side-effect action that receives the sequence.</param>
    ///<returns>The original <paramref name="source"/> sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is <c>null</c>.</exception>
    public static IEnumerable<T> PipeTap<T>(this IEnumerable<T> source, Action<IEnumerable<T>> action)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(action, nameof(action));
        action(source);
        return source;
    }

    ///<summary>
    ///Executes a side-effect action on each element as it flows through the pipeline. The source sequence is returned
    ///lazily with the action applied to each element.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="action">An action invoked on each element.</param>
    ///<returns>A lazy sequence that yields each element after invoking <paramref name="action"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is <c>null</c>.</exception>
    public static IEnumerable<T> PipeTapEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(action, nameof(action));
        return TapEachIterator(source, action);
    }
    #endregion
}
