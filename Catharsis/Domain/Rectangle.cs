namespace Catharsis.Domain;

///<summary>
///A rectangle. It is not sealed because <see cref="Square"/> is a rectangle with equal sides, and so can be used
///anywhere a <see cref="Rectangle"/> is expected.
///</summary>
///<param name="width">The width. Must be positive and finite.</param>
///<param name="height">The height. Must be positive and finite.</param>
///<exception cref="ArgumentOutOfRangeException">A dimension is not a positive, finite number.</exception>
public class Rectangle(double width, double height) : Shape
{
    #region Public properties
    ///<inheritdoc/>
    public override double Area => Width * Height;

    ///<summary>
    ///Gets the height.
    ///</summary>
    public double Height { get; } = RequirePositive(height, nameof(height));

    ///<inheritdoc/>
    public override double Perimeter => 2 * (Width + Height);

    ///<summary>
    ///Gets the width.
    ///</summary>
    public double Width { get; } = RequirePositive(width, nameof(width));
    #endregion
}
