using System.Collections.Concurrent;

namespace Catharsis.Caching;

///<summary>
///A two-level cache: a fast in-memory layer in front of a pluggable <see cref="ICacheLayer{TKey, TValue}"/>, such as
///a distributed cache. Reads check the in-memory layer first and populate it from the backing layer on a miss;
///writes go to both layers.
///</summary>
///<typeparam name="TKey">The type of the cache keys.</typeparam>
///<typeparam name="TValue">The type of the cached values.</typeparam>
///<example>
///<code>
///LayeredCache&lt;string, Product&gt; cache = new(new RedisCacheLayer(connection));
///(bool found, Product? value) = await cache.TryGetAsync(productId);
///</code>
///</example>
public sealed class LayeredCache<TKey, TValue> where TKey : notnull
{
    #region Fields
    readonly ConcurrentDictionary<TKey, TValue> _local;
    readonly ICacheLayer<TKey, TValue> _backing;
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a layered cache in front of the specified backing layer, using the default equality comparer for
    ///<typeparamref name="TKey"/>.
    ///</summary>
    ///<param name="backing">The backing cache layer.</param>
    ///<exception cref="ArgumentNullException"><paramref name="backing"/> is <c>null</c>.</exception>
    public LayeredCache(ICacheLayer<TKey, TValue> backing) : this(backing, null)
    {
    }

    ///<summary>
    ///Creates a layered cache in front of the specified backing layer.
    ///</summary>
    ///<param name="backing">The backing cache layer.</param>
    ///<param name="comparer">The equality comparer used to match keys in the local layer, or <c>null</c> to use the default comparer.</param>
    ///<exception cref="ArgumentNullException"><paramref name="backing"/> is <c>null</c>.</exception>
    public LayeredCache(ICacheLayer<TKey, TValue> backing, IEqualityComparer<TKey>? comparer)
    {
        ArgumentNullException.ThrowIfNull(backing);
        _backing = backing;
        _local = new ConcurrentDictionary<TKey, TValue>(comparer);
    }

    ///<summary>
    ///Removes all entries from the local layer, without affecting the backing layer.
    ///</summary>
    public void ClearLocal() => _local.Clear();

    ///<summary>
    ///Removes the entry with the specified key from the local layer, without affecting the backing layer.
    ///</summary>
    ///<param name="key">The key to remove.</param>
    ///<returns><c>true</c> if the entry was found and removed; otherwise <c>false</c>.</returns>
    public bool InvalidateLocal(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _local.TryRemove(key, out _);
    }

    ///<summary>
    ///Stores the value for the specified key in both the local and backing layers.
    ///</summary>
    ///<param name="key">The key of the entry.</param>
    ///<param name="value">The value to store.</param>
    ///<param name="cancellationToken">A cancellation token for the backing-layer write.</param>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public async ValueTask SetAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        _local[key] = value;
        await _backing.SetAsync(key, value, cancellationToken).ConfigureAwait(false);
    }

    ///<summary>
    ///Attempts to retrieve the value for the specified key, checking the local layer first and falling back to the
    ///backing layer on a miss. A backing-layer hit populates the local layer.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="cancellationToken">A cancellation token for a potential backing-layer lookup.</param>
    ///<returns>
    ///A tuple where <c>Found</c> indicates whether the key was present in either layer, and <c>Value</c> holds the
    ///retrieved value when it was.
    ///</returns>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public async ValueTask<(bool Found, TValue? Value)> TryGetAsync(TKey key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        if(_local.TryGetValue(key, out TValue? localValue))
        {
            return (true, localValue);
        }

        (bool found, TValue backingValue) = await _backing.TryGetAsync(key, cancellationToken).ConfigureAwait(false);

        if(found)
        {
            _local[key] = backingValue;
            return (true, backingValue);
        }

        return (false, default);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the current number of entries in the local layer.
    ///</summary>
    public int LocalCount => _local.Count;
    #endregion
}
