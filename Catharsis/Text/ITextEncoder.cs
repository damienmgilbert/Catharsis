namespace Catharsis.Text;

///<summary>
///Encodes a string for safe inclusion in a particular output context (e.g. HTML markup, a JavaScript string
///literal), giving <see cref="SafeHtmlEncoder"/> and <see cref="SafeJsEncoder"/> a common shape so either can be
///plugged into the same substitution pipeline without the caller needing to know which one it is.
///</summary>
public interface ITextEncoder
{
    #region Public methods
    ///<summary>
    ///Encodes <paramref name="value"/> for safe inclusion in this encoder's target context.
    ///</summary>
    ///<param name="value">The raw value to encode.</param>
    ///<returns>The encoded value.</returns>
    string Encode(string value);
    #endregion
}
