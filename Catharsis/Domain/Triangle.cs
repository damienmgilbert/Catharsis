namespace Catharsis.Domain;

///<summary>
///A triangle defined by its three side lengths. The area comes from Heron's formula, so no height is needed.
///</summary>
public sealed class Triangle : Shape
{
    #region Constructors
    ///<summary>Initializes a new <see cref="Triangle"/>.</summary>
    ///<param name="a">The first side. Must be positive and finite.</param>
    ///<param name="b">The second side. Must be positive and finite.</param>
    ///<param name="c">The third side. Must be positive and finite.</param>
    ///<exception cref="ArgumentOutOfRangeException">A side is not a positive, finite number.</exception>
    ///<exception cref="ArgumentException">The sides cannot form a triangle: one is not shorter than the other two combined.</exception>
    public Triangle(double a, double b, double c)
    {
        A = RequirePositive(a, nameof(a));
        B = RequirePositive(b, nameof(b));
        C = RequirePositive(c, nameof(c));

        if (a + b <= c || a + c <= b || b + c <= a)
        {
            throw new ArgumentException("Each side must be shorter than the other two combined.");
        }
    }
    #endregion

    #region Public properties
    ///<summary>Gets the first side.</summary>
    public double A { get; }

    ///<summary>Gets the second side.</summary>
    public double B { get; }

    ///<summary>Gets the third side.</summary>
    public double C { get; }

    ///<inheritdoc/>
    public override double Area
    {
        get
        {
            double s = Perimeter / 2;

            return Math.Sqrt(s * (s - A) * (s - B) * (s - C));
        }
    }

    ///<inheritdoc/>
    public override double Perimeter => A + B + C;
    #endregion
}
