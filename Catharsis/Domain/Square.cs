namespace Catharsis.Domain;

///<summary>
///A square: a <see cref="Rectangle"/> whose width and height are the same.
///</summary>
///<param name="side">The side length. Must be positive and finite.</param>
///<exception cref="ArgumentOutOfRangeException"><paramref name="side"/> is not a positive, finite number.</exception>
public sealed class Square(double side) : Rectangle(side, side)
{
    #region Public properties

    ///<summary>
    ///Gets the side length.
    ///</summary>
    public double Side => Width;
    #endregion
}
