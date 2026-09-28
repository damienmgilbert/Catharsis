using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Catharsis.DataStructures;

///<summary>
///A fixed-capacity cache that evicts the least-recently-used (LRU) entry when a new entry is added and the cache
///is at capacity.
///</summary>
///<typeparam name="TKey">The type of the cache keys.</typeparam>
///<typeparam name="TValue">The type of the cached values.</typeparam>
public sealed class LruCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
{
    #region Fields
    readonly int _capacity;
    readonly Dictionary<TKey, LinkedListNode<CacheEntry>> _map;
    readonly LinkedList<CacheEntry> _order = new();
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="LruCache{TKey, TValue}"/> with the specified capacity.
    ///</summary>
    ///<param name="capacity">The maximum number of entries. Must be greater than zero.</param>
    ///<exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than or equal to zero.</exception>
    public LruCache(int capacity) : this(capacity, EqualityComparer<TKey>.Default)
    {
    }

    ///<summary>
    ///Initializes a new <see cref="LruCache{TKey, TValue}"/> with the specified capacity and key equality
    ///comparer.
    ///</summary>
    ///<param name="capacity">The maximum number of entries. Must be greater than zero.</param>
    ///<param name="comparer">The comparer used for key equality.</param>
    ///<exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than or equal to zero.</exception>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="comparer"/> is <c>null</c>.</exception>
    public LruCache(int capacity, IEqualityComparer<TKey> comparer)
    {
        if(capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
        }

        if(comparer is null)
        {
            throw new ArgumentNullException(nameof(comparer), "Equality comparer must not be null.");
        }

        _capacity = capacity;
        _map = new Dictionary<TKey, LinkedListNode<CacheEntry>>(capacity, comparer);
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets or sets the value for the specified key. Getting promotes the entry to most-recently-used. Setting behaves
    ///like <see cref="AddOrUpdate"/>.
    ///</summary>
    ///<param name="key">The cache key.</param>
    ///<returns>The cached value.</returns>
    ///<exception cref="KeyNotFoundException">Thrown when the key is not found on get.</exception>
    public TValue this[TKey key]
    {
        get
        {
            if(TryGetValue(key, out TValue? value))
            {
                return value;
            }

            throw new KeyNotFoundException($"Key '{key}' was not found in the cache.");
        }
        set => AddOrUpdate(key, value);
    }
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds or updates a cache entry. If the cache is at capacity the least-recently-used entry is evicted first.
    ///</summary>
    ///<param name="key">The key of the entry.</param>
    ///<param name="value">The value to cache.</param>
    public void AddOrUpdate(TKey key, TValue value)
    {
        if(_map.TryGetValue(key, out LinkedListNode<CacheEntry>? existingNode))
        {
            _order.Remove(existingNode);
            _map.Remove(key);
        } else if(_map.Count >= _capacity)
        {
            LinkedListNode<CacheEntry> lru = _order.Last!;
            _order.RemoveLast();
            _map.Remove(lru.Value.Key);
        }

        CacheEntry entry = new(key, value);
        LinkedListNode<CacheEntry> node = _order.AddFirst(entry);
        _map[key] = node;
    }

    ///<summary>
    ///Removes all entries from the cache.
    ///</summary>
    public void Clear()
    {
        _map.Clear();
        _order.Clear();
    }

    ///<summary>
    ///Determines whether the cache contains the specified key without affecting the usage order.
    ///</summary>
    ///<param name="key">The key to look for.</param>
    ///<returns><c>true</c> if the key exists; otherwise <c>false</c>.</returns>
    public bool ContainsKey(TKey key) { return _map.ContainsKey(key); }

    ///<summary>
    ///Enumerates entries from most-recently-used to least-recently-used.
    ///</summary>
    ///<inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        LinkedListNode<CacheEntry>? current = _order.First;

        while(current is not null)
        {
            yield return new KeyValuePair<TKey, TValue>(current.Value.Key, current.Value.Value);
            current = current.Next;
        }
    }

    ///<summary>
    ///Removes the entry with the specified key.
    ///</summary>
    ///<param name="key">The key to remove.</param>
    ///<returns><c>true</c> if the entry was found and removed; otherwise <c>false</c>.</returns>
    public bool Remove(TKey key)
    {
        if(!_map.TryGetValue(key, out LinkedListNode<CacheEntry>? node))
        {
            return false;
        }

        _order.Remove(node);
        _map.Remove(key);
        return true;
    }

    ///<summary>
    ///Attempts to retrieve the value associated with the specified key. A successful lookup promotes the entry to most-
    ///recently-used.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="value">The cached value, if found.</param>
    ///<returns><c>true</c> if the key was found; otherwise <c>false</c>.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if(_map.TryGetValue(key, out LinkedListNode<CacheEntry>? node))
        {
            _order.Remove(node);
            _order.AddFirst(node);
            value = node.Value.Value;
            return true;
        }

        value = default;
        return false;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the maximum number of entries this cache can hold.
    ///</summary>
    public int Capacity => _capacity;

    ///<summary>
    ///Gets the current number of entries in the cache.
    ///</summary>
    public int Count => _map.Count;
    #endregion

    readonly record struct CacheEntry(TKey Key, TValue Value);
}
