using System.Text.RegularExpressions;

namespace Catharsis.Text.RegularExpressions;

///<summary>
///Matches strings against a glob-style wildcard pattern (<c>*</c> for any run of characters, <c>?</c> for exactly one
///character), compiled once to a <see cref="Regex"/> for repeated matching.
///</summary>
public sealed class WildcardMatcher
{
    #region Fields
    readonly Regex _pattern;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="WildcardMatcher"/> with the specified glob pattern.
    ///</summary>
    ///<param name="pattern">The wildcard pattern, using <c>*</c> and <c>?</c> as placeholders.</param>
    ///<param name="ignoreCase">Whether matching should be case-insensitive.</param>
    ///<exception cref="ArgumentNullException"><paramref name="pattern"/> is <c>null</c>.</exception>
    public WildcardMatcher(string pattern, bool ignoreCase = false)
    {
        if(pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern), "Pattern must not be null.");
        }

        string regexPattern = $"^{Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".")}$";
        RegexOptions options = RegexOptions.Compiled | RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);

        _pattern = new Regex(regexPattern, options, TimeSpan.FromSeconds(2));
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Determines whether <paramref name="input"/> matches the wildcard pattern in its entirety.
    ///</summary>
    ///<param name="input">The text to test.</param>
    ///<returns><c>true</c> if <paramref name="input"/> matches the pattern; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
    public bool IsMatch(string input)
    {
        if(input is null)
        {
            throw new ArgumentNullException(nameof(input), "Input must not be null.");
        }

        return _pattern.IsMatch(input);
    }
    #endregion
}
