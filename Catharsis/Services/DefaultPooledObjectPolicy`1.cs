namespace Catharsis.Services;

///<summary>
///A general-purpose <see cref="IPooledObjectPolicy{T}"/> that creates instances via a factory delegate and, optionally,
///resets or rejects returned instances via a predicate.
///</summary>
///<typeparam name="T">The type of object managed by the policy.</typeparam>
///<param name="factory">The delegate used to create new instances.</param>
///<param name="returnPredicate">
///An optional delegate invoked when an instance is returned. It may reset the instance's state as a side effect, and
///returns <c>true</c> to keep the instance in the pool or <c>false</c> to discard it. When <c>null</c>, every returned
///instance is kept.
///</param>
public sealed class DefaultPooledObjectPolicy<T>(Func<T> factory, Func<T, bool>? returnPredicate = null) : IPooledObjectPolicy<T>
{
    #region Fields
    private readonly Func<T> _factory = factory ?? throw new ArgumentNullException(nameof(factory), "Factory must not be null.");
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public T Create() => _factory();

    ///<inheritdoc/>
    public bool Return(T value) => returnPredicate?.Invoke(value) ?? true;
    #endregion
}
