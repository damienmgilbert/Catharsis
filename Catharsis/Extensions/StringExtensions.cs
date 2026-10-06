using System.Globalization;
using System.Text;

namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for <see cref="string"/> beyond what <see cref="Catharsis.Text.RegularExpressions"/>
///covers: truncation, slugification, and whitespace-aware fallback.
///</summary>
public static class StringExtensions
{
    #region Public methods

    ///<summary>
    ///Returns <paramref name="value"/> with its first character converted to uppercase, leaving the rest unchanged.
    ///</summary>
    ///<param name="value">The string to capitalize.</param>
    ///<returns>The capitalized string, or <paramref name="value"/> unchanged if it is empty.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string Capitalize(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Value must not be null.");
        }

        return value.Length == 0
            ? value
            : string.Create(value.Length, value, static (span, source) =>
            {
                span[0] = char.ToUpper(source[0], CultureInfo.CurrentCulture);
                source.AsSpan(1).CopyTo(span[1..]);
            });
    }

    ///<summary>
    ///Returns <paramref name="value"/> if it is not <c>null</c> or all whitespace; otherwise <paramref
    ///name="fallback"/>.
    ///</summary>
    ///<param name="value">The candidate string.</param>
    ///<param name="fallback">The value to return when <paramref name="value"/> is null or whitespace.</param>
    ///<returns><paramref name="value"/> or <paramref name="fallback"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="fallback"/> is <c>null</c>.</exception>
    public static string OrDefault(this string? value, string fallback)
    {
        if (fallback is null)
        {
            throw new ArgumentNullException(nameof(fallback), "Fallback must not be null.");
        }

        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    ///<summary>
    ///Converts <paramref name="value"/> into a lowercase, hyphen-separated slug suitable for URLs: runs of
    ///non-alphanumeric characters become a single hyphen, and leading/trailing hyphens are trimmed.
    ///</summary>
    ///<param name="value">The string to slugify.</param>
    ///<returns>The slugified string.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string Slugify(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Value must not be null.");
        }

        StringBuilder builder = new(value.Length);
        bool lastWasHyphen = true;

        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(char.ToLowerInvariant(c));
                lastWasHyphen = false;
            }
            else if (!lastWasHyphen)
            {
                builder.Append('-');
                lastWasHyphen = true;
            }
        }

        if (builder.Length > 0 && builder[^1] == '-')
        {
            builder.Length--;
        }

        return builder.ToString();
    }

    ///<summary>
    ///Truncates <paramref name="value"/> to at most <paramref name="maxLength"/> characters, appending <paramref
    ///name="ellipsis"/> when truncation occurs. The returned string, including the ellipsis, never exceeds
    ///<paramref name="maxLength"/> characters.
    ///</summary>
    ///<param name="value">The string to truncate.</param>
    ///<param name="maxLength">The maximum length of the result, including the ellipsis.</param>
    ///<param name="ellipsis">The suffix appended when truncation occurs. Defaults to <c>"..."</c>.</param>
    ///<returns><paramref name="value"/> unchanged if it already fits; otherwise a truncated, ellipsis-suffixed string.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="ellipsis"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative or shorter than <paramref name="ellipsis"/>.</exception>
    public static string Truncate(this string value, int maxLength, string ellipsis = "...")
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Value must not be null.");
        }

        if (ellipsis is null)
        {
            throw new ArgumentNullException(nameof(ellipsis), "Ellipsis must not be null.");
        }

        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Maximum length must not be negative.");
        }

        if (value.Length <= maxLength)
        {
            return value;
        }

        if (maxLength < ellipsis.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Maximum length must be at least as long as the ellipsis.");
        }

        return string.Concat(value.AsSpan(0, maxLength - ellipsis.Length), ellipsis);
    }
    #endregion
}
