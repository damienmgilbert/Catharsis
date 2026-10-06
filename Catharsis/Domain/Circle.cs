using Catharsis.Geometry;

namespace Catharsis.Domain;

///<summary>
///A circle, whose measurements come from <see cref="GeometryFormulas.Circle"/>.
///</summary>
///<param name="radius">The radius. Must be positive and finite.</param>
///<exception cref="ArgumentOutOfRangeException"><paramref name="radius"/> is not a positive, finite number.</exception>
public sealed class Circle(double radius) : Shape
{
    #region Public properties
    ///<summary>Gets the radius.</summary>
    public double Radius { get; } = RequirePositive(radius, nameof(radius));

    ///<inheritdoc/>
    public override double Area => GeometryFormulas.Circle.Area(Radius);

    ///<inheritdoc/>
    public override double Perimeter => GeometryFormulas.Circle.Circumference(Radius);
    #endregion
}
