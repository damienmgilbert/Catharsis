namespace Catharsis.Domain;

///<summary>
///A rectangle. It is not sealed because <see cref="Square"/> is a rectangle with equal sides, and so can be used
///anywhere a <see cref="Rectangle"/> is expected.
///</summary>
public class Rectangle : Shape
{
    #region Constructors
    ///<summary>Initializes a new <see cref="Rectangle"/>.</summary>
    ///<param name="width">The width. Must be positive and finite.</param>
    ///<param name="height">The height. Must be positive and finite.</param>
    ///<exception cref="ArgumentOutOfRangeException">A dimension is not a positive, finite number.</exception>
    public Rectangle(double width, double height)
    {
        Width = RequirePositive(width, nameof(width));
        Height = RequirePositive(height, nameof(height));
    }
    #endregion

    #region Public properties
    ///<summary>Gets the width.</summary>
    public double Width { get; }

    ///<summary>Gets the height.</summary>
    public double Height { get; }

    ///<inheritdoc/>
    public override double Area => Width * Height;

    ///<inheritdoc/>
    public override double Perimeter => 2 * (Width + Height);
    #endregion
}
