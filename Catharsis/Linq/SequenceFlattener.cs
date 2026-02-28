namespace Catharsis.Linq;

///<summary>
///Provides extension methods for flattening nested <see cref="IEnumerable{T}"/> structures, ungrouping ///<see
///cref="IGrouping{TKey,TElement}"/> sequences, and performing recursive descent into hierarchical data.
///</summary>
public static class SequenceFlattener
{
    #region Private methods
    private static IEnumerable<T> FlattenDepthFirstIterator<T>(IEnumerable<T> source, Func<T, IEnumerable<T>?> childrenSelector)
    {
        Stack<T> stack = new(source.Reverse());

        while (stack.Count > 0)
        {
            T current = stack.Pop();
            yield return current;

            IEnumerable<T>? children = childrenSelector(current);

            if (children is not null)
            {
                foreach (T child in children.Reverse())
                {
                    stack.Push(child);
                }
            }
        }
    }

    private static IEnumerable<T> FlattenRecursiveBreadthFirst<T>(IEnumerable<T> source, Func<T, IEnumerable<T>?> childrenSelector)
    {
        Queue<T> queue = new(source);

        while (queue.Count > 0)
        {
            T current = queue.Dequeue();
            yield return current;

            IEnumerable<T>? children = childrenSelector(current);

            if (children is not null)
            {
                foreach (T child in children)
                {
                    queue.Enqueue(child);
                }
            }
        }
    }

    private static IEnumerable<(T Element, int Depth)> FlattenWithDepthIterator<T>(IEnumerable<T> source, Func<T, IEnumerable<T>?> childrenSelector)
    {
        Queue<(T Element, int Depth)> queue = new();

        foreach (T item in source)
        {
            queue.Enqueue((item, 0));
        }

        while (queue.Count > 0)
        {
            (T current, int depth) = queue.Dequeue();
            yield return (current, depth);

            IEnumerable<T>? children = childrenSelector(current);

            if (children is not null)
            {
                foreach (T child in children)
                {
                    queue.Enqueue((child, depth + 1));
                }
            }
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Projects each element of a sequence to an <see cref="IEnumerable{T}"/> using a selector that receives the element
    ///and its zero-based index, then flattens the resulting sequences.
    ///</summary>
    ///<typeparam name="T">The source element type.</typeparam>
    ///<typeparam name="TResult">The result element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="selector">A function that produces a sequence from each element and its index.</param>
    ///<returns>A flat sequence of all projected results.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public static IEnumerable<TResult> FlatSelect<T, TResult>(this IEnumerable<T> source, Func<T, int, IEnumerable<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return source.SelectMany(selector);
    }

    ///<summary>
    ///Flattens a sequence of sequences into a single sequence by concatenating all inner sequences.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">A sequence of sequences to flatten.</param>
    ///<returns>A single flattened sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.SelectMany(inner => inner);
    }

    ///<summary>
    ///Flattens a sequence of arrays into a single sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">A sequence of arrays to flatten.</param>
    ///<returns>A single flattened sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T[]> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.SelectMany(inner => inner);
    }

    ///<summary>
    ///Flattens a sequence of lists into a single sequence.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">A sequence of lists to flatten.</param>
    ///<returns>A single flattened sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<List<T>> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.SelectMany(inner => inner);
    }

    ///<summary>
    ///Ungroups an <see cref="IOrderedEnumerable{TElement}"/> of groupings into a flat ordered sequence, preserving the
    ///order of groups. Within each group the original element order is preserved.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="source">An ordered sequence of groupings.</param>
    ///<returns>A flat sequence of elements in group order.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<TElement> FlattenOrdered<TKey, TElement>(this IOrderedEnumerable<IGrouping<TKey, TElement>> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.SelectMany(g => g);
    }

    ///<summary>
    ///Recursively flattens a hierarchical structure by applying <paramref name="childrenSelector"/> to retrieve
    ///children at each level. Uses breadth-first traversal.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The root-level elements.</param>
    ///<param name="childrenSelector">A function that returns children of an element.</param>
    ///<returns>A flat, breadth-first sequence of all elements in the hierarchy.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="childrenSelector"/> is <c>null</c>.</exception>
    public static IEnumerable<T> FlattenRecursive<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>?> childrenSelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(childrenSelector, nameof(childrenSelector));
        return FlattenRecursiveBreadthFirst(source, childrenSelector);
    }

    ///<summary>
    ///Recursively flattens a hierarchical structure using depth-first traversal.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The root-level elements.</param>
    ///<param name="childrenSelector">A function that returns children of an element.</param>
    ///<returns>A flat, depth-first sequence of all elements in the hierarchy.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="childrenSelector"/> is <c>null</c>.</exception>
    public static IEnumerable<T> FlattenRecursiveDepthFirst<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>?> childrenSelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(childrenSelector, nameof(childrenSelector));
        return FlattenDepthFirstIterator(source, childrenSelector);
    }

    ///<summary>
    ///Recursively flattens a hierarchical structure, returning each element paired with its depth level (zero-based).
    ///Uses breadth-first traversal.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The root-level elements (depth 0).</param>
    ///<param name="childrenSelector">A function that returns children of an element.</param>
    ///<returns>A flat sequence of <c>(Element, Depth)</c> tuples.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="childrenSelector"/> is <c>null</c>.</exception>
    public static IEnumerable<(T Element, int Depth)> FlattenWithDepth<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>?> childrenSelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(childrenSelector, nameof(childrenSelector));
        return FlattenWithDepthIterator(source, childrenSelector);
    }

    ///<summary>
    ///Ungroups a sequence of <see cref="IGrouping{TKey, TElement}"/> into a flat sequence of elements, discarding the
    ///keys.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="source">The grouped sequence.</param>
    ///<returns>A flat sequence of all elements across all groups.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<TElement> Ungroup<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.SelectMany(g => g);
    }

    ///<summary>
    ///Ungroups a sequence of <see cref="IGrouping{TKey, TElement}"/> into a flat sequence by projecting each element
    ///together with its group key.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="source">The grouped sequence.</param>
    ///<param name="resultSelector">A function that combines the group key and an element into a result.</param>
    ///<returns>A flat sequence of projected results.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="resultSelector"/> is <c>null</c>.</exception>
    public static IEnumerable<TResult> Ungroup<TKey, TElement, TResult>(this IEnumerable<IGrouping<TKey, TElement>> source, Func<TKey, TElement, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(resultSelector, nameof(resultSelector));
        return source.SelectMany(g => g.Select(e => resultSelector(g.Key, e)));
    }

    ///<summary>
    ///Ungroups a sequence of <see cref="IGrouping{TKey, TElement}"/> into a flat sequence of key-element tuples.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<param name="source">The grouped sequence.</param>
    ///<returns>A flat sequence of <c>(Key, Element)</c> tuples.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<(TKey Key, TElement Element)> UngroupToTuples<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return source.SelectMany(g => g.Select(e => (g.Key, e)));
    }
    #endregion
}
