namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for <see cref="Queue{T}"/>, <see cref="Stack{T}"/>, <see cref="PriorityQueue{TElement,
///TPriority}"/>, and <see cref="SortedList{TKey, TValue}"/> to add, remove, and modify elements or ranges of elements.
///</summary>
public static class QueueStackExtensions
{
    #region Public methods

    // ── SortedList<TKey, TValue> ────────────────────────────────────────
    ///<summary>
    ///Adds all key-value pairs from <paramref name="items"/> to the sorted list.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target sorted list.</param>
    ///<param name="items">The key-value pairs to add.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static SortedList<TKey, TValue> AddRange<TKey, TValue>(this SortedList<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source sorted list must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        foreach(KeyValuePair<TKey, TValue> kvp in items)
        {
            source[kvp.Key] = kvp.Value;
        }

        return source;
    }

    // ── SortedDictionary<TKey, TValue> ──────────────────────────────────
    ///<summary>
    ///Adds all key-value pairs from <paramref name="items"/> to the sorted dictionary. Existing keys are overwritten.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target sorted dictionary.</param>
    ///<param name="items">The key-value pairs to add.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static SortedDictionary<TKey, TValue> AddRange<TKey, TValue>(this SortedDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source sorted dictionary must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        foreach(KeyValuePair<TKey, TValue> kvp in items)
        {
            source[kvp.Key] = kvp.Value;
        }

        return source;
    }

    ///<summary>
    ///Dequeues up to <paramref name="count"/> elements from the queue.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target queue.</param>
    ///<param name="count">The maximum number of elements to dequeue.</param>
    ///<returns>A list of dequeued elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static List<T> DequeueRange<T>(this Queue<T> source, int count)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source queue must not be null.");
        }

        if(count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must not be negative.");
        }

        List<T> result = [with(Math.Min(count, source.Count))];
        for(int i = 0; (i < count) && (source.Count > 0); i++)
        {
            result.Add(source.Dequeue());
        }

        return result;
    }

    ///<summary>
    ///Dequeues up to <paramref name="count"/> elements from the priority queue.
    ///</summary>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<typeparam name="TPriority">The priority type.</typeparam>
    ///<param name="source">The target priority queue.</param>
    ///<param name="count">The maximum number of elements to dequeue.</param>
    ///<returns>A list of dequeued elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static List<TElement> DequeueRange<TElement, TPriority>(this PriorityQueue<TElement, TPriority> source, int count)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source priority queue must not be null.");
        }

        if(count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must not be negative.");
        }

        List<TElement> result = [with(Math.Min(count, source.Count))];
        for(int i = 0; (i < count) && (source.Count > 0); i++)
        {
            result.Add(source.Dequeue());
        }

        return result;
    }

    // ── Queue<T> ────────────────────────────────────────────────────────
    ///<summary>
    ///Enqueues all elements from <paramref name="items"/> into the queue.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target queue.</param>
    ///<param name="items">The items to enqueue.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static Queue<T> EnqueueRange<T>(this Queue<T> source, IEnumerable<T> items)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source queue must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        foreach(T item in items)
        {
            source.Enqueue(item);
        }

        return source;
    }

    // ── PriorityQueue<TElement, TPriority> ──────────────────────────────
    ///<summary>
    ///Enqueues all elements from <paramref name="items"/> into the priority queue.
    ///</summary>
    ///<typeparam name="TElement">The element type.</typeparam>
    ///<typeparam name="TPriority">The priority type.</typeparam>
    ///<param name="source">The target priority queue.</param>
    ///<param name="items">The element-priority pairs to enqueue.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static PriorityQueue<TElement, TPriority> EnqueueRange<TElement, TPriority>(this PriorityQueue<TElement, TPriority> source, IEnumerable<(TElement Element, TPriority Priority)> items)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source priority queue must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        foreach (var (element, priority) in items)
            source.Enqueue(element, priority);

        return source;
    }

    ///<summary>
    ///Modifies all values in the sorted list by applying <paramref name="modifier"/>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target sorted list.</param>
    ///<param name="modifier">A function that produces a new value given the key and current value.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="modifier"/> is <c>null</c>.</exception>
    public static SortedList<TKey, TValue> ModifyAll<TKey, TValue>(this SortedList<TKey, TValue> source, Func<TKey, TValue, TValue> modifier) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source sorted list must not be null.");
        }

        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        List<TKey> keys = [.. source.Keys];
        foreach(TKey key in keys)
        {
            source[key] = modifier(key, source[key]);
        }

        return source;
    }

    ///<summary>
    ///Modifies all values in the sorted dictionary by applying <paramref name="modifier"/>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target sorted dictionary.</param>
    ///<param name="modifier">A function that produces a new value given the key and current value.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="modifier"/> is <c>null</c>.</exception>
    public static SortedDictionary<TKey, TValue> ModifyAll<TKey, TValue>(this SortedDictionary<TKey, TValue> source, Func<TKey, TValue, TValue> modifier) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source sorted dictionary must not be null.");
        }

        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        List<TKey> keys = [.. source.Keys];
        foreach(TKey key in keys)
        {
            source[key] = modifier(key, source[key]);
        }

        return source;
    }

    ///<summary>
    ///Pops up to <paramref name="count"/> elements from the stack.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target stack.</param>
    ///<param name="count">The maximum number of elements to pop.</param>
    ///<returns>A list of popped elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static List<T> PopRange<T>(this Stack<T> source, int count)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source stack must not be null.");
        }

        if(count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must not be negative.");
        }

        List<T> result = [with(Math.Min(count, source.Count))];
        for(int i = 0; (i < count) && (source.Count > 0); i++)
        {
            result.Add(source.Pop());
        }

        return result;
    }

    // ── Stack<T> ────────────────────────────────────────────────────────
    ///<summary>
    ///Pushes all elements from <paramref name="items"/> onto the stack.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target stack.</param>
    ///<param name="items">The items to push.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static Stack<T> PushRange<T>(this Stack<T> source, IEnumerable<T> items)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source stack must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        foreach(T item in items)
        {
            source.Push(item);
        }

        return source;
    }

    ///<summary>
    ///Removes all entries with keys in <paramref name="keys"/> from the sorted list.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target sorted list.</param>
    ///<param name="keys">The keys to remove.</param>
    ///<returns>The number of entries removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keys"/> is <c>null</c>.</exception>
    public static int RemoveRange<TKey, TValue>(this SortedList<TKey, TValue> source, IEnumerable<TKey> keys) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source sorted list must not be null.");
        }

        if(keys is null)
        {
            throw new ArgumentNullException(nameof(keys), "Keys must not be null.");
        }

        int removed = 0;
        foreach(TKey key in keys)
        {
            if(source.Remove(key))
            {
                removed++;
            }
        }

        return removed;
    }

    ///<summary>
    ///Removes all entries with keys in <paramref name="keys"/> from the sorted dictionary.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target sorted dictionary.</param>
    ///<param name="keys">The keys to remove.</param>
    ///<returns>The number of entries removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keys"/> is <c>null</c>.</exception>
    public static int RemoveRange<TKey, TValue>(this SortedDictionary<TKey, TValue> source, IEnumerable<TKey> keys) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source sorted dictionary must not be null.");
        }

        if(keys is null)
        {
            throw new ArgumentNullException(nameof(keys), "Keys must not be null.");
        }

        int removed = 0;
        foreach(TKey key in keys)
        {
            if(source.Remove(key))
            {
                removed++;
            }
        }

        return removed;
    }

    ///<summary>
    ///Removes all entries matching <paramref name="predicate"/> from the sorted list.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target sorted list.</param>
    ///<param name="predicate">A function that returns <c>true</c> for entries to remove.</param>
    ///<returns>The number of entries removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static int RemoveWhere<TKey, TValue>(this SortedList<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source sorted list must not be null.");
        }

        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        List<TKey> keysToRemove = [.. source.Where(predicate).Select(static kvp => kvp.Key)];
        int removed = 0;
        foreach(TKey key in keysToRemove)
        {
            if(source.Remove(key))
            {
                removed++;
            }
        }

        return removed;
    }

    ///<summary>
    ///Removes all entries matching <paramref name="predicate"/> from the sorted dictionary.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target sorted dictionary.</param>
    ///<param name="predicate">A function that returns <c>true</c> for entries to remove.</param>
    ///<returns>The number of entries removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static int RemoveWhere<TKey, TValue>(this SortedDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source sorted dictionary must not be null.");
        }

        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        List<TKey> keysToRemove = [.. source.Where(predicate).Select(static kvp => kvp.Key)];
        int removed = 0;
        foreach(TKey key in keysToRemove)
        {
            if(source.Remove(key))
            {
                removed++;
            }
        }

        return removed;
    }
    #endregion
}
