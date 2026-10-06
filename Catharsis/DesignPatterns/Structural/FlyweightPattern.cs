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
