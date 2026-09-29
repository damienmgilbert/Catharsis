using System.Globalization;

namespace Catharsis.Operators;

///<summary>
///A monetary amount tagged with an ISO 4217 currency code. Arithmetic between two amounts is only defined when both
///share a currency, so mixing currencies fails loudly instead of silently producing a meaningless number.
///</summary>
public readonly struct Money : IEquatable<Money>, IComparable<Money>
{
    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="Money"/>.
    ///</summary>
    ///<param name="amount">The amount.</param>
    ///<param name="currency">A three-letter currency code, such as <c>USD</c>. Stored upper-case.</param>
    ///<exception cref="ArgumentException"><paramref name="currency"/> is not exactly three letters.</exception>
    public Money(decimal amount, string currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        if(currency.Length != 3 || !currency.All(char.IsAsciiLetter))
        {
            throw new ArgumentException("Currency must be a three-letter code.", nameof(currency));
        }

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }
    #endregion

    #region Operators
    ///<summary>Adds two amounts of the same currency.</summary>
    ///<exception cref="InvalidOperationException">The currencies differ.</exception>
    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    ///<summary>Subtracts one amount from another of the same currency.</summary>
    ///<exception cref="InvalidOperationException">The currencies differ.</exception>
    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    ///<summary>Negates an amount.</summary>
    public static Money operator -(Money value) => new(-value.Amount, value.Currency);

    ///<summary>Scales an amount by a factor.</summary>
    public static Money operator *(Money value, decimal factor) => new(value.Amount * factor, value.Currency);

    ///<summary>Scales an amount by a factor.</summary>
    public static Money operator *(decimal factor, Money value) => value * factor;

    ///<summary>Divides an amount by a divisor.</summary>
    ///<exception cref="DivideByZeroException"><paramref name="divisor"/> is zero.</exception>
    public static Money operator /(Money value, decimal divisor) => new(value.Amount / divisor, value.Currency);

    ///<summary>Determines whether two amounts are equal.</summary>
    public static bool operator ==(Money left, Money right) => left.Equals(right);

    ///<summary>Determines whether two amounts differ.</summary>
    public static bool operator !=(Money left, Money right) => !left.Equals(right);

    ///<summary>Determines whether one amount is less than another of the same currency.</summary>
    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    ///<summary>Determines whether one amount is less than or equal to another of the same currency.</summary>
    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    ///<summary>Determines whether one amount is greater than another of the same currency.</summary>
    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    ///<summary>Determines whether one amount is greater than or equal to another of the same currency.</summary>
    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    ///<summary>Extracts the numeric amount, discarding the currency.</summary>
    public static explicit operator decimal(Money value) => value.Amount;
    #endregion

    #region Public methods
    ///<summary>
    ///Compares two amounts of the same currency.
    ///</summary>
    ///<exception cref="InvalidOperationException">The currencies differ.</exception>
    public int CompareTo(Money other)
    {
        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    ///<inheritdoc/>
    public bool Equals(Money other) => (Amount == other.Amount) && string.Equals(Currency, other.Currency, StringComparison.Ordinal);

    ///<inheritdoc/>
    public override bool Equals(object? obj) => (obj is Money other) && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    ///<summary>
    ///Rounds the amount to <paramref name="decimals"/> places using banker's rounding.
    ///</summary>
    public Money Round(int decimals = 2) => new(Math.Round(Amount, decimals, MidpointRounding.ToEven), Currency);

    ///<summary>
    ///Splits the amount into <paramref name="parts"/> pieces that sum exactly to the original, giving any leftover
    ///cents to the earliest pieces.
    ///</summary>
    ///<param name="parts">The number of pieces. Must be positive.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="parts"/> is not positive.</exception>
    public Money[] Allocate(int parts)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(parts);

        decimal share = Math.Round(Amount / parts, 2, MidpointRounding.ToZero);
        decimal remainder = Amount - (share * parts);
        decimal cent = Amount < 0 ? -0.01m : 0.01m;
        Money[] result = new Money[parts];

        for(int i = 0; i < parts; i++)
        {
            decimal extra = remainder != 0 ? cent : 0m;
            remainder -= extra;
            result[i] = new Money(share + extra, Currency);
        }

        return result;
    }

    ///<summary>
    ///Returns the amount with two decimals followed by the currency code, for example <c>12.50 USD</c>.
    ///</summary>
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Amount:F2} {Currency}");
    #endregion

    #region Public properties
    ///<summary>Gets the numeric amount.</summary>
    public decimal Amount { get; }

    ///<summary>Gets the upper-case three-letter currency code.</summary>
    public string Currency { get; }
    #endregion

    #region Private methods
    private static void EnsureSameCurrency(Money left, Money right)
    {
        if(!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Cannot combine {left.Currency} with {right.Currency}.");
        }
    }
    #endregion
}
