using System.Security.Cryptography;

namespace Catharsis.Security;

///<summary>
///Authenticated symmetric encryption over <see cref="AesGcm"/>, framing a random per-message nonce and the
///authentication tag together with the ciphertext into a single self-contained byte array (<c>nonce || ciphertext
///|| tag</c>), so callers don't need to manage nonce generation or storage themselves.
///</summary>
///<param name="key">The 128, 192, or 256-bit AES key.</param>
///<exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
public sealed class AesGcmEnvelope(byte[] key) : IDisposable
{
    #region Fields
    const int NonceSize = 12;
    const int TagSize = 16;

    readonly AesGcm _aes = new(key ?? throw new ArgumentNullException(nameof(key)), TagSize);
    #endregion

    #region Public methods
    ///<summary>
    ///Encrypts <paramref name="plaintext"/> into a self-contained envelope.
    ///</summary>
    ///<param name="plaintext">The data to encrypt.</param>
    ///<returns>A byte array containing the nonce, ciphertext, and authentication tag.</returns>
    public byte[] Encrypt(ReadOnlySpan<byte> plaintext)
    {
        byte[] envelope = new byte[NonceSize + plaintext.Length + TagSize];

        Span<byte> nonce = envelope.AsSpan(0, NonceSize);
        Span<byte> ciphertext = envelope.AsSpan(NonceSize, plaintext.Length);
        Span<byte> tag = envelope.AsSpan(NonceSize + plaintext.Length, TagSize);

        RandomNumberGenerator.Fill(nonce);
        _aes.Encrypt(nonce, plaintext, ciphertext, tag);

        return envelope;
    }

    ///<summary>
    ///Decrypts an envelope produced by <see cref="Encrypt"/>.
    ///</summary>
    ///<param name="envelope">The nonce, ciphertext, and authentication tag, as produced by <see cref="Encrypt"/>.</param>
    ///<returns>The decrypted plaintext.</returns>
    ///<exception cref="ArgumentException"><paramref name="envelope"/> is too short to contain a nonce and tag.</exception>
    ///<exception cref="CryptographicException">The tag does not match, or the envelope has been tampered with.</exception>
    public byte[] Decrypt(ReadOnlySpan<byte> envelope)
    {
        if(envelope.Length < (NonceSize + TagSize))
        {
            throw new ArgumentException("Envelope is too short to contain a nonce and tag.", nameof(envelope));
        }

        ReadOnlySpan<byte> nonce = envelope[..NonceSize];
        ReadOnlySpan<byte> tag = envelope[^TagSize..];
        ReadOnlySpan<byte> ciphertext = envelope[NonceSize..^TagSize];

        byte[] plaintext = new byte[ciphertext.Length];
        _aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return plaintext;
    }

    ///<inheritdoc/>
    public void Dispose() => _aes.Dispose();
    #endregion
}
