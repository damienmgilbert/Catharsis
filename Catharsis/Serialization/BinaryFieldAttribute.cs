namespace Catharsis.Serialization;

///<summary>
///Marks a property for inclusion in a <see cref="BinaryRecordSerializer{T}"/>'s fixed layout and specifies its position
///within that layout.
///</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class BinaryFieldAttribute : Attribute
{
    #region Constructors

    ///<summary>
    ///Initializes a new instance of <see cref="BinaryFieldAttribute"/>.
    ///</summary>
    ///<param name="order">The zero-based position of this field within the record's binary layout.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="order"/> is negative.</exception>
    public BinaryFieldAttribute(int order)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(order);
        Order = order;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the zero-based position of this field within the record's binary layout.
    ///</summary>
    public int Order { get; }
    #endregion
}
