namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for <see cref="ReadOnlySpan{T}"/> and <see cref="Span{T}"/> that fill small gaps left
///between the base class library and <c>CommunityToolkit.HighPerformance</c>.
///</summary>
public static class SpanExtensions
{
    #region Public methods

    ///<summary>
    ///Determines whether the span contains any of the specified values as a substring, using ordinal comparison.
    ///</summary>
    ///<param name="span">The span to search.</param>
    ///<param name="values">The candidate substrings to look for.</param>
    ///<returns>
    ///<c>true</c> if any value in <paramref name="values"/> occurs within <paramref name="span"/>; otherwise
    ///<c>false</c>.
    ///</returns>
    ///<exception cref="ArgumentNullException"><paramref name="values"/> is <c>null</c>.</exception>
    public static bool ContainsAnyOf(this ReadOnlySpan<char> span, params string[] values)
    {
        if(values is null)
        {
            throw new ArgumentNullException(nameof(values), "Values must not be null.");
        }

        foreach(string value in values)
        {
            if(span.Contains(value, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    ///<summary>
    ///Counts the number of times <paramref name="value"/> occurs in the span.
    ///</summary>
    ///<param name="span">The span to search.</param>
    ///<param name="value">The character to count.</param>
    ///<returns>The number of occurrences.</returns>
    public static int CountOccurrences(this ReadOnlySpan<char> span, char value)
    {
        int count = 0;

        foreach(char c in span)
        {
            if(c == value)
            {
                count++;
            }
        }

        return count;
    }

    ///<summary>
    ///Returns a slice of at most <paramref name="maxLength"/> characters from the start of the span.
    ///</summary>
    ///<param name="span">The span to truncate.</param>
    ///<param name="maxLength">The maximum number of characters to keep. Must not be negative.</param>
    ///<returns>
    ///<paramref name="span"/> unchanged if it is already within <paramref name="maxLength"/>; otherwise a truncated
    ///slice.
    ///</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative.</exception>
    public static ReadOnlySpan<char> Truncate(this ReadOnlySpan<char> span, int maxLength)
    {
        if(maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Maximum length must not be negative.");
        }

        return span.Length <= maxLength ? span : span[..maxLength];
    }
    #endregion
}
