using System.Collections.Concurrent;

namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for types in <see cref="System.Collections.Concurrent"/> to add, remove, and modify
///elements or ranges of elements.
///</summary>
public static class ConcurrentCollectionExtensions
{
    #region Public methods

    // ── ConcurrentBag<T> ────────────────────────────────────────────────
    ///<summary>
    ///Adds all elements from <paramref name="items"/> to the <see cref="ConcurrentBag{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target bag.</param>
    ///<param name="items">The items to add.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static ConcurrentBag<T> AddRange<T>(this ConcurrentBag<T> source, IEnumerable<T> items)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source bag must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        foreach(T item in items)
        {
            source.Add(item);
        }

        return source;
    }

    // ── ConcurrentDictionary<TKey, TValue> ──────────────────────────────
    ///<summary>
    ///Adds all key-value pairs from <paramref name="items"/> to the <see cref="ConcurrentDictionary{TKey,TValue}"/>.
    ///Existing keys are updated with the FileName values.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target concurrent dictionary.</param>
    ///<param name="items">The key-value pairs to add or update.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static ConcurrentDictionary<TKey, TValue> AddRange<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        foreach(KeyValuePair<TKey, TValue> kvp in items)
        {
            source.AddOrUpdate(kvp.Key, kvp.Value, (_, _) => kvp.Value);
        }

        return source;
    }

    // ── BlockingCollection<T> ───────────────────────────────────────────
    ///<summary>
    ///Adds all elements from <paramref name="items"/> to the <see cref="BlockingCollection{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target blocking collection.</param>
    ///<param name="items">The items to add.</param>
    ///<param name="cancellationToken">An optional cancellation token.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static BlockingCollection<T> AddRange<T>(this BlockingCollection<T> source, IEnumerable<T> items, CancellationToken cancellationToken = default)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source collection must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        foreach(T item in items)
        {
            source.Add(item, cancellationToken);
        }

        return source;
    }

    ///<summary>
    ///Dequeues up to <paramref name="count"/> elements from the <see cref="ConcurrentQueue{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target queue.</param>
    ///<param name="count">The maximum number of elements to dequeue.</param>
    ///<returns>A list of dequeued elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static List<T> DequeueRange<T>(this ConcurrentQueue<T> source, int count)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source queue must not be null.");
        }

        if(count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must not be negative.");
        }

        List<T> result = new List<T>(count);
        for(int i = 0; (i < count) && source.TryDequeue(out T? item); i++)
        {
            result.Add(item);
        }

        return result;
    }

    // ── ConcurrentQueue<T> ──────────────────────────────────────────────
    ///<summary>
    ///Enqueues all elements from <paramref name="items"/> into the <see cref="ConcurrentQueue{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target queue.</param>
    ///<param name="items">The items to enqueue.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static ConcurrentQueue<T> EnqueueRange<T>(this ConcurrentQueue<T> source, IEnumerable<T> items)
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

    ///<summary>
    ///Modifies all values in the <see cref="ConcurrentDictionary{TKey,TValue}"/> by applying <paramref
    ///name="modifier"/>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target concurrent dictionary.</param>
    ///<param name="modifier">A function that produces a FileName value given the key and current value.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="modifier"/> is <c>null</c>.</exception>
    public static ConcurrentDictionary<TKey, TValue> ModifyAll<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source, Func<TKey, TValue, TValue> modifier) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        }

        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        foreach(TKey key in source.Keys.ToList())
        {
            source.AddOrUpdate(key, k => modifier(k, default!), (k, v) => modifier(k, v));
        }

        return source;
    }

    ///<summary>
    ///Pops up to <paramref name="count"/> elements from the <see cref="ConcurrentStack{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target stack.</param>
    ///<param name="count">The maximum number of elements to pop.</param>
    ///<returns>A list of popped elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static List<T> PopRange<T>(this ConcurrentStack<T> source, int count)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source stack must not be null.");
        }

        if(count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must not be negative.");
        }

        T[] buffer = new T[count];
        int popped = source.TryPopRange(buffer);
        return new List<T>(buffer[..popped]);
    }

    // ── ConcurrentStack<T> ──────────────────────────────────────────────
    ///<summary>
    ///Pushes all elements from <paramref name="items"/> onto the <see cref="ConcurrentStack{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target stack.</param>
    ///<param name="items">The items to push.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static ConcurrentStack<T> PushRange<T>(this ConcurrentStack<T> source, IEnumerable<T> items)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source stack must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        T[] array = items as T[] ?? [.. items];
        source.PushRange(array);
        return source;
    }

    ///<summary>
    ///Removes all entries with keys in <paramref name="keys"/> from the <see
    ///cref="ConcurrentDictionary{TKey,TValue}"/>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target concurrent dictionary.</param>
    ///<param name="keys">The keys to remove.</param>
    ///<returns>The number of entries removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keys"/> is <c>null</c>.</exception>
    public static int RemoveRange<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source, IEnumerable<TKey> keys) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        }

        if(keys is null)
        {
            throw new ArgumentNullException(nameof(keys), "Keys must not be null.");
        }

        int removed = 0;
        foreach(TKey key in keys)
        {
            if(source.TryRemove(key, out _))
            {
                removed++;
            }
        }

        return removed;
    }

    ///<summary>
    ///Removes all entries matching <paramref name="predicate"/> from the <see
    ///cref="ConcurrentDictionary{TKey,TValue}"/>.
    ///</summary>
    ///<typeparam name="TKey">The key type.</typeparam>
    ///<typeparam name="TValue">The value type.</typeparam>
    ///<param name="source">The target concurrent dictionary.</param>
    ///<param name="predicate">A function that returns <c>true</c> for entries to remove.</param>
    ///<returns>The number of entries removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static int RemoveWhere<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        }

        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        List<TKey> keysToRemove = [.. source.Where(predicate).Select(static kvp => kvp.Key)];
        int removed = 0;
        foreach(TKey key in keysToRemove)
        {
            if(source.TryRemove(key, out _))
            {
                removed++;
            }
        }

        return removed;
    }

    ///<summary>
    ///Takes up to <paramref name="count"/> elements from the <see cref="BlockingCollection{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target blocking collection.</param>
    ///<param name="count">The maximum number of elements to take.</param>
    ///<returns>A list of taken elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static List<T> TakeRange<T>(this BlockingCollection<T> source, int count)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source collection must not be null.");
        }

        if(count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must not be negative.");
        }

        List<T> result = new List<T>(count);
        for(int i = 0; (i < count) && source.TryTake(out T? item); i++)
        {
            result.Add(item);
        }

        return result;
    }
    #endregion
}
