namespace Catharsis.DesignPatterns.Enterprise;

///<summary>
///Implements the Repository design pattern: a minimal in-memory scaffold showing the shape of a generic repository
///abstraction, decoupling callers from how entities are actually stored.
///</summary>
///<typeparam name="TEntity">The type of entity stored in the repository.</typeparam>
///<typeparam name="TKey">The type of the entity's key.</typeparam>
///<param name="keySelector">A delegate that extracts an entity's key.</param>
public sealed class RepositoryPattern<TEntity, TKey>(Func<TEntity, TKey> keySelector) where TKey : notnull
{
    #region Fields
    readonly Func<TEntity, TKey> _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector), "Key selector must not be null.");
    readonly Dictionary<TKey, TEntity> _store = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Adds or replaces an entity, keyed by the configured key selector.
    ///</summary>
    ///<param name="entity">The entity to add or replace.</param>
    ///<exception cref="ArgumentNullException"><paramref name="entity"/> is <c>null</c>.</exception>
    public void Add(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _store[_keySelector(entity)] = entity;
    }

    ///<summary>
    ///Returns every entity currently in the repository.
    ///</summary>
    ///<returns>A snapshot of all stored entities.</returns>
    public IReadOnlyCollection<TEntity> GetAll() => [.. _store.Values];

    ///<summary>
    ///Removes the entity with the specified key.
    ///</summary>
    ///<param name="key">The key of the entity to remove.</param>
    ///<returns><c>true</c> if the entity was found and removed; otherwise <c>false</c>.</returns>
    public bool Remove(TKey key) => _store.Remove(key);

    ///<summary>
    ///Attempts to retrieve the entity with the specified key.
    ///</summary>
    ///<param name="key">The key of the entity to find.</param>
    ///<param name="entity">The found entity, if any.</param>
    ///<returns><c>true</c> if an entity with the specified key was found; otherwise <c>false</c>.</returns>
    public bool TryGet(TKey key, out TEntity entity) => _store.TryGetValue(key, out entity!);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of entities in the repository.
    ///</summary>
    public int Count => _store.Count;
    #endregion
}
