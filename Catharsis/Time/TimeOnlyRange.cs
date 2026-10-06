namespace Catharsis.Time;

///<summary>
///Represents a closed, inclusive range [<see cref="Start"/>, <see cref="End"/>] of <see cref="TimeOnly"/> values
///within a single day. This does not model a range that wraps past midnight; <paramref name="start"/> and
///<paramref name="end"/> are simply ordered like any other comparable bound.
///</summary>
public readonly struct TimeOnlyRange : IEquatable<TimeOnlyRange>
{
    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="TimeOnlyRange"/> with the specified bounds. If <paramref name="start"/> is later
    ///than <paramref name="end"/>, the values are swapped so the range is always well-formed.
    ///</summary>
    ///<param name="start">One bound of the range.</param>
    ///<param name="end">The other bound of the range.</param>
    public TimeOnlyRange(TimeOnly start, TimeOnly end)
    {
        if(start > end)
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
    ///Determines whether two ranges are equal.
    ///</summary>
    public static bool operator ==(TimeOnlyRange left, TimeOnlyRange right) => left.Equals(right);

    ///<summary>
    ///Determines whether two ranges are not equal.
    ///</summary>
    public static bool operator !=(TimeOnlyRange left, TimeOnlyRange right) => !left.Equals(right);
    #endregion

    #region Public methods
    ///<summary>
    ///Determines whether the range contains the specified time.
    ///</summary>
    ///<param name="value">The time to test.</param>
    ///<returns><c>true</c> if <paramref name="value"/> is within [<see cref="Start"/>, <see cref="End"/>]; otherwise <c>false</c>.</returns>
    public bool Contains(TimeOnly value) => (Start <= value) && (value <= End);

    ///<inheritdoc/>
    public bool Equals(TimeOnlyRange other) => (Start == other.Start) && (End == other.End);

    ///<inheritdoc/>
    public override bool Equals(object? obj) => (obj is TimeOnlyRange other) && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Start, End);

    ///<summary>
    ///Returns the intersection of this range with <paramref name="other"/>, or <c>null</c> if they do not overlap.
    ///</summary>
    ///<param name="other">The other range.</param>
    ///<returns>The intersecting range, or <c>null</c> if no overlap exists.</returns>
    public TimeOnlyRange? Intersect(TimeOnlyRange other)
    {
        if(!Overlaps(other))
        {
            return null;
        }

        return new TimeOnlyRange((Start >= other.Start) ? Start : other.Start, (End <= other.End) ? End : other.End);
    }

    ///<summary>
    ///Determines whether this range overlaps with <paramref name="other"/>.
    ///</summary>
    ///<param name="other">The other range.</param>
    ///<returns><c>true</c> if the ranges share at least one common instant; otherwise <c>false</c>.</returns>
    public bool Overlaps(TimeOnlyRange other) => (Start <= other.End) && (other.Start <= End);

    ///<inheritdoc/>
    public override string ToString() => $"[{Start}, {End}]";

    ///<summary>
    ///Returns the smallest range that covers both this range and <paramref name="other"/>.
    ///</summary>
    ///<param name="other">The other range.</param>
    ///<returns>A new range spanning both ranges.</returns>
    public TimeOnlyRange Union(TimeOnlyRange other) => new((Start <= other.Start) ? Start : other.Start, (End >= other.End) ? End : other.End);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the duration between <see cref="Start"/> and <see cref="End"/>.
    ///</summary>
    public TimeSpan Duration => End - Start;

    ///<summary>
    ///Gets the inclusive end time of the range.
    ///</summary>
    public TimeOnly End { get; }

    ///<summary>
    ///Gets the inclusive start time of the range.
    ///</summary>
    public TimeOnly Start { get; }
    #endregion
}
