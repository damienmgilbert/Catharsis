namespace Catharsis.Domain;

///<summary>
///Base class for entities: objects defined by an identity that persists while their other state changes. Two entities
///are equal when they are the same concrete type and have the same <see cref="Id"/>. The identity can be read but never
///reassigned, which keeps it stable for use as a dictionary key.
///</summary>
///<typeparam name="TId">The identity type.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull, IEquatable<TId>
{
    #region Constructors

    ///<summary>
    ///Initializes a new entity.
    ///</summary>
    ///<param name="id">The identity. Must not be <c>null</c>.</param>
    ///<exception cref="ArgumentNullException"><paramref name="id"/> is <c>null</c>.</exception>
    protected Entity(TId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
    }
    #endregion

    #region Operators
    ///<summary>
    ///Determines whether two entities are different entities.
    ///</summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    ///<summary>
    ///Determines whether two entities are the same entity.
    ///</summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return left is null ? right is null : left.Equals(right);
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public bool Equals(Entity<TId>? other) => other is not null && (ReferenceEquals(this, other) || (GetType() == other.GetType() && Id.Equals(other.Id)));

    ///<inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the identity.
    ///</summary>
    public TId Id { get; }
    #endregion
}
