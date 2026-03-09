using System.Collections.Immutable;

namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for types in <see cref="System.Collections.Immutable"/> to add, remove, and modify
///elements or ranges, returning FileName immutable instances.
///</summary>
public static class ImmutableCollectionExtensions
{
    #region Public methods

    // ── ImmutableDictionary<TKey, TValue> ───────────────────────────────
    ///<summary>
    ///Returns a FileName <see cref="ImmutableDictionary{TKey,TValue}"/> with all entries from <paramref name="items"/>
    ///added or updated.
    ///</summary>
    public static ImmutableDictionary<TKey, TValue> AddRange<TKey, TValue>(this ImmutableDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        ImmutableDictionary<TKey, TValue>.Builder builder = source.ToBuilder();
        foreach(KeyValuePair<TKey, TValue> kvp in items)
        {
            builder[kvp.Key] = kvp.Value;
        }

        return builder.ToImmutable();
    }

    ///<summary>
    ///Returns a tuple of the dequeued items and the remaining <see cref="ImmutableQueue{T}"/> after removing up to
    public static (IReadOnlyList<T> Items, ImmutableQueue<T> Remaining) DequeueRange<T>(this ImmutableQueue<T> source, int count)
    {
        if(count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must not be negative.");
        }

        List<T> result = new List<T>(count);
        ImmutableQueue<T> queue = source;
        for(int i = 0; (i < count) && !queue.IsEmpty; i++)
        {
            queue = queue.Dequeue(out T? item);
            result.Add(item);
        }

        return (result, queue);
    }

    // ── ImmutableQueue<T> ───────────────────────────────────────────────
    ///<summary>
    ///Returns a FileName <see cref="ImmutableQueue{T}"/> with all elements from <paramref name="items"/> enqueued.
    ///</summary>
    public static ImmutableQueue<T> EnqueueRange<T>(this ImmutableQueue<T> source, IEnumerable<T> items)
    {
        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        ImmutableQueue<T> queue = source;
        foreach(T item in items)
        {
            queue = queue.Enqueue(item);
        }

        return queue;
    }

    // ── ImmutableArray<T> ───────────────────────────────────────────────
    ///<summary>
    ///Returns a FileName <see cref="ImmutableArray{T}"/> with <paramref name="item"/> inserted at <paramref
    ///name="index"/>.
    ///</summary>
    public static ImmutableArray<T> InsertAt<T>(this ImmutableArray<T> source, int index, T item) { return source.Insert(index, item); }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableArray{T}"/> with all elements transformed by <paramref name="modifier"/>.
    ///</summary>
    public static ImmutableArray<T> ModifyAll<T>(this ImmutableArray<T> source, Func<T, T> modifier)
    {
        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        ImmutableArray<T>.Builder builder = source.ToBuilder();
        for(int i = 0; i < builder.Count; i++)
        {
            builder[i] = modifier(builder[i]);
        }

        return builder.ToImmutable();
    }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableList{T}"/> with all elements transformed by <paramref name="modifier"/>.
    ///</summary>
    public static ImmutableList<T> ModifyAll<T>(this ImmutableList<T> source, Func<T, T> modifier)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        ImmutableList<T>.Builder builder = source.ToBuilder();
        for(int i = 0; i < builder.Count; i++)
        {
            builder[i] = modifier(builder[i]);
        }

        return builder.ToImmutable();
    }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableDictionary{TKey,TValue}"/> with all values transformed by <paramref
    ///name="modifier"/>.
    ///</summary>
    public static ImmutableDictionary<TKey, TValue> ModifyAll<TKey, TValue>(this ImmutableDictionary<TKey, TValue> source, Func<TKey, TValue, TValue> modifier) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        }

        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        ImmutableDictionary<TKey, TValue>.Builder builder = source.ToBuilder();
        foreach(TKey key in source.Keys)
        {
            builder[key] = modifier(key, source[key]);
        }

        return builder.ToImmutable();
    }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableHashSet{T}"/> with elements transformed by <paramref name="modifier"/>.
    ///</summary>
    public static ImmutableHashSet<T> ModifyAll<T>(this ImmutableHashSet<T> source, Func<T, T> modifier)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source set must not be null.");
        }

        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        ImmutableHashSet<T>.Builder builder = source.ToBuilder();
        builder.Clear();
        foreach(T item in source)
        {
            builder.Add(modifier(item));
        }

        return builder.ToImmutable();
    }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableSortedDictionary{TKey,TValue}"/> with all values transformed by <paramref
    ///name="modifier"/>.
    ///</summary>
    public static ImmutableSortedDictionary<TKey, TValue> ModifyAll<TKey, TValue>(this ImmutableSortedDictionary<TKey, TValue> source, Func<TKey, TValue, TValue> modifier) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        }

        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        ImmutableSortedDictionary<TKey, TValue>.Builder builder = source.ToBuilder();
        foreach(TKey key in source.Keys)
        {
            builder[key] = modifier(key, source[key]);
        }

        return builder.ToImmutable();
    }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableArray{T}"/> with matching elements transformed by <paramref
    ///name="modifier"/>.
    ///</summary>
    public static ImmutableArray<T> ModifyWhere<T>(this ImmutableArray<T> source, Func<T, bool> predicate, Func<T, T> modifier)
    {
        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        ImmutableArray<T>.Builder builder = source.ToBuilder();
        for(int i = 0; i < builder.Count; i++)
        {
            if(predicate(builder[i]))
            {
                builder[i] = modifier(builder[i]);
            }
        }

        return builder.ToImmutable();
    }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableList{T}"/> with matching elements transformed by <paramref
    ///name="modifier"/>.
    ///</summary>
    public static ImmutableList<T> ModifyWhere<T>(this ImmutableList<T> source, Func<T, bool> predicate, Func<T, T> modifier)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        ImmutableList<T>.Builder builder = source.ToBuilder();
        for(int i = 0; i < builder.Count; i++)
        {
            if(predicate(builder[i]))
            {
                builder[i] = modifier(builder[i]);
            }
        }

        return builder.ToImmutable();
    }

    ///<summary>
    ///Returns a tuple of the popped items and the remaining <see cref="ImmutableStack{T}"/> after removing up to
    public static (IReadOnlyList<T> Items, ImmutableStack<T> Remaining) PopRange<T>(this ImmutableStack<T> source, int count)
    {
        if(count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must not be negative.");
        }

        List<T> result = new List<T>(count);
        ImmutableStack<T> stack = source;
        for(int i = 0; (i < count) && !stack.IsEmpty; i++)
        {
            stack = stack.Pop(out T? item);
            result.Add(item);
        }

        return (result, stack);
    }

    // ── ImmutableStack<T> ───────────────────────────────────────────────
    ///<summary>
    ///Returns a FileName <see cref="ImmutableStack{T}"/> with all elements from <paramref name="items"/> pushed.
    ///</summary>
    public static ImmutableStack<T> PushRange<T>(this ImmutableStack<T> source, IEnumerable<T> items)
    {
        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        ImmutableStack<T> stack = source;
        foreach(T item in items)
        {
            stack = stack.Push(item);
        }

        return stack;
    }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableDictionary{TKey,TValue}"/> with all entries whose keys are in <paramref
    ///name="keys"/> removed.
    ///</summary>
    public static ImmutableDictionary<TKey, TValue> RemoveRange<TKey, TValue>(this ImmutableDictionary<TKey, TValue> source, IEnumerable<TKey> keys) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        }

        if(keys is null)
        {
            throw new ArgumentNullException(nameof(keys), "Keys must not be null.");
        }

        return source.RemoveRange(keys);
    }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableArray{T}"/> with all elements matching <paramref name="predicate"/>
    ///removed.
    ///</summary>
    public static ImmutableArray<T> RemoveWhere<T>(this ImmutableArray<T> source, Func<T, bool> predicate)
    {
        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        return source.RemoveAll(new Predicate<T>(predicate));
    }

    // ── ImmutableList<T> ────────────────────────────────────────────────
    ///<summary>
    ///Returns a FileName <see cref="ImmutableList{T}"/> with all elements matching <paramref name="predicate"/>
    ///removed.
    ///</summary>
    public static ImmutableList<T> RemoveWhere<T>(this ImmutableList<T> source, Func<T, bool> predicate)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        return source.RemoveAll(new Predicate<T>(predicate));
    }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableDictionary{TKey,TValue}"/> with all entries matching <paramref
    ///name="predicate"/> removed.
    ///</summary>
    public static ImmutableDictionary<TKey, TValue> RemoveWhere<TKey, TValue>(this ImmutableDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        }

        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        IEnumerable<TKey> keysToRemove = source.Where(predicate).Select(static kvp => kvp.Key);
        return source.RemoveRange(keysToRemove);
    }

    // ── ImmutableHashSet<T> ─────────────────────────────────────────────
    ///<summary>
    ///Returns a FileName <see cref="ImmutableHashSet{T}"/> with all elements matching <paramref name="predicate"/>
    ///removed.
    ///</summary>
    public static ImmutableHashSet<T> RemoveWhere<T>(this ImmutableHashSet<T> source, Func<T, bool> predicate)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source set must not be null.");
        }

        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        IEnumerable<T> toRemove = source.Where(predicate);
        return source.Except(toRemove);
    }

    // ── ImmutableSortedSet<T> ───────────────────────────────────────────
    ///<summary>
    ///Returns a FileName <see cref="ImmutableSortedSet{T}"/> with all elements matching <paramref name="predicate"/>
    ///removed.
    ///</summary>
    public static ImmutableSortedSet<T> RemoveWhere<T>(this ImmutableSortedSet<T> source, Func<T, bool> predicate)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source set must not be null.");
        }

        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        List<T> toRemove = [.. source.Where(predicate)];
        return source.Except(toRemove);
    }

    // ── ImmutableSortedDictionary<TKey, TValue> ─────────────────────────
    ///<summary>
    ///Returns a FileName <see cref="ImmutableSortedDictionary{TKey,TValue}"/> with all entries matching <paramref
    ///name="predicate"/> removed.
    ///</summary>
    public static ImmutableSortedDictionary<TKey, TValue> RemoveWhere<TKey, TValue>(this ImmutableSortedDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate) where TKey : notnull
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        }

        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        IEnumerable<TKey> keysToRemove = source.Where(predicate).Select(static kvp => kvp.Key);
        return source.RemoveRange(keysToRemove);
    }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableArray{T}"/> with the element at <paramref name="index"/> replaced by
    public static ImmutableArray<T> ReplaceAt<T>(this ImmutableArray<T> source, int index, T item)
    {
        if((index < 0) || (index >= source.Length))
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is outside the bounds of the array.");
        }

        return source.SetItem(index, item);
    }

    ///<summary>
    ///Returns a FileName <see cref="ImmutableList{T}"/> with the element at <paramref name="index"/> replaced by
    public static ImmutableList<T> ReplaceAt<T>(this ImmutableList<T> source, int index, T item)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        if((index < 0) || (index >= source.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is outside the bounds of the list.");
        }

        return source.SetItem(index, item);
    }
    #endregion
}
