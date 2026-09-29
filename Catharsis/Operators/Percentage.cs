using System.Globalization;

namespace Catharsis.Operators;

///<summary>
///A ratio expressed as a percentage. Stores the underlying fraction (<c>0.25</c> for 25%), and lets a percentage be
///applied to a number or a <see cref="Money"/> with the <c>*</c> operator.
///</summary>
public readonly struct Percentage : IEquatable<Percentage>, IComparable<Percentage>, IFormattable
{
    #region Constructors
    private Percentage(decimal fraction) { Fraction = fraction; }
    #endregion

    #region Operators
    ///<summary>Adds two percentages.</summary>
    public static Percentage operator +(Percentage left, Percentage right) => new(left.Fraction + right.Fraction);

    ///<summary>Subtracts one percentage from another.</summary>
    public static Percentage operator -(Percentage left, Percentage right) => new(left.Fraction - right.Fraction);

    ///<summary>Applies a percentage to a number.</summary>
    public static decimal operator *(decimal value, Percentage percentage) => value * percentage.Fraction;

    ///<summary>Applies a percentage to a number.</summary>
    public static decimal operator *(Percentage percentage, decimal value) => value * percentage.Fraction;

    ///<summary>Applies a percentage to a monetary amount.</summary>
    public static Money operator *(Money value, Percentage percentage) => value * percentage.Fraction;

    ///<summary>Determines whether two percentages are equal.</summary>
    public static bool operator ==(Percentage left, Percentage right) => left.Equals(right);

    ///<summary>Determines whether two percentages differ.</summary>
    public static bool operator !=(Percentage left, Percentage right) => !left.Equals(right);

    ///<summary>Determines whether one percentage is smaller than another.</summary>
    public static bool operator <(Percentage left, Percentage right) => left.CompareTo(right) < 0;

    ///<summary>Determines whether one percentage is smaller than or equal to another.</summary>
    public static bool operator <=(Percentage left, Percentage right) => left.CompareTo(right) <= 0;

    ///<summary>Determines whether one percentage is larger than another.</summary>
    public static bool operator >(Percentage left, Percentage right) => left.CompareTo(right) > 0;

    ///<summary>Determines whether one percentage is larger than or equal to another.</summary>
    public static bool operator >=(Percentage left, Percentage right) => left.CompareTo(right) >= 0;
    #endregion

    #region Public methods
    ///<summary>Creates a percentage from a whole-percent value, so <c>FromPercent(25)</c> is 25%.</summary>
    public static Percentage FromPercent(decimal percent) => new(percent / 100m);

    ///<summary>Creates a percentage from a fraction, so <c>FromFraction(0.25m)</c> is 25%.</summary>
    public static Percentage FromFraction(decimal fraction) => new(fraction);

    ///<inheritdoc/>
    public int CompareTo(Percentage other) => Fraction.CompareTo(other.Fraction);

    ///<inheritdoc/>
    public bool Equals(Percentage other) => Fraction == other.Fraction;

    ///<inheritdoc/>
    public override bool Equals(object? obj) => (obj is Percentage other) && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => Fraction.GetHashCode();

    ///<summary>Returns the value followed by a percent sign, for example <c>25%</c>.</summary>
    public override string ToString() => ToString(null, CultureInfo.InvariantCulture);

    ///<summary>
    ///Formats the percentage. The <paramref name="format"/> is a standard or custom <see cref="decimal"/> format applied
    ///to the whole-percent value (so <c>"F1"</c> gives <c>12.5%</c>). <c>null</c>, empty or <c>"%"</c> gives up to two
    ///decimals, and <c>"P"</c> uses the culture's own percent formatting of the fraction.
    ///</summary>
    ///<param name="format">The format, as described above.</param>
    ///<param name="formatProvider">Supplies culture-specific symbols; defaults to the invariant culture.</param>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.InvariantCulture;

        return format switch
        {
            null or "" or "%" => Value.ToString("0.##", formatProvider) + "%",
            "P" or "p" => Fraction.ToString("P", formatProvider),
            _ => Value.ToString(format, formatProvider) + "%"
        };
    }
    #endregion

    #region Public properties
    ///<summary>Gets the underlying fraction, where 1 is 100%.</summary>
    public decimal Fraction { get; }

    ///<summary>Gets the value in whole-percent units, where 100 is 100%.</summary>
    public decimal Value => Fraction * 100m;
    #endregion
}
