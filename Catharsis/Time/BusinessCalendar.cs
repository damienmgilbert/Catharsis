namespace Catharsis.Time;

///<summary>
///Performs holiday-aware business-day arithmetic against a configurable set of holidays and weekend days.
///</summary>
///<remarks>
///This differs from <see cref="Catharsis.Extensions.DateTimeExtensions.AddBusinessDays"/>, which only skips
///Saturday and Sunday with no concept of holidays: use that extension for a quick Monday-to-Friday calculation, and
///this type when specific holidays or non-standard weekends (e.g. Friday/Saturday) need to be honored.
///</remarks>
///<param name="holidays">The dates treated as holidays (non-business days), or <c>null</c> for none.</param>
///<param name="weekendDays">The days of the week treated as weekends, or <c>null</c> for the default of Saturday and Sunday.</param>
public sealed class BusinessCalendar(IEnumerable<DateOnly>? holidays = null, IEnumerable<DayOfWeek>? weekendDays = null)
{
    #region Fields
    readonly HashSet<DateOnly> _holidays = [.. holidays ?? []];
    readonly HashSet<DayOfWeek> _weekendDays = [.. weekendDays ?? [DayOfWeek.Saturday, DayOfWeek.Sunday]];
    #endregion

    #region Public methods
    ///<summary>
    ///Adds the specified number of business days to <paramref name="date"/>, skipping weekends and holidays.
    ///</summary>
    ///<param name="date">The starting date.</param>
    ///<param name="businessDays">The number of business days to add. May be negative to count backward.</param>
    ///<returns>The resulting business date.</returns>
    public DateOnly AddBusinessDays(DateOnly date, int businessDays)
    {
        int direction = Math.Sign(businessDays);

        if (direction == 0)
        {
            return date;
        }

        int remaining = Math.Abs(businessDays);
        DateOnly current = date;

        while (remaining > 0)
        {
            current = current.AddDays(direction);

            if (IsBusinessDay(current))
            {
                remaining--;
            }
        }

        return current;
    }

    ///<summary>
    ///Counts the number of business days in the inclusive range between <paramref name="start"/> and
    ///<paramref name="end"/>, in either order.
    ///</summary>
    ///<param name="start">One bound of the range.</param>
    ///<param name="end">The other bound of the range.</param>
    ///<returns>The number of business days in the range.</returns>
    public int CountBusinessDays(DateOnly start, DateOnly end)
    {
        DateOnly rangeStart = (start <= end) ? start : end;
        DateOnly rangeEnd = (start <= end) ? end : start;

        int count = 0;

        for (DateOnly date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
        {
            if (IsBusinessDay(date))
            {
                count++;
            }
        }

        return count;
    }

    ///<summary>
    ///Determines whether the specified date is a business day: not a configured weekend day and not a configured
    ///holiday.
    ///</summary>
    ///<param name="date">The date to test.</param>
    ///<returns><c>true</c> if <paramref name="date"/> is a business day; otherwise <c>false</c>.</returns>
    public bool IsBusinessDay(DateOnly date) => !_weekendDays.Contains(date.DayOfWeek) && !_holidays.Contains(date);
    #endregion
}
