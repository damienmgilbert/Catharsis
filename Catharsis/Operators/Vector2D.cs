using System.Globalization;

namespace Catharsis.Operators;

///<summary>
///An immutable two-dimensional vector of <see cref="double"/> components with the usual algebra as operators and an
///axis indexer (0 = X, 1 = Y).
///</summary>
///<param name="x">The X component.</param>
///<param name="y">The Y component.</param>
public readonly struct Vector2D(double x, double y) : IEquatable<Vector2D>
{

    #region Operators
    ///<summary>Adds two vectors component-wise.</summary>
    public static Vector2D operator +(Vector2D left, Vector2D right) => new(left.X + right.X, left.Y + right.Y);

    ///<summary>Subtracts one vector from another component-wise.</summary>
    public static Vector2D operator -(Vector2D left, Vector2D right) => new(left.X - right.X, left.Y - right.Y);

    ///<summary>Negates a vector.</summary>
    public static Vector2D operator -(Vector2D value) => new(-value.X, -value.Y);

    ///<summary>Scales a vector.</summary>
    public static Vector2D operator *(Vector2D value, double scalar) => new(value.X * scalar, value.Y * scalar);

    ///<summary>Scales a vector.</summary>
    public static Vector2D operator *(double scalar, Vector2D value) => value * scalar;

    ///<summary>Divides a vector by a scalar.</summary>
    public static Vector2D operator /(Vector2D value, double scalar) => new(value.X / scalar, value.Y / scalar);

    ///<summary>Computes the dot product of two vectors.</summary>
    public static double operator *(Vector2D left, Vector2D right) => (left.X * right.X) + (left.Y * right.Y);

    ///<summary>Determines whether two vectors are equal.</summary>
    public static bool operator ==(Vector2D left, Vector2D right) => left.Equals(right);

    ///<summary>Determines whether two vectors differ.</summary>
    public static bool operator !=(Vector2D left, Vector2D right) => !left.Equals(right);

    ///<summary>Converts a tuple to a vector.</summary>
    public static implicit operator Vector2D((double X, double Y) value) => new(value.X, value.Y);
    #endregion

    #region Public methods
    ///<summary>Returns the vector scaled to length one, or <see cref="Zero"/> if this vector has no length.</summary>
    public Vector2D Normalize() => Length == 0 ? Zero : this / Length;

    ///<summary>Computes the 2D cross product (the Z component of the 3D cross product).</summary>
    public double Cross(Vector2D other) => (X * other.Y) - (Y * other.X);

    ///<summary>Rotates the vector counter-clockwise by <paramref name="radians"/>.</summary>
    public Vector2D Rotate(double radians)
    {
        (double sin, double cos) = Math.SinCos(radians);
        return new Vector2D((X * cos) - (Y * sin), (X * sin) + (Y * cos));
    }

    ///<inheritdoc/>
    public bool Equals(Vector2D other) => X.Equals(other.X) && Y.Equals(other.Y);

    ///<inheritdoc/>
    public override bool Equals(object? obj) => (obj is Vector2D other) && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(X, Y);

    ///<inheritdoc/>
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"({X}, {Y})");
    #endregion

    #region Public properties
    ///<summary>Gets the zero vector.</summary>
    public static Vector2D Zero => default;

    ///<summary>Gets the X component.</summary>
    public double X { get; } = x;

    ///<summary>Gets the Y component.</summary>
    public double Y { get; } = y;

    ///<summary>Gets the Euclidean length.</summary>
    public double Length => Math.Sqrt((X * X) + (Y * Y));

    ///<summary>Gets the component on <paramref name="axis"/> (0 = X, 1 = Y).</summary>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="axis"/> is not 0 or 1.</exception>
    public double this[int axis] => axis switch
    {
        0 => X,
        1 => Y,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "Axis must be 0 or 1.")
    };
    #endregion
}
