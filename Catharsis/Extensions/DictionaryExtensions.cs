namespace Catharsis.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IDictionary{TKey,TValue}"/> to
/// add, remove, and modify individual entries or ranges of entries.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Adds or updates the entry for <paramref name="key"/>.
    /// If the key exists, the value is replaced; otherwise the key-value pair is added.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="source">The target dictionary.</param>
    /// <param name="key">The key to add or update.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>The original <paramref name="source"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");

        source[key] = value;
        return source;
    }

    /// <summary>
    /// Adds or updates the entry for <paramref name="key"/> using a factory for new values and an updater for existing values.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="source">The target dictionary.</param>
    /// <param name="key">The key to add or update.</param>
    /// <param name="addFactory">A function that produces the value when the key does not exist.</param>
    /// <param name="updateFactory">A function that produces the updated value given the existing value when the key already exists.</param>
    /// <returns>The new or updated value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="addFactory"/>, or <paramref name="updateFactory"/> is <c>null</c>.</exception>
    public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TKey, TValue> addFactory, Func<TKey, TValue, TValue> updateFactory)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        if (addFactory is null) throw new ArgumentNullException(nameof(addFactory), "Add factory must not be null.");
        if (updateFactory is null) throw new ArgumentNullException(nameof(updateFactory), "Update factory must not be null.");

        if (source.TryGetValue(key, out var existing))
        {
            var updated = updateFactory(key, existing);
            source[key] = updated;
            return updated;
        }
        else
        {
            var added = addFactory(key);
            source[key] = added;
            return added;
        }
    }

    /// <summary>
    /// Adds all key-value pairs from <paramref name="items"/> to the dictionary.
    /// Existing keys are overwritten.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="source">The target dictionary.</param>
    /// <param name="items">The key-value pairs to add.</param>
    /// <returns>The original <paramref name="source"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        if (items is null) throw new ArgumentNullException(nameof(items), "Items must not be null.");

        foreach (var kvp in items)
            source[kvp.Key] = kvp.Value;

        return source;
    }

    /// <summary>
    /// Removes all entries with keys in <paramref name="keys"/> from the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="source">The target dictionary.</param>
    /// <param name="keys">The keys to remove.</param>
    /// <returns>The number of entries removed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keys"/> is <c>null</c>.</exception>
    public static int RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<TKey> keys)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        if (keys is null) throw new ArgumentNullException(nameof(keys), "Keys must not be null.");

        int removed = 0;
        foreach (var key in keys)
        {
            if (source.Remove(key))
                removed++;
        }

        return removed;
    }

    /// <summary>
    /// Removes all entries matching <paramref name="predicate"/> from the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="source">The target dictionary.</param>
    /// <param name="predicate">A function that returns <c>true</c> for entries to remove.</param>
    /// <returns>The number of entries removed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static int RemoveWhere<TKey, TValue>(this IDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        if (predicate is null) throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");

        var keysToRemove = source.Where(predicate).Select(kvp => kvp.Key).ToList();
        int removed = 0;
        foreach (var key in keysToRemove)
        {
            if (source.Remove(key))
                removed++;
        }

        return removed;
    }

    /// <summary>
    /// Replaces the value for <paramref name="key"/> if it exists.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="source">The target dictionary.</param>
    /// <param name="key">The key whose value to replace.</param>
    /// <param name="newValue">The new value.</param>
    /// <returns><c>true</c> if the key was found and replaced; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static bool ReplaceValue<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue newValue)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");

        if (!source.ContainsKey(key))
            return false;

        source[key] = newValue;
        return true;
    }

    /// <summary>
    /// Modifies all values in the dictionary by applying <paramref name="modifier"/> to each entry.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="source">The target dictionary.</param>
    /// <param name="modifier">A function that transforms each value given its key and current value.</param>
    /// <returns>The original <paramref name="source"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="modifier"/> is <c>null</c>.</exception>
    public static IDictionary<TKey, TValue> ModifyAll<TKey, TValue>(this IDictionary<TKey, TValue> source, Func<TKey, TValue, TValue> modifier)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        if (modifier is null) throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");

        var keys = source.Keys.ToList();
        foreach (var key in keys)
            source[key] = modifier(key, source[key]);

        return source;
    }

    /// <summary>
    /// Modifies values of entries matching <paramref name="predicate"/> by applying <paramref name="modifier"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="source">The target dictionary.</param>
    /// <param name="predicate">A function that returns <c>true</c> for entries to modify.</param>
    /// <param name="modifier">A function that transforms matching entry values.</param>
    /// <returns>The original <paramref name="source"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="predicate"/>, or <paramref name="modifier"/> is <c>null</c>.</exception>
    public static IDictionary<TKey, TValue> ModifyWhere<TKey, TValue>(this IDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate, Func<TKey, TValue, TValue> modifier)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        if (predicate is null) throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        if (modifier is null) throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");

        var keysToModify = source.Where(predicate).Select(kvp => kvp.Key).ToList();
        foreach (var key in keysToModify)
            source[key] = modifier(key, source[key]);

        return source;
    }

    /// <summary>
    /// Gets the value for <paramref name="key"/> if it exists; otherwise adds and returns the value produced by <paramref name="factory"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="source">The target dictionary.</param>
    /// <param name="key">The key to look up or add.</param>
    /// <param name="factory">A function that produces the value when the key does not exist.</param>
    /// <returns>The existing or newly added value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="factory"/> is <c>null</c>.</exception>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TKey, TValue> factory)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source dictionary must not be null.");
        if (factory is null) throw new ArgumentNullException(nameof(factory), "Factory function must not be null.");

        if (source.TryGetValue(key, out var value))
            return value;

        value = factory(key);
        source[key] = value;
        return value;
    }
}
