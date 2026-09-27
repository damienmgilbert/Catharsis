namespace Catharsis.Caching;

///<summary>
///Represents a single backing layer for a <see cref="LayeredCache{TKey, TValue}"/>, such as a distributed cache
///sitting behind an in-memory layer. Implementations are supplied by the caller, so <see cref="LayeredCache{TKey, TValue}"/>
///has no dependency on any specific distributed-cache technology.
///</summary>
///<typeparam name="TKey">The type of the cache keys.</typeparam>
///<typeparam name="TValue">The type of the cached values.</typeparam>
public interface ICacheLayer<TKey, TValue>
{
    #region Public methods
    ///<summary>
    ///Attempts to retrieve the value for the specified key from this layer.
    ///</summary>
    ///<param name="key">The key to look up.</param>
    ///<param name="cancellationToken">A cancellation token for the lookup.</param>
    ///<returns>
    ///A tuple where <c>Found</c> indicates whether the key was present, and <c>Value</c> holds the retrieved value
    ///when it was.
    ///</returns>
    ValueTask<(bool Found, TValue Value)> TryGetAsync(TKey key, CancellationToken cancellationToken = default);

    ///<summary>
    ///Stores the value for the specified key in this layer.
    ///</summary>
    ///<param name="key">The key of the entry.</param>
    ///<param name="value">The value to store.</param>
    ///<param name="cancellationToken">A cancellation token for the write.</param>
    ValueTask SetAsync(TKey key, TValue value, CancellationToken cancellationToken = default);
    #endregion
}
