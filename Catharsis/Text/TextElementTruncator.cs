using System.Globalization;

namespace Catharsis.Text;

///<summary>
///Truncates a string to at most a specified number of grapheme clusters (user-perceived characters), using ///<see
///cref="StringInfo"/> so a combining sequence or surrogate pair is never split in half the way truncating by raw <see
///cref="string.Length"/> could.
///</summary>
public static class TextElementTruncator
{
    #region Public methods

    ///<summary>
    ///Truncates <paramref name="value"/> to at most <paramref name="maxTextElements"/> grapheme clusters.
    ///</summary>
    ///<param name="value">The string to truncate.</param>
    ///<param name="maxTextElements">The maximum number of grapheme clusters to keep. Must not be negative.</param>
    ///<returns>
    ///<paramref name="value"/> unchanged if it already has at most <paramref name="maxTextElements"/> grapheme
    ///clusters; otherwise a truncated copy.
    ///</returns>
    ///<exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="maxTextElements"/> is negative.</exception>
    public static string Truncate(string value, int maxTextElements)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegative(maxTextElements);

        StringInfo info = new(value);
        return (info.LengthInTextElements <= maxTextElements) ? value : info.SubstringByTextElements(0, maxTextElements);
    }
    #endregion
}
