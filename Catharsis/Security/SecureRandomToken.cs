using System.Buffers.Text;
using System.Security.Cryptography;

namespace Catharsis.Security;

///<summary>
///Generates cryptographically random, URL-safe string tokens (e.g. for password-reset links, API keys, or session
///identifiers) backed by <see cref="RandomNumberGenerator"/>.
///</summary>
public static class SecureRandomToken
{
    #region Public methods
    ///<summary>
    ///Generates a new random token.
    ///</summary>
    ///<param name="byteLength">The number of random bytes to generate before encoding. Defaults to 32 (256 bits).</param>
    ///<returns>A URL-safe, base64url-encoded token.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="byteLength"/> is not positive.</exception>
    public static string Generate(int byteLength = 32)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteLength);

        byte[] bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Base64Url.EncodeToString(bytes);
    }
    #endregion
}
