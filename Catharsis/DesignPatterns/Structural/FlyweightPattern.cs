using System.Collections.Concurrent;

namespace Catharsis.DesignPatterns.Structural;

///<summary>
///Implements the Flyweight design pattern.
///</summary>
public class FlyweightPattern
{
    #region Public methods
    ///<summary>
    ///Flyweight — retrieves a shared <typeparamref name="T"/> instance from <paramref name="cache"/> for the given
    ///<paramref name="key"/>, or creates one via <paramref name="factory"/> on first access. Thread-safe.
    ///</summary>
    ///<typeparam name="TKey">The type of the extrinsic key.</typeparam>
    ///<typeparam name="T">The flyweight type.</typeparam>
    ///<param name="key">The extrinsic key identifying the shared instance.</param>
    ///<param name="cache">A thread-safe dictionary used as the flyweight pool.</param>
    ///<param name="factory">A delegate that creates a FileName flyweight when one is not cached.</param>
    ///<returns>The shared flyweight instance for <paramref name="key"/>.</returns>
    public static T Flyweight<TKey, T>(TKey key, ConcurrentDictionary<TKey, T> cache, Func<TKey, T> factory) where TKey : notnull
    {
        if(cache is null)
        {
            throw new ArgumentNullException(nameof(cache), "Flyweight cache must not be null.");
        }

        if(factory is null)
        {
            throw new ArgumentNullException(nameof(factory), "Flyweight factory must not be null.");
        }

        return cache.GetOrAdd(key, factory);
    }
    #endregion
}
