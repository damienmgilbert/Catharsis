using Catharsis.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Catharsis.UnitTests.Security;

///<summary>
///Unit tests for the <see cref="CertificateThumbprintValidator"/> class.
///</summary>
[TestClass]
public class CertificateThumbprintValidatorTests
{
    #region Private methods
    static X509Certificate2 CreateCertificate(DateTimeOffset notBefore, DateTimeOffset notAfter)
    {
        using RSA rsa = RSA.Create(2048);
        CertificateRequest request = new("CN=Catharsis Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return request.CreateSelfSigned(notBefore, notAfter);
    }
    #endregion

    #region Constructor

    [TestMethod]
    public void Constructor_NullThumbprints_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new CertificateThumbprintValidator(null!)); }

    [TestMethod]
    public void Constructor_EmptyThumbprints_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new CertificateThumbprintValidator([])); }

    #endregion

    #region IsThumbprintTrusted

    [TestMethod]
    public void IsThumbprintTrusted_NullCertificate_Throws()
    {
        CertificateThumbprintValidator validator = new(["AABBCC"]);
        Assert.ThrowsExactly<ArgumentNullException>(() => validator.IsThumbprintTrusted(null!));
    }

    [TestMethod]
    public void IsThumbprintTrusted_MatchingThumbprint_ReturnsTrue()
    {
        using X509Certificate2 certificate = CreateCertificate(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        CertificateThumbprintValidator validator = new([certificate.Thumbprint]);

        Assert.IsTrue(validator.IsThumbprintTrusted(certificate));
    }

    [TestMethod]
    public void IsThumbprintTrusted_ThumbprintWithColonsAndLowercase_StillMatches()
    {
        using X509Certificate2 certificate = CreateCertificate(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        string decorated = string.Join(":", Chunk(certificate.Thumbprint.ToLowerInvariant(), 2));

        CertificateThumbprintValidator validator = new([decorated]);

        Assert.IsTrue(validator.IsThumbprintTrusted(certificate));
    }

    [TestMethod]
    public void IsThumbprintTrusted_UnknownThumbprint_ReturnsFalse()
    {
        using X509Certificate2 certificate = CreateCertificate(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        CertificateThumbprintValidator validator = new(["0000000000000000000000000000000000000000"]);

        Assert.IsFalse(validator.IsThumbprintTrusted(certificate));
    }

    #endregion

    #region IsWithinValidityPeriod

    [TestMethod]
    public void IsWithinValidityPeriod_NullCertificate_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => CertificateThumbprintValidator.IsWithinValidityPeriod(null!)); }

    [TestMethod]
    public void IsWithinValidityPeriod_CurrentlyValidCertificate_ReturnsTrue()
    {
        using X509Certificate2 certificate = CreateCertificate(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        Assert.IsTrue(CertificateThumbprintValidator.IsWithinValidityPeriod(certificate));
    }

    [TestMethod]
    public void IsWithinValidityPeriod_ExpiredCertificate_ReturnsFalse()
    {
        using X509Certificate2 certificate = CreateCertificate(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1));
        Assert.IsFalse(CertificateThumbprintValidator.IsWithinValidityPeriod(certificate));
    }

    [TestMethod]
    public void IsWithinValidityPeriod_NotYetValidCertificate_ReturnsFalse()
    {
        using X509Certificate2 certificate = CreateCertificate(DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(2));
        Assert.IsFalse(CertificateThumbprintValidator.IsWithinValidityPeriod(certificate));
    }

    #endregion

    #region Validate

    [TestMethod]
    public void Validate_TrustedAndCurrentlyValid_ReturnsTrue()
    {
        using X509Certificate2 certificate = CreateCertificate(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        CertificateThumbprintValidator validator = new([certificate.Thumbprint]);

        Assert.IsTrue(validator.Validate(certificate));
    }

    [TestMethod]
    public void Validate_TrustedButExpired_ReturnsFalse()
    {
        using X509Certificate2 certificate = CreateCertificate(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1));
        CertificateThumbprintValidator validator = new([certificate.Thumbprint]);

        Assert.IsFalse(validator.Validate(certificate));
    }

    [TestMethod]
    public void Validate_ValidButUntrusted_ReturnsFalse()
    {
        using X509Certificate2 certificate = CreateCertificate(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        CertificateThumbprintValidator validator = new(["0000000000000000000000000000000000000000"]);

        Assert.IsFalse(validator.Validate(certificate));
    }

    #endregion

    #region Test helpers
    static IEnumerable<string> Chunk(string value, int size)
    {
        for(int i = 0; i < value.Length; i += size)
        {
            yield return value.Substring(i, Math.Min(size, value.Length - i));
        }
    }
    #endregion
}
