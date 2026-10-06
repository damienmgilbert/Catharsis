using System.Security.Cryptography;

namespace Catharsis.Security;

///<summary>
///Signs and verifies byte payloads with HMAC-SHA256, for tamper-evident tokens. Complements
///<see cref="SecureRandomToken"/>, which only generates random tokens: use this when a payload's authenticity —
///not just its randomness — must be verifiable, such as a signed cookie value or webhook payload.
///</summary>
///<param name="key">The shared secret key.</param>
///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
///<exception cref="ArgumentException"><paramref name="key"/> is empty.</exception>
public sealed class HmacSigner(byte[] key)
{
    #region Fields
    readonly byte[] _key = ValidateKey(key);
    #endregion

    #region Private methods
    static byte[] ValidateKey(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if(key.Length == 0)
        {
            throw new ArgumentException("Key must not be empty.", nameof(key));
        }

        return key;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Computes the HMAC-SHA256 signature of <paramref name="data"/>.
    ///</summary>
    ///<param name="data">The data to sign.</param>
    ///<returns>The 32-byte signature.</returns>
    public byte[] Sign(ReadOnlySpan<byte> data) => HMACSHA256.HashData(_key, data);

    ///<summary>
    ///Verifies that <paramref name="signature"/> is the correct HMAC-SHA256 signature of <paramref name="data"/>,
    ///using a constant-time comparison to avoid leaking timing information about the correct signature.
    ///</summary>
    ///<param name="data">The data that was signed.</param>
    ///<param name="signature">The signature to verify.</param>
    ///<returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
    public bool Verify(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature) => CryptographicOperations.FixedTimeEquals(HMACSHA256.HashData(_key, data), signature);
    #endregion
}
