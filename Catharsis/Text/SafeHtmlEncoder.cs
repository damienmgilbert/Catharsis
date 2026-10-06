using System.Text.Encodings.Web;

namespace Catharsis.Text;

///<summary>
///Encodes a string for safe inclusion in HTML markup, wrapping <see cref="HtmlEncoder"/> behind <see
///cref="ITextEncoder"/>.
///</summary>
///<param name="encoder">The underlying encoder to use, or <c>null</c> to use <see cref="HtmlEncoder.Default"/>.</param>
public sealed class SafeHtmlEncoder(HtmlEncoder? encoder = null) : ITextEncoder
{
    #region Fields
    private readonly HtmlEncoder _encoder = encoder ?? HtmlEncoder.Default;
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public string Encode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return _encoder.Encode(value);
    }
    #endregion
}
