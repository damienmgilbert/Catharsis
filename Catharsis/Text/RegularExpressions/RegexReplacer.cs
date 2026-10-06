using System.Text.RegularExpressions;

namespace Catharsis.Text.RegularExpressions;

///<summary>
///A fluent builder that chains multiple regex-based replacements against a string. Each replacement is appended to an
///internal pipeline and applied sequentially when <see cref="Apply"/> is called.
///</summary>
public sealed class RegexReplacer
{
    #region Fields
    readonly List<(Regex Pattern, string Replacement)> _rules = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Applies all queued replacement rules to the specified input string in order.
    ///</summary>
    ///<param name="input">The text to transform.</param>
    ///<returns>The resulting string after all replacements have been applied.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
    public string Apply(string input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input), "Input must not be null.");
        }

        string result = input;

        foreach ((Regex pattern, string replacement) in _rules)
        {
            result = pattern.Replace(result, replacement);
        }

        return result;
    }

    ///<summary>
    ///Removes all queued replacement rules.
    ///</summary>
    ///<returns>The current <see cref="RegexReplacer"/> instance for fluent chaining.</returns>
    public RegexReplacer Clear()
    {
        _rules.Clear();
        return this;
    }

    ///<summary>
    ///Appends a replacement rule using a pre-compiled <see cref="Regex"/> instance.
    ///</summary>
    ///<param name="pattern">The regex pattern to match.</param>
    ///<param name="replacement">The replacement string (supports group references such as <c>$1</c>).</param>
    ///<returns>The current <see cref="RegexReplacer"/> instance for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    public RegexReplacer Replace(Regex pattern, string replacement)
    {
        if (pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern), "Pattern must not be null.");
        }

        if (replacement is null)
        {
            throw new ArgumentNullException(nameof(replacement), "Replacement must not be null.");
        }

        _rules.Add((pattern, replacement));
        return this;
    }

    ///<summary>
    ///Appends a replacement rule using a string pattern that is compiled with <see cref="RegexOptions.Compiled"/>.
    ///</summary>
    ///<param name="pattern">The regex pattern string.</param>
    ///<param name="replacement">The replacement string.</param>
    ///<param name="options">
    ///Additional <see cref="RegexOptions"/> applied alongside <see cref="RegexOptions.Compiled"/>. Defaults to <see
    ///cref="RegexOptions.None"/>.
    ///</param>
    ///<returns>The current <see cref="RegexReplacer"/> instance for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    public RegexReplacer Replace(string pattern, string replacement, RegexOptions options = RegexOptions.None)
    {
        if (pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern), "Pattern must not be null.");
        }

        if (replacement is null)
        {
            throw new ArgumentNullException(nameof(replacement), "Replacement must not be null.");
        }

        _rules.Add((new Regex(pattern, options | RegexOptions.Compiled, TimeSpan.FromSeconds(2)), replacement));
        return this;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of replacement rules currently in the pipeline.
    ///</summary>
    public int Count => _rules.Count;
    #endregion
}
