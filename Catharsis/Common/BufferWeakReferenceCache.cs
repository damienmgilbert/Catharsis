using System.Collections.Concurrent;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Common;

///<summary>
///A cache that holds weak references to buffer objects, allowing them to be garbage collected when memory pressure
///increases while providing reuse when possible.
///</summary>
///<typeparam name="T">The type of objects to cache. Must be a reference type.</typeparam>
public sealed class BufferWeakReferenceCache<T> where T : class
{
    #region Fields
    readonly ConcurrentDictionary<string, WeakReference<T>> _cache = new();
    #endregion

    #region Public methods
    ///<summary>
    ///Clears all entries from the cache.
    ///</summary>
    public void Clear() { _cache.Clear(); }

    ///<summary>
    ///Gets an existing cached value or creates and caches a new one using the factory.
    ///</summary>
    ///<param name="key">The cache key.</param>
    ///<param name="factory">The factory to create a new value if not cached.</param>
    ///<returns>The cached or newly created value.</returns>
    public T GetOrCreate(string key, Func<T> factory)
    {
        Guard.IsNotNullOrEmpty(key);
        Guard.IsNotNull(factory);

        if(TryGet(key, out T? existing) && existing is not null)
        {
            return existing;
        }

        T value = factory();
        Set(key, value);
        return value;
    }

    ///<summary>
    ///Removes all entries whose weak references have been collected.
    ///</summary>
    ///<returns>The number of dead entries removed.</returns>
    public int Purge()
    {
        int removed = 0;
        foreach(string key in _cache.Keys)
        {
            if(_cache.TryGetValue(key, out WeakReference<T>? weakRef) && !weakRef.TryGetTarget(out _))
            {
                if(_cache.TryRemove(key, out _))
                {
                    removed++;
                }
            }
        }
        return removed;
    }

    ///<summary>
    ///Removes the entry with the specified key.
    ///</summary>
    ///<param name="key">The cache key to remove.</param>
    ///<returns><c>true</c> if the entry was removed; otherwise <c>false</c>.</returns>
    public bool Remove(string key)
    {
        Guard.IsNotNullOrEmpty(key);
        return _cache.TryRemove(key, out _);
    }

    ///<summary>
    ///Adds or updates a cached object under the specified key.
    ///</summary>
    ///<param name="key">The cache key.</param>
    ///<param name="value">The object to cache.</param>
    public void Set(string key, T value)
    {
        Guard.IsNotNullOrEmpty(key);
        Guard.IsNotNull(value);

        _cache[key] = new WeakReference<T>(value);
    }

    ///<summary>
    ///Attempts to retrieve a cached object by key.
    ///</summary>
    ///<param name="key">The cache key.</param>
    ///<param name="value">The cached object, if still alive.</param>
    ///<returns><c>true</c> if the object was found and is still alive; otherwise <c>false</c>.</returns>
    public bool TryGet(string key, out T? value)
    {
        Guard.IsNotNullOrEmpty(key);

        if(_cache.TryGetValue(key, out WeakReference<T>? weakRef) && weakRef.TryGetTarget(out value))
        {
            return true;
        }

        value = null;
        return false;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of entries in the cache (including potentially collected items).
    ///</summary>
    public int Count => _cache.Count;
    #endregion
}
