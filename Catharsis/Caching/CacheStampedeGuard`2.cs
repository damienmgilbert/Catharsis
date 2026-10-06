using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Catharsis.Concurrency;

namespace Catharsis.Caching;

///<summary>
///Wraps a cache with single-flight-protected fills: on a miss, concurrent callers for the same key share one
///underlying fetch instead of each triggering their own, preventing a thundering herd of duplicate work when a
///popular key is absent or has just been invalidated.
///</summary>
///<typeparam name="TKey">The type of the cache keys.</typeparam>
///<typeparam name="TValue">The type of the cached values.</typeparam>
///<example>
///<code>
///CacheStampedeGuard&lt;string, Product&gt; cache = new();
///
///// If 1000 requests for the same productId arrive while the cache is cold, only one database call is made.
///Product product = await cache.GetOrAddAsync(productId, () => database.LoadProductAsync(productId));
///</code>
///</example>
///<param name="comparer">The equality comparer used to match keys, or <c>null</c> to use the default comparer.</param>
public sealed class CacheStampedeGuard<TKey, TValue>(IEqualityComparer<TKey>? comparer = null) where TKey : notnull
{
    #region Fields
    readonly ConcurrentDictionary<TKey, TValue> _cache = new(comparer);
    readonly SingleFlightExecutor<TKey, TValue> _singleFlight = new(comparer);
    #endregion

    #region Public methods

    ///<summary>
    ///Removes all entries from the cache.
    ///</summary>
    public void Clear() => _cache.Clear();

    ///<summary>
    ///Gets the cached value for the specified key, or fetches and caches it via <paramref name="valueFactory"/> on a
    ///miss. Concurrent misses for the same key share a single call to <paramref name="valueFactory"/>.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="valueFactory">The async factory that fetches the value when the key is absent.</param>
    ///<returns>The cached or newly fetched value.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="valueFactory"/> is <c>null</c>.</exception>
    public async Task<TValue> GetOrAddAsync(TKey key, Func<Task<TValue>> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        if (_cache.TryGetValue(key, out TValue? cached))
        {
            return cached;
        }

        return await _singleFlight.ExecuteAsync(key, async () =>
        {
            if (_cache.TryGetValue(key, out TValue? existing))
            {
                return existing;
            }

            TValue fresh = await valueFactory().ConfigureAwait(false);
            _cache[key] = fresh;
            return fresh;
        }).ConfigureAwait(false);
    }

    ///<summary>
    ///Removes the entry with the specified key, so the next lookup triggers a fresh fetch.
    ///</summary>
    ///<param name="key">The key to invalidate.</param>
    ///<returns><c>true</c> if the entry was found and removed; otherwise <c>false</c>.</returns>
    public bool Invalidate(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _cache.TryRemove(key, out _);
    }

    ///<summary>
    ///Attempts to retrieve the already-cached value for the specified key, without fetching it.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="value">The cached value, if present.</param>
    ///<returns><c>true</c> if the key was found; otherwise <c>false</c>.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _cache.TryGetValue(key, out value);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the current number of cached entries.
    ///</summary>
    public int Count => _cache.Count;
    #endregion
}
