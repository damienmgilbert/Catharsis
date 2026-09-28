using Catharsis.Security;
using System.Security.Cryptography;
using System.Text;

namespace Catharsis.UnitTests.Security;

///<summary>
///Unit tests for the <see cref="AesGcmEnvelope"/> class.
///</summary>
[TestClass]
public class AesGcmEnvelopeTests
{
    #region Private methods
    static byte[] CreateKey() => RandomNumberGenerator.GetBytes(32);
    #endregion

    #region Constructor

    [TestMethod]
    public void Constructor_NullKey_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new AesGcmEnvelope(null!)); }

    #endregion

    #region Encrypt / Decrypt round trip

    [TestMethod]
    public void Decrypt_EncryptedData_RoundTripsToOriginalPlaintext()
    {
        using AesGcmEnvelope envelope = new(CreateKey());
        byte[] plaintext = Encoding.UTF8.GetBytes("hello, aes-gcm");

        byte[] encrypted = envelope.Encrypt(plaintext);
        byte[] decrypted = envelope.Decrypt(encrypted);

        CollectionAssert.AreEqual(plaintext, decrypted);
    }

    [TestMethod]
    public void Encrypt_EmptyPlaintext_RoundTrips()
    {
        using AesGcmEnvelope envelope = new(CreateKey());

        byte[] encrypted = envelope.Encrypt([]);
        byte[] decrypted = envelope.Decrypt(encrypted);

        Assert.IsEmpty(decrypted);
    }

    [TestMethod]
    public void Encrypt_SamePlaintextTwice_ProducesDifferentEnvelopes()
    {
        using AesGcmEnvelope envelope = new(CreateKey());
        byte[] plaintext = "hello"u8.ToArray();

        byte[] first = envelope.Encrypt(plaintext);
        byte[] second = envelope.Encrypt(plaintext);

        CollectionAssert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void Encrypt_EnvelopeLength_IsNonceLengthPlusPlaintextLengthPlusTagLength()
    {
        using AesGcmEnvelope envelope = new(CreateKey());
        byte[] plaintext = "hello"u8.ToArray();

        byte[] encrypted = envelope.Encrypt(plaintext);

        Assert.AreEqual(12 + plaintext.Length + 16, encrypted.Length);
    }

    #endregion

    #region Decrypt validation

    [TestMethod]
    public void Decrypt_TooShortEnvelope_Throws()
    {
        using AesGcmEnvelope envelope = new(CreateKey());
        Assert.ThrowsExactly<ArgumentException>(() => envelope.Decrypt(new byte[10]));
    }

    [TestMethod]
    public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
    {
        using AesGcmEnvelope envelope = new(CreateKey());
        byte[] encrypted = envelope.Encrypt("hello"u8.ToArray());
        encrypted[^1] ^= 0xFF;

        Assert.ThrowsExactly<AuthenticationTagMismatchException>(() => envelope.Decrypt(encrypted));
    }

    [TestMethod]
    public void Decrypt_WithDifferentKey_ThrowsCryptographicException()
    {
        using AesGcmEnvelope envelopeA = new(CreateKey());
        using AesGcmEnvelope envelopeB = new(CreateKey());

        byte[] encrypted = envelopeA.Encrypt("hello"u8.ToArray());

        Assert.ThrowsExactly<AuthenticationTagMismatchException>(() => envelopeB.Decrypt(encrypted));
    }

    #endregion

    #region Dispose

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        AesGcmEnvelope envelope = new(CreateKey());
        envelope.Dispose();
        envelope.Dispose();
    }

    #endregion
}
