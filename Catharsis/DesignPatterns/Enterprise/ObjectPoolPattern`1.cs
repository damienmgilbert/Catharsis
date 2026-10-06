using System.Collections.Concurrent;

namespace Catharsis.DesignPatterns.Enterprise;

///<summary>
///Implements the Object Pool design pattern: a minimal scaffold showing the pattern's shape (rent, use, return). For a
///production-ready pool with DI and logging integration, use ///<see cref="Catharsis.Services.PooledObjectFactory{T}"/>
///instead.
///</summary>
///<typeparam name="T">The type of object to pool.</typeparam>
///<param name="factory">A delegate that creates a new instance when the pool is empty.</param>
public sealed class ObjectPoolPattern<T>(Func<T> factory)
{
    #region Fields
    private readonly Func<T> _factory = factory ?? throw new ArgumentNullException(nameof(factory), "Factory must not be null.");
    private readonly ConcurrentBag<T> _items = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Rents an item from the pool, creating a new one via the configured factory if the pool is currently empty.
    ///</summary>
    ///<returns>An item ready for use.</returns>
    public T Rent() => _items.TryTake(out T? item) ? item : _factory();

    ///<summary>
    ///Returns an item to the pool for reuse.
    ///</summary>
    ///<param name="item">The item to return.</param>
    public void Return(T item) => _items.Add(item);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of items currently available in the pool.
    ///</summary>
    public int Count => _items.Count;
    #endregion
}
