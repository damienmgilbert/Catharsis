using System.Globalization;

namespace Catharsis.Scheduling;

///<summary>
///Parses and evaluates a standard 5-field cron expression (minute hour day-of-month month day-of-week), computing the
///next occurrence after a given point in time. Supports <c>*</c>, single values, comma-separated lists, ranges (<c>a-
///b</c>), and step values (<c>*/n</c>, <c>a-b/n</c>). When both the day-of-month and day-of-week fields are restricted,
///either matching (the standard cron "OR" behavior) is enough for a day to match.
///</summary>
///<example>
public sealed class CronExpression
{
    #region Fields
    private readonly FieldMatcher _dayOfMonth;
    private readonly FieldMatcher _dayOfWeek;
    private readonly string _expression;
    private readonly FieldMatcher _hour;
    private readonly FieldMatcher _minute;
    private readonly FieldMatcher _month;
    #endregion

    #region Constructors
    private CronExpression(string expression, FieldMatcher minute, FieldMatcher hour, FieldMatcher dayOfMonth, FieldMatcher month, FieldMatcher dayOfWeek)
    {
        _expression = expression;
        _minute = minute;
        _hour = hour;
        _dayOfMonth = dayOfMonth;
        _month = month;
        _dayOfWeek = dayOfWeek;
    }
    #endregion

    #region Private methods
    private bool DayMatches(DateTime date)
    {
        bool domRestricted = !_dayOfMonth.IsWildcard;
        bool dowRestricted = !_dayOfWeek.IsWildcard;

        if(domRestricted && dowRestricted)
        {
            return _dayOfMonth.Matches(date.Day) || _dayOfWeek.Matches((int)date.DayOfWeek);
        }

        if(domRestricted)
        {
            return _dayOfMonth.Matches(date.Day);
        }

        if(dowRestricted)
        {
            return _dayOfWeek.Matches((int)date.DayOfWeek);
        }

        return true;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Computes the next occurrence strictly after the specified point in time, to minute resolution.
    ///</summary>
    ///<param name="after">The point in time to search from.</param>
    ///<returns>The next matching occurrence.</returns>
    ///<exception cref="InvalidOperationException">No matching occurrence was found within 5 years of <paramref name="after"/>.</exception>
    public DateTime GetNextOccurrence(DateTime after)
    {
        DateTime candidate = new DateTime(after.Year, after.Month, after.Day, after.Hour, after.Minute, 0, after.Kind).AddMinutes(1);
        DateTime limit = candidate.AddYears(5);

        while(candidate <= limit)
        {
            if(_month.Matches(candidate.Month) && DayMatches(candidate) && _hour.Matches(candidate.Hour) && _minute.Matches(candidate.Minute))
            {
                return candidate;
            }

            candidate = candidate.AddMinutes(1);
        }

        throw new InvalidOperationException("No matching occurrence was found within 5 years of the specified time.");
    }

    ///<summary>
    ///Computes the specified number of consecutive occurrences after the given point in time.
    ///</summary>
    ///<param name="after">The point in time to search from.</param>
    ///<param name="count">The number of occurrences to compute.</param>
    ///<returns>The matching occurrences, in chronological order.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public IEnumerable<DateTime> GetNextOccurrences(DateTime after, int count)
    {
        if(count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");
        }

        DateTime current = after;

        for(int i = 0; i < count; i++)
        {
            current = GetNextOccurrence(current);
            yield return current;
        }
    }

    ///<summary>
    ///Parses the specified 5-field cron expression.
    ///</summary>
    ///<param name="expression">The cron expression, as five space-separated fields.</param>
    ///<returns>The parsed expression.</returns>
    ///<exception cref="ArgumentException"><paramref name="expression"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="FormatException"><paramref name="expression"/> is not a valid 5-field cron expression.</exception>
    public static CronExpression Parse(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        string[] fields = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if(fields.Length != 5)
        {
            throw new FormatException("A cron expression must have exactly 5 space-separated fields (minute hour day-of-month month day-of-week).");
        }

        FieldMatcher minute = FieldMatcher.Parse(fields[0], 0, 59);
        FieldMatcher hour = FieldMatcher.Parse(fields[1], 0, 23);
        FieldMatcher dayOfMonth = FieldMatcher.Parse(fields[2], 1, 31);
        FieldMatcher month = FieldMatcher.Parse(fields[3], 1, 12);
        FieldMatcher dayOfWeek = FieldMatcher.Parse(fields[4], 0, 6);

        return new CronExpression(expression, minute, hour, dayOfMonth, month, dayOfWeek);
    }

    ///<inheritdoc/>
    public override string ToString() => _expression;

    ///<summary>
    ///Attempts to parse the specified 5-field cron expression.
    ///</summary>
    ///<param name="expression">The cron expression, as five space-separated fields.</param>
    ///<param name="result">The parsed expression, if parsing succeeded.</param>
    ///<returns><c>true</c> if the expression was parsed successfully; otherwise <c>false</c>.</returns>
    public static bool TryParse(string? expression, out CronExpression? result)
    {
        if(string.IsNullOrWhiteSpace(expression))
        {
            result = null;
            return false;
        }

        try
        {
            result = Parse(expression);
            return true;
        } catch(FormatException)
        {
            result = null;
            return false;
        }
    }
    #endregion

    private sealed class FieldMatcher
    {
        #region Fields
        private readonly bool[] _allowed;
        private readonly int _min;
        #endregion

        #region Constructors
        private FieldMatcher(bool[] allowed, int min, bool isWildcard)
        {
            _allowed = allowed;
            _min = min;
            IsWildcard = isWildcard;
        }
        #endregion

        #region Private methods
        private static void ParsePart(string part, int min, int max, bool[] allowed)
        {
            int step = 1;
            string rangePart = part;

            int slashIndex = part.IndexOf('/', StringComparison.Ordinal);

            if(slashIndex >= 0)
            {
                rangePart = part[..slashIndex];

                if(!int.TryParse(part[(slashIndex + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out step) || step < 1)
                {
                    throw new FormatException($"Invalid step value in cron field part '{part}'.");
                }
            }

            int rangeStart;
            int rangeEnd;

            if(rangePart == "*")
            {
                rangeStart = min;
                rangeEnd = max;
            } else
            {
                int dashIndex = rangePart.IndexOf('-', StringComparison.Ordinal);

                if(dashIndex >= 0)
                {
                    if(!int.TryParse(rangePart[..dashIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out rangeStart) || !int.TryParse(rangePart[(dashIndex + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out rangeEnd))
                    {
                        throw new FormatException($"Invalid range in cron field part '{part}'.");
                    }
                } else
                {
                    if(!int.TryParse(rangePart, NumberStyles.Integer, CultureInfo.InvariantCulture, out rangeStart))
                    {
                        throw new FormatException($"Invalid value in cron field part '{part}'.");
                    }

                    rangeEnd = rangeStart;
                }
            }

            if(rangeStart < min || rangeEnd > max || rangeStart > rangeEnd)
            {
                throw new FormatException($"Cron field part '{part}' is out of the valid range [{min}, {max}].");
            }

            for(int value = rangeStart; value <= rangeEnd; value += step)
            {
                allowed[value - min] = true;
            }
        }
        #endregion

        #region Public methods
        public bool Matches(int value) => _allowed[value - _min];

        public static FieldMatcher Parse(string field, int min, int max)
        {
            bool[] allowed = new bool[max - min + 1];

            foreach(string part in field.Split(','))
            {
                ParsePart(part, min, max, allowed);
            }

            return new FieldMatcher(allowed, min, field == "*");
        }
        #endregion

        #region Public properties
        public bool IsWildcard { get; }
        #endregion
    }
}
