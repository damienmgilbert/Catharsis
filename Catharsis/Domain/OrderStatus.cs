namespace Catharsis.Domain;

///<summary>
///Where an <see cref="Order"/> is in its life.
///</summary>
[EnumExtensions]
public enum OrderStatus
{
    ///<summary>
    ///Lines can still be added.
    ///</summary>
    Draft,

    ///<summary>
    ///The order has been placed and is locked.
    ///</summary>
    Confirmed,

    ///<summary>
    ///The order has been abandoned.
    ///</summary>
    Cancelled
}
