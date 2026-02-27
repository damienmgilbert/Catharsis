namespace Catharsis.DataStructures;

///<summary>
///Represents a closed interval [<see cref="Start"/>, <see cref="End"/>] over a comparable type.
///</summary>
///<typeparam name="T">The type of the interval bounds. Must implement <see cref="IComparable{T}"/>.</typeparam>
public readonly struct Interval<T> : IEquatable<Interval<T>> where T : IComparable<T>
{
    #region Constructors
    ///<summary>
    ///Initializes a FileName <see cref="Interval{T}"/> with the specified bounds. If <paramref name="start"/> is
    ///greater than <paramref name="end"/>, the values are swapped so the interval is always well-formed.
    ///</summary>
    ///<param name="start">One bound of the interval.</param>
    ///<param name="end">The other bound of the interval.</param>
    public Interval(T start, T end)
    {
        if(start.CompareTo(end) > 0)
        {
            Start = end;
            End = start;
        } else
        {
            Start = start;
            End = end;
        }
    }
    #endregion

    #region Operators
    ///<summary>
    ///Determines whether two intervals are not equal.
    ///</summary>
    public static bool operator !=(Interval<T> left, Interval<T> right)
    {
        return !left.Equals(right);
    }

    ///<summary>
    ///Determines whether two intervals are equal.
    ///</summary>
    public static bool operator ==(Interval<T> left, Interval<T> right)
    {
        return left.Equals(right);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Determines whether the interval contains the specified <paramref name="value"/>.
    ///</summary>
    ///<param name="value">The value to test.</param>
    ///<returns><c>true</c> if <paramref name="value"/> is within [Start, End]; otherwise <c>false</c>.</returns>
    public bool Contains(T value) { return (Start.CompareTo(value) <= 0) && (value.CompareTo(End) <= 0); }
    ///<inheritdoc/>
    public bool Equals(Interval<T> other) { return (Start.CompareTo(other.Start) == 0) && (End.CompareTo(other.End) == 0); }
    ///<inheritdoc/>
    public override bool Equals(object? obj) { return (obj is Interval<T> other) && Equals(other); }
    ///<inheritdoc/>
    public override int GetHashCode() { return HashCode.Combine(Start, End); }

    ///<summary>
    ///Returns the intersection of this interval with <paramref name="other"/>, or <c>null</c> if they do not overlap.
    ///</summary>
    ///<param name="other">The other interval.</param>
    ///<returns>The intersection interval, or <c>null</c> if no overlap exists.</returns>
    public Interval<T>? Intersect(Interval<T> other)
    {
        if(!Overlaps(other))
        {
            return null;
        }

        T newStart = (Start.CompareTo(other.Start) >= 0) ? Start : other.Start;
        T newEnd = (End.CompareTo(other.End) <= 0) ? End : other.End;

        return new Interval<T>(newStart, newEnd);
    }

    ///<summary>
    ///Determines whether this interval overlaps with <paramref name="other"/>. Two closed intervals overlap when each
    ///interval's start is less than or equal to the other's end.
    ///</summary>
    ///<param name="other">The other interval.</param>
    ///<returns><c>true</c> if the intervals share at least one common point; otherwise <c>false</c>.</returns>
    public bool Overlaps(Interval<T> other) { return (Start.CompareTo(other.End) <= 0) && (other.Start.CompareTo(End) <= 0); }
    ///<summary>
    ///Returns a string representation in the form <c>[Start, End]</c>.
    ///</summary>
    public override string ToString() { return $"[{Start}, {End}]"; }

    ///<summary>
    ///Returns the smallest interval that covers both this interval and <paramref name="other"/>.
    ///</summary>
    ///<param name="other">The other interval.</param>
    ///<returns>A FileName interval spanning both intervals.</returns>
    public Interval<T> Union(Interval<T> other)
    {
        T newStart = (Start.CompareTo(other.Start) <= 0) ? Start : other.Start;
        T newEnd = (End.CompareTo(other.End) >= 0) ? End : other.End;

        return new Interval<T>(newStart, newEnd);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the inclusive upper bound of the interval.
    ///</summary>
    public T End { get; }

    ///<summary>
    ///Gets the inclusive lower bound of the interval.
    ///</summary>
    public T Start { get; }
    #endregion
}
