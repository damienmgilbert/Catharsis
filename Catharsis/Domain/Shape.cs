using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Catharsis.Domain;

///<summary>
///The abstraction shared by every two-dimensional shape. Callers work with <see cref="Shape"/> and get the right
///<see cref="Area"/> and <see cref="Perimeter"/> through polymorphism, without knowing which concrete shape they hold.
///Shapes order by area.
///</summary>
[SuppressMessage("Design", "CA1036:Override methods on comparable types", Justification = "Ordering is by area, so two different shapes can compare as equal in size; equality deliberately stays reference identity rather than area equality.")]
public abstract class Shape : IComparable<Shape>
{
    #region Operators
    ///<summary>Determines whether one shape has a smaller area than another.</summary>
    public static bool operator <(Shape? left, Shape? right) => left is null ? right is not null : left.CompareTo(right) < 0;

    ///<summary>Determines whether one shape has an area no larger than another.</summary>
    public static bool operator <=(Shape? left, Shape? right) => left is null || left.CompareTo(right) <= 0;

    ///<summary>Determines whether one shape has a larger area than another.</summary>
    public static bool operator >(Shape? left, Shape? right) => left is not null && left.CompareTo(right) > 0;

    ///<summary>Determines whether one shape has an area no smaller than another.</summary>
    public static bool operator >=(Shape? left, Shape? right) => left is null ? right is null : left.CompareTo(right) >= 0;
    #endregion

    #region Public methods
    ///<summary>
    ///Compares shapes by <see cref="Area"/>. A <c>null</c> shape sorts before any real shape.
    ///</summary>
    public int CompareTo(Shape? other) => other is null ? 1 : Area.CompareTo(other.Area);

    ///<summary>
    ///Returns the shape's name with its area and perimeter, for example <c>Circle (area 3.142, perimeter 6.283)</c>.
    ///</summary>
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Name} (area {Area:0.###}, perimeter {Perimeter:0.###})");
    #endregion

    #region Public properties
    ///<summary>Gets the enclosed area.</summary>
    public abstract double Area { get; }

    ///<summary>Gets the length of the boundary.</summary>
    public abstract double Perimeter { get; }

    ///<summary>Gets the shape's name. Defaults to the concrete type's name.</summary>
    public virtual string Name => GetType().Name;
    #endregion

    #region Protected methods
    ///<summary>
    ///Checks that a dimension is a positive, finite number.
    ///</summary>
    ///<param name="value">The dimension.</param>
    ///<param name="paramName">The parameter name for the exception.</param>
    ///<returns><paramref name="value"/>, so the call can be used inline.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero, negative, infinite or NaN.</exception>
    protected static double RequirePositive(double value, string paramName)
    {
        if(!(value > 0) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(paramName, value, "A dimension must be a positive, finite number.");
        }

        return value;
    }
    #endregion
}
