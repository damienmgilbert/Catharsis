namespace Catharsis.Text.RegularExpressions;

///<summary>
///Represents the result of a single regular expression match, capturing the matched value, its start index within the
///input, and the length of the match. This is a lightweight, immutable value type designed for use with <see
///cref="RegexTokenizer"/> and similar utilities.
///</summary>
///<param name="Value">The matched text.</param>
///<param name="Index">The zero-based starting position of the match within the input string.</param>
///<param name="Length">The number of characters in the match.</param>
public readonly record struct MatchResult(string Value, int Index, int Length)
{
    #region Public methods

    ///<inheritdoc/>
    public override string ToString() => $"'{Value}' at {Index} (length {Length})";
    #endregion
}
