using System.Globalization;
using System.Text;

namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for <see cref="TimeSpan"/>: clamping to a range and formatting as a short
///human-readable duration.
///</summary>
public static class TimeSpanExtensions
{
    #region Public methods

    ///<summary>
    ///Clamps the value to the inclusive range [<paramref name="min"/>, <paramref name="max"/>].
    ///</summary>
    ///<param name="value">The value to clamp.</param>
    ///<param name="min">The inclusive lower bound.</param>
    ///<param name="max">The inclusive upper bound.</param>
    ///<returns>The clamped value.</returns>
    ///<exception cref="ArgumentException"><paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public static TimeSpan Clamp(this TimeSpan value, TimeSpan min, TimeSpan max)
    {
        if (min > max)
        {
            throw new ArgumentException("Minimum must not be greater than maximum.", nameof(min));
        }

        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }

    ///<summary>
    ///Formats the duration as a short human-readable string using its two largest non-zero units, e.g.
    ///<c>"2h 15m"</c> or <c>"3d 4h"</c>. A zero duration formats as <c>"0s"</c>.
    ///</summary>
    ///<param name="value">The duration to format.</param>
    ///<returns>The formatted duration.</returns>
    public static string ToHumanReadable(this TimeSpan value)
    {
        TimeSpan absolute = value.Duration();

        (int Amount, string Unit)[] parts =
        [
            ((int)absolute.TotalDays, "d"),
            (absolute.Hours, "h"),
            (absolute.Minutes, "m"),
            (absolute.Seconds, "s"),
        ];

        StringBuilder builder = new();
        int used = 0;

        foreach ((int amount, string unit) in parts)
        {
            if (amount == 0)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(amount.ToString(CultureInfo.InvariantCulture)).Append(unit);
            used++;

            if (used == 2)
            {
                break;
            }
        }

        if (value < TimeSpan.Zero && builder.Length > 0)
        {
            builder.Insert(0, '-');
        }

        return builder.Length > 0 ? builder.ToString() : "0s";
    }
    #endregion
}
