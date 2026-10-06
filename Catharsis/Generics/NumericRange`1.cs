using System.Numerics;

namespace Catharsis.Generics;

///<summary>
///A closed range <c>[Start, End]</c> over any numeric type, built on generic math so the same code serves ///<see
///cref="int"/>, <see cref="double"/>, <see cref="decimal"/> and user-defined numbers. Unlike ///<see
///cref="Catharsis.DataStructures.Interval{T}"/>, which is a general comparable interval, this type can also measure and
///step through its values.
///</summary>
///<typeparam name="T">The numeric type.</typeparam>
public readonly struct NumericRange<T> : IEquatable<NumericRange<T>> where T : INumber<T>
{
    #region Constructors

    ///<summary>
    ///Creates a range.
    ///</summary>
    ///<param name="start">The lower bound, inclusive.</param>
    ///<param name="end">The upper bound, inclusive. Must not be less than <paramref name="start"/>.</param>
    ///<exception cref="ArgumentException"><paramref name="end"/> is less than <paramref name="start"/>.</exception>
    public NumericRange(T start, T end)
    {
        if(end < start)
        {
            throw new ArgumentException("End must not be less than start.", nameof(end));
        }

        Start = start;
        End = end;
    }
    #endregion

    #region Operators
    ///<summary>
    ///Determines whether two ranges differ.
    ///</summary>
    public static bool operator !=(NumericRange<T> left, NumericRange<T> right)
    {
        return !left.Equals(right);
    }

    ///<summary>
    ///Determines whether two ranges are equal.
    ///</summary>
    public static bool operator ==(NumericRange<T> left, NumericRange<T> right)
    {
        return left.Equals(right);
    }
    #endregion

    #region Private methods
    private IEnumerable<T> StepIterator(T step)
    {
        for(T current = Start; current <= End; current += step)
        {
            yield return current;
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Limits <paramref name="value"/> to the range.
    ///</summary>
    public T Clamp(T value) => T.Clamp(value, Start, End);

        ///<summary>
///Determines whether <paramref name="value"/> lies within the range.
///</summary>
    public bool Contains(T value) => value >= Start && value <= End;

    ///<summary>
    ///Determines whether <paramref name="other"/> lies entirely within this range.
    ///</summary>
    public bool Contains(NumericRange<T> other) => other.Start >= Start && other.End <= End;

    ///<inheritdoc/>
    public bool Equals(NumericRange<T> other) => Start == other.Start && End == other.End;

    ///<inheritdoc/>
    public override bool Equals(object? obj) => (obj is NumericRange<T> other) && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Start, End);

    ///<summary>
    ///Determines whether the two ranges share at least one value.
    ///</summary>
    public bool Overlaps(NumericRange<T> other) => Start <= other.End && other.Start <= End;

    ///<summary>
    ///Maps <paramref name="value"/> from this range onto <paramref name="target"/> proportionally.
    ///</summary>
    ///<param name="value">A value within this range.</param>
    ///<param name="target">The range to map onto.</param>
    ///<exception cref="InvalidOperationException">This range has zero length.</exception>
    public T Remap(T value, NumericRange<T> target)
    {
        if(Length == T.Zero)
        {
            throw new InvalidOperationException("Cannot remap from a zero-length range.");
        }

        return target.Start + ((value - Start) * target.Length / Length);
    }

    ///<summary>
    ///Gets the smallest range covering both this range and <paramref name="other"/>.
    ///</summary>
    public NumericRange<T> Span(NumericRange<T> other) => new(T.Min(Start, other.Start), T.Max(End, other.End));

    ///<summary>
    ///Enumerates the range from <see cref="Start"/> in increments of <paramref name="step"/>.
    ///</summary>
    ///<param name="step">The increment. Must be positive.</param>
    ///<returns>Each value not exceeding <see cref="End"/>.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="step"/> is not positive.</exception>
    public IEnumerable<T> Step(T step)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(step, T.Zero);

        return StepIterator(step);
    }

    ///<inheritdoc/>
    public override string ToString() => $"[{Start}, {End}]";

    ///<summary>
    ///Gets the values the two ranges share.
    ///</summary>
    ///<param name="other">The range to intersect with.</param>
    ///<param name="result">The shared range, when there is one.</param>
    ///<returns><c>true</c> if the ranges overlap.</returns>
    public bool TryIntersect(NumericRange<T> other, out NumericRange<T> result)
    {
        if(!Overlaps(other))
        {
            result = default;
            return false;
        }

        result = new NumericRange<T>(T.Max(Start, other.Start), T.Min(End, other.End));
        return true;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the upper bound, inclusive.
    ///</summary>
    public T End { get; }

    ///<summary>
    ///Gets <c>End - Start</c>.
    ///</summary>
    public T Length => End - Start;

        ///<summary>
///Gets the lower bound, inclusive.
///</summary>
    public T Start { get; }
    #endregion
}
