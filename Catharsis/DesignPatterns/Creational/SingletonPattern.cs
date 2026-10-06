using System.Collections.Concurrent;

namespace Catharsis.DesignPatterns.Creational;

///<summary>
///Implements the Singleton design pattern.
///</summary>
public class SingletonPattern
{
    #region Public methods

    ///<summary>
    ///Singleton — stores <paramref name="obj"/> in <paramref name="cache"/> under <paramref name="key"/> on first
    ///access and returns the cached instance on subsequent calls. Thread-safe.
    ///</summary>
    ///<typeparam name="T">The type of the singleton instance.</typeparam>
    ///<typeparam name="TKey">The type of the cache key.</typeparam>
    ///<param name="obj">The instance to cache if not already present.</param>
    ///<param name="key">The key that identifies the singleton in the cache.</param>
    ///<param name="cache">A thread-safe dictionary used as the singleton store.</param>
    ///<returns>The cached singleton instance for <paramref name="key"/>.</returns>
    public static T Singleton<T, TKey>(T obj, TKey key, ConcurrentDictionary<TKey, T> cache) where TKey : notnull
    {
        if(cache is null)
        {
            throw new ArgumentNullException(nameof(cache), "Singleton cache must not be null.");
        }

        return cache.GetOrAdd(key, _ => obj);
    }
    #endregion
}
