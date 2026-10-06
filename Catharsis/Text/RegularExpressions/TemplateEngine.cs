using System.Text.RegularExpressions;

namespace Catharsis.Text.RegularExpressions;

///<summary>
///Renders a minimal <c>{{token}}</c>-style string template by substituting each token with a value from a supplied
///lookup, leaving unrecognized tokens untouched.
///</summary>
public sealed class TemplateEngine
{
    #region Fields
    private static readonly Regex TokenPattern = new(@"\{\{\s*(\w+)\s*\}\}", RegexOptions.Compiled, TimeSpan.FromSeconds(2));
    #endregion

    #region Public methods
    ///<summary>
    ///Renders <paramref name="template"/>, replacing every <c>{{token}}</c> placeholder whose name is a key in
    public string Render(string template, IReadOnlyDictionary<string, string?> values)
    {
        if(template is null)
        {
            throw new ArgumentNullException(nameof(template), "Template must not be null.");
        }

        if(values is null)
        {
            throw new ArgumentNullException(nameof(values), "Values must not be null.");
        }

        return TokenPattern.Replace(template, match => values.TryGetValue(match.Groups[1].Value, out string? value) ? (value ?? string.Empty) : match.Value);
    }
    #endregion
}
