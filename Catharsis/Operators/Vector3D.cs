using System.Globalization;

namespace Catharsis.Operators;

///<summary>
///An immutable three-dimensional vector of <see cref="double"/> components with the usual algebra as operators and an
///axis indexer (0 = X, 1 = Y, 2 = Z).
///</summary>
///<param name="x">The X component.</param>
///<param name="y">The Y component.</param>
///<param name="z">The Z component.</param>
public readonly struct Vector3D(double x, double y, double z) : IEquatable<Vector3D>
{

    #region Operators
    ///<summary>Adds two vectors component-wise.</summary>
    public static Vector3D operator +(Vector3D left, Vector3D right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    ///<summary>Subtracts one vector from another component-wise.</summary>
    public static Vector3D operator -(Vector3D left, Vector3D right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    ///<summary>Negates a vector.</summary>
    public static Vector3D operator -(Vector3D value) => new(-value.X, -value.Y, -value.Z);

    ///<summary>Scales a vector.</summary>
    public static Vector3D operator *(Vector3D value, double scalar) => new(value.X * scalar, value.Y * scalar, value.Z * scalar);

    ///<summary>Scales a vector.</summary>
    public static Vector3D operator *(double scalar, Vector3D value) => value * scalar;

    ///<summary>Divides a vector by a scalar.</summary>
    public static Vector3D operator /(Vector3D value, double scalar) => new(value.X / scalar, value.Y / scalar, value.Z / scalar);

    ///<summary>Computes the dot product of two vectors.</summary>
    public static double operator *(Vector3D left, Vector3D right) => (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);

    ///<summary>Computes the cross product of two vectors.</summary>
    public static Vector3D operator ^(Vector3D left, Vector3D right) => new(
        (left.Y * right.Z) - (left.Z * right.Y),
        (left.Z * right.X) - (left.X * right.Z),
        (left.X * right.Y) - (left.Y * right.X));

    ///<summary>Determines whether two vectors are equal.</summary>
    public static bool operator ==(Vector3D left, Vector3D right) => left.Equals(right);

    ///<summary>Determines whether two vectors differ.</summary>
    public static bool operator !=(Vector3D left, Vector3D right) => !left.Equals(right);

    ///<summary>Widens a <see cref="Vector2D"/> to a vector with <c>Z = 0</c>.</summary>
    public static implicit operator Vector3D(Vector2D value) => new(value.X, value.Y, 0);
    #endregion

    #region Public methods
    ///<summary>Returns the vector scaled to length one, or <see cref="Zero"/> if this vector has no length.</summary>
    public Vector3D Normalize() => Length == 0 ? Zero : this / Length;

    ///<inheritdoc/>
    public bool Equals(Vector3D other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    ///<inheritdoc/>
    public override bool Equals(object? obj) => (obj is Vector3D other) && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    ///<inheritdoc/>
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"({X}, {Y}, {Z})");
    #endregion

    #region Public properties
    ///<summary>Gets the zero vector.</summary>
    public static Vector3D Zero => default;

    ///<summary>Gets the X component.</summary>
    public double X { get; } = x;

    ///<summary>Gets the Y component.</summary>
    public double Y { get; } = y;

    ///<summary>Gets the Z component.</summary>
    public double Z { get; } = z;

    ///<summary>Gets the Euclidean length.</summary>
    public double Length => Math.Sqrt((X * X) + (Y * Y) + (Z * Z));

    ///<summary>Gets the component on <paramref name="axis"/> (0 = X, 1 = Y, 2 = Z).</summary>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="axis"/> is not 0, 1 or 2.</exception>
    public double this[int axis] => axis switch
    {
        0 => X,
        1 => Y,
        2 => Z,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "Axis must be 0, 1 or 2.")
    };
    #endregion
}
