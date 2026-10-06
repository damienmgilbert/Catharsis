namespace Catharsis.Time;

///<summary>
///Represents a closed, inclusive range [<see cref="Start"/>, <see cref="End"/>] of <see cref="DateOnly"/> values.
///Complements the generic <see cref="Catharsis.DataStructures.Interval{T}"/> with date-specific conveniences such as
///<see cref="DayCount"/> and <see cref="ToDates"/>.
///</summary>
public readonly struct DateOnlyRange : IEquatable<DateOnlyRange>
{
    #region Constructors

    ///<summary>
    ///Initializes a new <see cref="DateOnlyRange"/> with the specified bounds. If <paramref name="start"/> is later
    ///than <paramref name="end"/>, the values are swapped so the range is always well-formed.
    ///</summary>
    ///<param name="start">One bound of the range.</param>
    ///<param name="end">The other bound of the range.</param>
    public DateOnlyRange(DateOnly start, DateOnly end)
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
    ///Determines whether two ranges are not equal.
    ///</summary>
    public static bool operator !=(DateOnlyRange left, DateOnlyRange right)
    {
        return !left.Equals(right);
    }

    ///<summary>
    ///Determines whether two ranges are equal.
    ///</summary>
    public static bool operator ==(DateOnlyRange left, DateOnlyRange right)
    {
        return left.Equals(right);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Determines whether the range contains the specified date.
    ///</summary>
    ///<param name="value">The date to test.</param>
    ///<returns>
    ///<c>true</c> if <paramref name="value"/> is within [<see cref="Start"/>, <see cref="End"/>]; otherwise
    ///<c>false</c>.
    ///</returns>
    public bool Contains(DateOnly value) => (Start <= value) && (value <= End);

    ///<inheritdoc/>
    public bool Equals(DateOnlyRange other) => (Start == other.Start) && (End == other.End);

    ///<inheritdoc/>
    public override bool Equals(object? obj) => (obj is DateOnlyRange other) && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Start, End);

    ///<summary>
    ///Returns the intersection of this range with <paramref name="other"/>, or <c>null</c> if they do not overlap.
    ///</summary>
    ///<param name="other">The other range.</param>
    ///<returns>The intersecting range, or <c>null</c> if no overlap exists.</returns>
    public DateOnlyRange? Intersect(DateOnlyRange other)
    {
        if(!Overlaps(other))
        {
            return null;
        }

        return new DateOnlyRange((Start >= other.Start) ? Start : other.Start, (End <= other.End) ? End : other.End);
    }

    ///<summary>
    ///Determines whether this range overlaps with <paramref name="other"/>.
    ///</summary>
    ///<param name="other">The other range.</param>
    ///<returns><c>true</c> if the ranges share at least one common date; otherwise <c>false</c>.</returns>
    public bool Overlaps(DateOnlyRange other) => (Start <= other.End) && (other.Start <= End);

    ///<summary>
    ///Enumerates every date in the range, from <see cref="Start"/> to <see cref="End"/> inclusive.
    ///</summary>
    ///<returns>A sequence of every date in the range.</returns>
    public IEnumerable<DateOnly> ToDates()
    {
        for(DateOnly date = Start; date <= End; date = date.AddDays(1))
        {
            yield return date;
        }
    }

    ///<inheritdoc/>
    public override string ToString() => $"[{Start}, {End}]";

    ///<summary>
    ///Returns the smallest range that covers both this range and <paramref name="other"/>.
    ///</summary>
    ///<param name="other">The other range.</param>
    ///<returns>A new range spanning both ranges.</returns>
    public DateOnlyRange Union(DateOnlyRange other) => new((Start <= other.Start) ? Start : other.Start, (End >= other.End) ? End : other.End);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of dates in the range, counting both <see cref="Start"/> and <see cref="End"/>.
    ///</summary>
    public int DayCount => (End.DayNumber - Start.DayNumber) + 1;

    ///<summary>
    ///Gets the inclusive end date of the range.
    ///</summary>
    public DateOnly End { get; }

    ///<summary>
    ///Gets the inclusive start date of the range.
    ///</summary>
    public DateOnly Start { get; }
    #endregion
}
