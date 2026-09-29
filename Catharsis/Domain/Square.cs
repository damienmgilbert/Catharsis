namespace Catharsis.Domain;

///<summary>
///A square: a <see cref="Rectangle"/> whose width and height are the same.
///</summary>
public sealed class Square : Rectangle
{
    #region Constructors
    ///<summary>Initializes a new <see cref="Square"/>.</summary>
    ///<param name="side">The side length. Must be positive and finite.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="side"/> is not a positive, finite number.</exception>
    public Square(double side) : base(side, side) { }
    #endregion

    #region Public properties
    ///<summary>Gets the side length.</summary>
    public double Side => Width;
    #endregion
}
