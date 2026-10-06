using System.Text.Encodings.Web;

namespace Catharsis.Text;

///<summary>
///Encodes a string for safe inclusion in a JavaScript string literal, wrapping <see cref="JavaScriptEncoder"/>
///behind <see cref="ITextEncoder"/>.
///</summary>
///<param name="encoder">The underlying encoder to use, or <c>null</c> to use <see cref="JavaScriptEncoder.Default"/>.</param>
public sealed class SafeJsEncoder(JavaScriptEncoder? encoder = null) : ITextEncoder
{
    #region Fields
    readonly JavaScriptEncoder _encoder = encoder ?? JavaScriptEncoder.Default;
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
