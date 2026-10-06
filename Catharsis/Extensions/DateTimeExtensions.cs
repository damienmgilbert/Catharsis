namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for <see cref="DateTime"/>: period boundaries and simple weekend-skipping business-day
///arithmetic. For holiday-aware scheduling, compose with a calendar of your own; this type has no notion of
///holidays.
///</summary>
public static class DateTimeExtensions
{
    #region Public methods

    ///<summary>
    ///Adds the specified number of business days (Monday-Friday), skipping weekends.
    ///</summary>
    ///<param name="value">The starting date.</param>
    ///<param name="days">The number of business days to add. May be negative to go backward.</param>
    ///<returns>The resulting date.</returns>
    public static DateTime AddBusinessDays(this DateTime value, int days)
    {
        int direction = Math.Sign(days);

        if (direction == 0)
        {
            return value;
        }

        DateTime result = value;
        int remaining = Math.Abs(days);

        while (remaining > 0)
        {
            result = result.AddDays(direction);

            if (!result.IsWeekend())
            {
                remaining--;
            }
        }

        return result;
    }

    ///<summary>
    ///Returns the last moment of the day (23:59:59.9999999) for the specified date.
    ///</summary>
    ///<param name="value">The source date.</param>
    ///<returns>The end of the day, preserving <see cref="DateTime.Kind"/>.</returns>
    public static DateTime EndOfDay(this DateTime value) => value.StartOfDay().AddDays(1).AddTicks(-1);

    ///<summary>
    ///Returns the last moment of the month for the specified date.
    ///</summary>
    ///<param name="value">The source date.</param>
    ///<returns>The end of the month, preserving <see cref="DateTime.Kind"/>.</returns>
    public static DateTime EndOfMonth(this DateTime value) => value.StartOfMonth().AddMonths(1).AddTicks(-1);

    ///<summary>
    ///Determines whether the date falls on a Saturday or Sunday.
    ///</summary>
    ///<param name="value">The date to test.</param>
    ///<returns><c>true</c> if the date is a Saturday or Sunday; otherwise <c>false</c>.</returns>
    public static bool IsWeekend(this DateTime value) => value.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    ///<summary>
    ///Returns midnight (00:00:00) on the same date.
    ///</summary>
    ///<param name="value">The source date.</param>
    ///<returns>The start of the day, preserving <see cref="DateTime.Kind"/>.</returns>
    public static DateTime StartOfDay(this DateTime value) => new(value.Year, value.Month, value.Day, 0, 0, 0, value.Kind);

    ///<summary>
    ///Returns the first day of the month at midnight.
    ///</summary>
    ///<param name="value">The source date.</param>
    ///<returns>The start of the month, preserving <see cref="DateTime.Kind"/>.</returns>
    public static DateTime StartOfMonth(this DateTime value) => new(value.Year, value.Month, 1, 0, 0, 0, value.Kind);

    ///<summary>
    ///Returns midnight on the first day of the week containing the specified date.
    ///</summary>
    ///<param name="value">The source date.</param>
    ///<param name="startOfWeek">Which day is considered the start of the week. Defaults to <see cref="DayOfWeek.Monday"/>.</param>
    ///<returns>The start of the week, preserving <see cref="DateTime.Kind"/>.</returns>
    public static DateTime StartOfWeek(this DateTime value, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        DateTime midnight = value.StartOfDay();
        int diff = ((int)midnight.DayOfWeek - (int)startOfWeek + 7) % 7;
        return midnight.AddDays(-diff);
    }
    #endregion
}
