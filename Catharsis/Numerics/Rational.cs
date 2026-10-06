using System.Numerics;

namespace Catharsis.Numerics;

///<summary>
///An exact rational number, represented as a <see cref="BigInteger"/> numerator and denominator kept permanently in
///lowest terms with a positive denominator. Unlike <see cref="double"/> or <see cref="decimal"/>, arithmetic on a
///<see cref="Rational"/> never loses precision, at the cost of the numerator and denominator growing without bound
///across repeated operations.
///</summary>
public readonly struct Rational : IEquatable<Rational>, IComparable<Rational>
{
    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="Rational"/> equal to the whole number <paramref name="value"/>.
    ///</summary>
    ///<param name="value">The whole number value.</param>
    public Rational(BigInteger value) : this(value, BigInteger.One) { }

    ///<summary>
    ///Initializes a new <see cref="Rational"/> equal to <paramref name="numerator"/> / <paramref name="denominator"/>,
    ///reducing it to lowest terms.
    ///</summary>
    ///<param name="numerator">The numerator.</param>
    ///<param name="denominator">The denominator. Must not be zero.</param>
    ///<exception cref="DivideByZeroException"><paramref name="denominator"/> is zero.</exception>
    public Rational(BigInteger numerator, BigInteger denominator)
    {
        if (denominator.IsZero)
        {
            throw new DivideByZeroException("Denominator must not be zero.");
        }

        if (denominator.Sign < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        BigInteger divisor = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
        Numerator = numerator / divisor;
        Denominator = denominator / divisor;
    }
    #endregion

    #region Operators
    ///<summary>Adds two rationals.</summary>
    public static Rational operator +(Rational left, Rational right) => new((left.Numerator * right.Denominator) + (right.Numerator * left.Denominator), left.Denominator * right.Denominator);

    ///<summary>Subtracts one rational from another.</summary>
    public static Rational operator -(Rational left, Rational right) => new((left.Numerator * right.Denominator) - (right.Numerator * left.Denominator), left.Denominator * right.Denominator);

    ///<summary>Negates a rational.</summary>
    public static Rational operator -(Rational value) => new(-value.Numerator, value.Denominator);

    ///<summary>Multiplies two rationals.</summary>
    public static Rational operator *(Rational left, Rational right) => new(left.Numerator * right.Numerator, left.Denominator * right.Denominator);

    ///<summary>Divides one rational by another.</summary>
    ///<exception cref="DivideByZeroException"><paramref name="right"/> is zero.</exception>
    public static Rational operator /(Rational left, Rational right) => new(left.Numerator * right.Denominator, left.Denominator * right.Numerator);

    ///<summary>Determines whether two rationals are equal.</summary>
    public static bool operator ==(Rational left, Rational right) => left.Equals(right);

    ///<summary>Determines whether two rationals are not equal.</summary>
    public static bool operator !=(Rational left, Rational right) => !left.Equals(right);

    ///<summary>Determines whether one rational is less than another.</summary>
    public static bool operator <(Rational left, Rational right) => left.CompareTo(right) < 0;

    ///<summary>Determines whether one rational is less than or equal to another.</summary>
    public static bool operator <=(Rational left, Rational right) => left.CompareTo(right) <= 0;

    ///<summary>Determines whether one rational is greater than another.</summary>
    public static bool operator >(Rational left, Rational right) => left.CompareTo(right) > 0;

    ///<summary>Determines whether one rational is greater than or equal to another.</summary>
    public static bool operator >=(Rational left, Rational right) => left.CompareTo(right) >= 0;

    ///<summary>Converts a <see cref="BigInteger"/> to an equivalent <see cref="Rational"/>.</summary>
    public static implicit operator Rational(BigInteger value) => new(value);

    ///<summary>Converts a <see cref="long"/> to an equivalent <see cref="Rational"/>.</summary>
    public static implicit operator Rational(long value) => new(value);
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public int CompareTo(Rational other) => (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);

    ///<inheritdoc/>
    public bool Equals(Rational other) => (Numerator == other.Numerator) && (Denominator == other.Denominator);

    ///<inheritdoc/>
    public override bool Equals(object? obj) => (obj is Rational other) && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

    ///<summary>
    ///Converts this rational to the nearest <see cref="double"/>, which may lose precision.
    ///</summary>
    ///<returns>The closest <see cref="double"/> approximation of this value.</returns>
    public double ToDouble() => (double)Numerator / (double)Denominator;

    ///<summary>
    ///Returns a string in the form <c>numerator/denominator</c>, or just <c>numerator</c> when the denominator is 1.
    ///</summary>
    public override string ToString() => Denominator.IsOne ? Numerator.ToString(System.Globalization.CultureInfo.InvariantCulture) : string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{Numerator}/{Denominator}");
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the denominator in lowest terms. Always positive.
    ///</summary>
    public BigInteger Denominator { get; }

    ///<summary>
    ///Gets the numerator in lowest terms.
    ///</summary>
    public BigInteger Numerator { get; }
    #endregion
}
