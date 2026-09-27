using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Catharsis.Caching;

///<summary>
///A thread-safe cache whose entries expire a fixed duration after they are set. Expired entries are removed lazily
///on lookup, and <see cref="RemoveExpired"/> can be called to purge them proactively.
///</summary>
///<typeparam name="TKey">The type of the cache keys.</typeparam>
///<typeparam name="TValue">The type of the cached values.</typeparam>
///<example>
///<code>
///TtlCache&lt;string, Session&gt; sessions = new(defaultTtl: TimeSpan.FromMinutes(20));
///sessions.Set(sessionId, session);
///
///if(sessions.TryGetValue(sessionId, out Session? session))
///{
///    // Still within its 20-minute window.
///}
///</code>
///</example>
public sealed class TtlCache<TKey, TValue> where TKey : notnull
{
    #region Fields
    readonly ConcurrentDictionary<TKey, Entry> _entries;
    readonly TimeSpan _defaultTtl;
    readonly TimeProvider _timeProvider;
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a cache with the specified default entry lifetime.
    ///</summary>
    ///<param name="defaultTtl">The lifetime applied to entries set via <see cref="Set(TKey, TValue)"/>.</param>
    ///<param name="timeProvider">The time source used to evaluate expiration, or <c>null</c> to use <see cref="TimeProvider.System"/>.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="defaultTtl"/> is not greater than zero.</exception>
    public TtlCache(TimeSpan defaultTtl, TimeProvider? timeProvider = null) : this(defaultTtl, null, timeProvider)
    {
    }

    ///<summary>
    ///Creates a cache with the specified default entry lifetime and key equality comparer.
    ///</summary>
    ///<param name="defaultTtl">The lifetime applied to entries set via <see cref="Set(TKey, TValue)"/>.</param>
    ///<param name="comparer">The equality comparer used to match keys, or <c>null</c> to use the default comparer.</param>
    ///<param name="timeProvider">The time source used to evaluate expiration, or <c>null</c> to use <see cref="TimeProvider.System"/>.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="defaultTtl"/> is not greater than zero.</exception>
    public TtlCache(TimeSpan defaultTtl, IEqualityComparer<TKey>? comparer, TimeProvider? timeProvider = null)
    {
        if(defaultTtl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultTtl), "Default TTL must be greater than zero.");
        }

        _defaultTtl = defaultTtl;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _entries = new ConcurrentDictionary<TKey, Entry>(comparer);
    }

    ///<summary>
    ///Removes all entries from the cache.
    ///</summary>
    public void Clear() => _entries.Clear();

    ///<summary>
    ///Removes every entry that has already expired.
    ///</summary>
    ///<returns>The number of entries removed.</returns>
    public int RemoveExpired()
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        int removed = 0;

        foreach(KeyValuePair<TKey, Entry> entry in _entries)
        {
            if(entry.Value.ExpiresAt <= now && _entries.TryRemove(entry.Key, out _))
            {
                removed++;
            }
        }

        return removed;
    }

    ///<summary>
    ///Sets the value for the specified key using the default TTL.
    ///</summary>
    ///<param name="key">The key of the entry.</param>
    ///<param name="value">The value to cache.</param>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public void Set(TKey key, TValue value) => Set(key, value, _defaultTtl);

    ///<summary>
    ///Sets the value for the specified key using the specified TTL.
    ///</summary>
    ///<param name="key">The key of the entry.</param>
    ///<param name="value">The value to cache.</param>
    ///<param name="ttl">The lifetime of this entry, overriding the cache's default TTL.</param>
    ///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="ttl"/> is not greater than zero.</exception>
    public void Set(TKey key, TValue value, TimeSpan ttl)
    {
        ArgumentNullException.ThrowIfNull(key);

        if(ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), "TTL must be greater than zero.");
        }

        _entries[key] = new Entry(value, _timeProvider.GetUtcNow() + ttl);
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
    ///Attempts to retrieve the value for the specified key, treating an expired entry as absent.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="value">The cached value, if present and not expired.</param>
    ///<returns><c>true</c> if a non-expired entry was found; otherwise <c>false</c>.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if(_entries.TryGetValue(key, out Entry entry))
        {
            if(entry.ExpiresAt > _timeProvider.GetUtcNow())
            {
                value = entry.Value;
                return true;
            }

            _entries.TryRemove(key, out _);
        }

        value = default;
        return false;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of entries currently stored, including any not-yet-purged expired entries.
    ///</summary>
    public int Count => _entries.Count;
    #endregion

    readonly record struct Entry(TValue Value, DateTimeOffset ExpiresAt);
}
