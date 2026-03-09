using Catharsis.ComponentModel.Licensing;

namespace Catharsis.UnitTests.ComponentModel.Licensing;

///<summary>
///Unit tests for the <see cref="BasicLicense"/> class.
///</summary>
[TestClass]
public sealed class BasicLicenseTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_DefaultOptionalParameters()
    {
        BasicLicense license = new BasicLicense("KEY");

        Assert.IsNull(license.Licensee);
        Assert.IsNull(license.ExpiresUtc);
    }

    [TestMethod]
    public void Constructor_NullKey_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new BasicLicense(null!)); }
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        DateTime expiry = new DateTime(2030, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        BasicLicense license = new BasicLicense("KEY-123", "Alice", expiry);

        Assert.AreEqual("KEY-123", license.LicenseKey);
        Assert.AreEqual("Alice", license.Licensee);
        Assert.AreEqual(expiry, license.ExpiresUtc);
    }

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        BasicLicense license = new BasicLicense("KEY");

        license.Dispose();
        license.Dispose();
    }

    [TestMethod]
    public void IsExpired_FutureExpiry_ReturnsFalse()
    {
        BasicLicense license = new BasicLicense("KEY", expiresUtc: DateTime.UtcNow.AddYears(1));

        Assert.IsFalse(license.IsExpired);
    }

    [TestMethod]
    public void IsExpired_NoExpiry_ReturnsFalse()
    {
        BasicLicense license = new BasicLicense("KEY");

        Assert.IsFalse(license.IsExpired);
    }

    [TestMethod]
    public void IsExpired_PastExpiry_ReturnsTrue()
    {
        BasicLicense license = new BasicLicense("KEY", expiresUtc: DateTime.UtcNow.AddDays(-1));

        Assert.IsTrue(license.IsExpired);
    }

    [TestMethod]
    public void IsValid_AfterDispose_ReturnsFalse()
    {
        BasicLicense license = new BasicLicense("KEY");
        license.Dispose();

        Assert.IsFalse(license.IsValid);
    }

    [TestMethod]
    public void IsValid_ExpiredLicense_ReturnsFalse()
    {
        BasicLicense license = new BasicLicense("KEY", expiresUtc: DateTime.UtcNow.AddDays(-1));

        Assert.IsFalse(license.IsValid);
    }

    [TestMethod]
    public void IsValid_NewPerpetualLicense_ReturnsTrue()
    {
        BasicLicense license = new BasicLicense("KEY");

        Assert.IsTrue(license.IsValid);
    }

    [TestMethod]
    public void LicenseKey_ReturnsConstructorValue()
    {
        BasicLicense license = new BasicLicense("MY-KEY-456");

        Assert.AreEqual("MY-KEY-456", license.LicenseKey);
    }
    #endregion
}
