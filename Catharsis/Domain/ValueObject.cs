namespace Catharsis.Domain;

///<summary>
///Base class for value objects: types with no identity, equal when all their parts are equal. A subclass lists its
///parts once in <see cref="GetEqualityComponents"/> and gets <see cref="Equals(ValueObject?)"/>,
///<see cref="GetHashCode"/> and the <c>==</c> and <c>!=</c> operators for free. Two value objects of different concrete
///types are never equal.
///</summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    #region Operators
    ///<summary>Determines whether two value objects are equal.</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right) => left is null ? right is null : left.Equals(right);

    ///<summary>Determines whether two value objects differ.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public bool Equals(ValueObject? other) => other is not null && GetType() == other.GetType() && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    ///<inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    ///<inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(GetType());

        foreach (object? component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }
    #endregion

    #region Protected methods
    ///<summary>
    ///Yields every part that participates in equality, in a fixed order.
    ///</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();
    #endregion
}
