using System.Collections;
using System.Globalization;

namespace Catharsis.Text;

///<summary>
///Enumerates a string's grapheme clusters (user-perceived characters) rather than its UTF-16 code units, so
///emoji, combining marks, and other multi-code-unit characters are treated as a single element instead of being
///split apart the way indexing or <see cref="string.Substring(int)"/> would.
///</summary>
///<param name="value">The string to enumerate by grapheme cluster.</param>
public sealed class GraphemeClusterEnumerator(string value) : IEnumerable<string>
{
    #region Fields
    readonly string _value = value ?? throw new ArgumentNullException(nameof(value));
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public IEnumerator<string> GetEnumerator()
    {
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(_value);

        while (enumerator.MoveNext())
        {
            yield return (string)enumerator.Current;
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of grapheme clusters in the string. This can be less than <see cref="string.Length"/> for
    ///strings containing surrogate pairs, combining marks, or other multi-code-unit characters.
    ///</summary>
    public int Count => new StringInfo(_value).LengthInTextElements;
    #endregion
}
