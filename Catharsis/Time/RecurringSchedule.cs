namespace Catharsis.Time;

///<summary>
///Describes a weekly recurrence such as "every Monday at 9am local time" and computes its next occurrence,
///correctly accounting for the target time zone's UTC offset (including daylight saving transitions) at the
///specific future date being computed, not just at the moment of the call.
///</summary>
///<param name="dayOfWeek">The day of the week the recurrence falls on.</param>
///<param name="timeOfDay">The local time of day the recurrence falls at.</param>
///<param name="timeZone">The time zone that <paramref name="timeOfDay"/> is expressed in.</param>
///<exception cref="ArgumentNullException"><paramref name="timeZone"/> is <c>null</c>.</exception>
public sealed class RecurringSchedule(DayOfWeek dayOfWeek, TimeOnly timeOfDay, TimeZoneInfo timeZone)
{
    #region Fields
    readonly TimeZoneInfo _timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone), "Time zone must not be null.");
    #endregion

    #region Public methods
    ///<summary>
    ///Computes the next occurrence of this recurrence strictly after <paramref name="after"/>.
    ///</summary>
    ///<param name="after">The point in time to search after.</param>
    ///<returns>The next occurrence, expressed with the configured time zone's UTC offset at that instant.</returns>
    public DateTimeOffset GetNextOccurrence(DateTimeOffset after)
    {
        DateTime localAfter = TimeZoneInfo.ConvertTime(after, _timeZone).DateTime;
        DateOnly candidateDate = DateOnly.FromDateTime(localAfter);

        int daysUntil = ((int)DayOfWeek - (int)candidateDate.DayOfWeek + 7) % 7;
        candidateDate = candidateDate.AddDays(daysUntil);
        DateTime candidateLocal = candidateDate.ToDateTime(TimeOfDay);

        if (candidateLocal <= localAfter)
        {
            candidateDate = candidateDate.AddDays(7);
            candidateLocal = candidateDate.ToDateTime(TimeOfDay);
        }

        TimeSpan offset = _timeZone.GetUtcOffset(DateTime.SpecifyKind(candidateLocal, DateTimeKind.Unspecified));
        return new DateTimeOffset(candidateLocal, offset);
    }

    ///<inheritdoc/>
    public override string ToString() => $"Every {DayOfWeek} at {TimeOfDay} ({_timeZone.Id})";
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the day of the week the recurrence falls on.
    ///</summary>
    public DayOfWeek DayOfWeek { get; } = dayOfWeek;

    ///<summary>
    ///Gets the local time of day the recurrence falls at.
    ///</summary>
    public TimeOnly TimeOfDay { get; } = timeOfDay;

    ///<summary>
    ///Gets the time zone that <see cref="TimeOfDay"/> is expressed in.
    ///</summary>
    public TimeZoneInfo TimeZone => _timeZone;
    #endregion
}
