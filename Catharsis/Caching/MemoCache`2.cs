using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Catharsis.Caching;

///<summary>
///A thread-safe memoization cache that computes a value for a key at most once, even under concurrent access, and
///reuses the cached result for subsequent lookups until the entry is removed or the cache is cleared. Unlike
///<see cref="Catharsis.DataStructures.LruCache{TKey, TValue}"/>, this cache is unbounded and centers on the atomic
///"get or compute" pattern rather than manual insertion with LRU eviction.
///</summary>
///<typeparam name="TKey">The type of the cache keys.</typeparam>
///<typeparam name="TValue">The type of the cached values.</typeparam>
///<example>
///<code>
///MemoCache&lt;string, Regex&gt; compiledPatterns = new();
///Regex regex = compiledPatterns.GetOrAdd(pattern, static p => new Regex(p));
///</code>
///</example>
///<param name="comparer">The equality comparer used to match keys, or <c>null</c> to use the default comparer.</param>
public sealed class MemoCache<TKey, TValue>(IEqualityComparer<TKey>? comparer = null) where TKey : notnull
{
    #region Fields
    readonly ConcurrentDictionary<TKey, Lazy<TValue>> _entries = new(comparer);
    #endregion

    #region Public methods
    ///<summary>
    ///Determines whether the cache contains the specified key.
    ///</summary>
    ///<param name="key">The key to look for.</param>
    ///<returns><c>true</c> if the key exists; otherwise <c>false</c>.</returns>
    public bool ContainsKey(TKey key) => _entries.ContainsKey(key);

    ///<summary>
    ///Removes all entries from the cache.
    ///</summary>
    public void Clear() => _entries.Clear();

    ///<summary>
    ///Gets the cached value for the specified key, computing and caching it via <paramref name="valueFactory"/> if
    ///absent. The factory runs at most once per key, even when called concurrently for the same key.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="valueFactory">The factory that computes the value when the key is absent.</param>
    ///<returns>The cached or newly computed value.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="valueFactory"/> is <c>null</c>.</exception>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        Lazy<TValue> lazy = _entries.GetOrAdd(key, static (k, factory) => new Lazy<TValue>(() => factory(k)), valueFactory);
        return lazy.Value;
    }

    ///<summary>
    ///Removes the entry with the specified key.
    ///</summary>
    ///<param name="key">The key to remove.</param>
    ///<returns><c>true</c> if the entry was found and removed; otherwise <c>false</c>.</returns>
    public bool TryRemove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _entries.TryRemove(key, out _);
    }

    ///<summary>
    ///Attempts to retrieve the already-cached value for the specified key, without computing it.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="value">The cached value, if present.</param>
    ///<returns><c>true</c> if the key was found; otherwise <c>false</c>.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_entries.TryGetValue(key, out Lazy<TValue>? lazy))
        {
            value = lazy.Value;
            return true;
        }

        value = default;
        return false;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the current number of entries in the cache.
    ///</summary>
    public int Count => _entries.Count;
    #endregion
}
