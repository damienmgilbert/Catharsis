namespace Catharsis.Services;

///<summary>
///Defines how a pool creates new instances of <typeparamref name="T"/> and decides whether a returned instance is
///fit to keep, mirroring the shape of <c>Microsoft.Extensions.ObjectPool.IPooledObjectPolicy&lt;T&gt;</c> without
///depending on that package.
///</summary>
///<typeparam name="T">The type of object managed by the policy.</typeparam>
public interface IPooledObjectPolicy<T>
{
    #region Public methods
    ///<summary>
    ///Creates a new instance of <typeparamref name="T"/> for the pool.
    ///</summary>
    ///<returns>A new instance.</returns>
    T Create();

    ///<summary>
    ///Determines whether a returned instance should be kept in the pool, optionally resetting its state first.
    ///</summary>
    ///<param name="value">The instance being returned.</param>
    ///<returns><c>true</c> if the instance should be kept in the pool; <c>false</c> if it should be discarded.</returns>
    bool Return(T value);
    #endregion
}
