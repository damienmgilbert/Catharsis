using System.Collections;
using System.Text.RegularExpressions;

namespace Catharsis.Text.RegularExpressions;

///<summary>
///Splits text into tokens using a <see cref="Regex"/> pattern. Each token records the matched value, its start index,
///and its length. The tokenizer implements <see cref="IEnumerable{T}"/> of <see cref="MatchResult"/> for easy iteration
///and LINQ composition.
///</summary>
public sealed class RegexTokenizer : IEnumerable<MatchResult>
{
    #region Fields
    private readonly Regex _pattern;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="RegexTokenizer"/> with the specified pattern.
    ///</summary>
    ///<param name="pattern">The regex pattern that defines a single token.</param>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <c>null</c>.</exception>
    public RegexTokenizer(Regex pattern)
    {
        if(pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern), "Pattern must not be null.");
        }

        _pattern = pattern;
    }

    ///<summary>
    ///Initializes a new <see cref="RegexTokenizer"/> with a pattern string compiled with <see
    ///cref="RegexOptions.Compiled"/>.
    ///</summary>
    ///<param name="pattern">The regex pattern string.</param>
    ///<param name="options">
    ///Additional <see cref="RegexOptions"/> applied alongside <see cref="RegexOptions.Compiled"/>. Defaults to <see
    ///cref="RegexOptions.None"/>.
    ///</param>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <c>null</c>.</exception>
    public RegexTokenizer(string pattern, RegexOptions options = RegexOptions.None)
    {
        if(pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern), "Pattern must not be null.");
        }

        _pattern = new Regex(pattern, options | RegexOptions.Compiled, TimeSpan.FromSeconds(2));
    }
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Public methods
    ///<summary>
    ///Returns the number of tokens that match the pattern in the specified input.
    ///</summary>
    ///<param name="input">The text to search.</param>
    ///<returns>The count of matching tokens.</returns>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is <c>null</c>.</exception>
    public int Count(string input)
    {
        if(input is null)
        {
            throw new ArgumentNullException(nameof(input), "Input must not be null.");
        }

        return _pattern.Count(input);
    }

    ///<inheritdoc/>
    public IEnumerator<MatchResult> GetEnumerator() => Tokenize(string.Empty).GetEnumerator();

    ///<summary>
    ///Returns the non-matching segments of the input (the text between tokens).
    ///</summary>
    ///<param name="input">The text to split.</param>
    ///<returns>An array of strings representing the gaps between matched tokens.</returns>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is <c>null</c>.</exception>
    public string[] Split(string input)
    {
        if(input is null)
        {
            throw new ArgumentNullException(nameof(input), "Input must not be null.");
        }

        return _pattern.Split(input);
    }

    ///<summary>
    ///Tokenizes the specified input string and returns the matched tokens as a list of <see cref="MatchResult"/>
    ///values.
    ///</summary>
    ///<param name="input">The text to tokenize.</param>
    ///<returns>A read-only list of tokens extracted from <paramref name="input"/>.</returns>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is <c>null</c>.</exception>
    public IReadOnlyList<MatchResult> Tokenize(string input)
    {
        if(input is null)
        {
            throw new ArgumentNullException(nameof(input), "Input must not be null.");
        }

        MatchCollection matches = _pattern.Matches(input);
        List<MatchResult> tokens = [ with(matches.Count) ];

        foreach(Match match in matches)
        {
            tokens.Add(new MatchResult(match.Value, match.Index, match.Length));
        }

        return tokens;
    }
    #endregion
}
