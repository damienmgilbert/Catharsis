using Catharsis.ComponentModel.Licensing;

namespace Catharsis.UnitTests.ComponentModel.Licensing;

[TestClass]
public sealed class BasicLicenseTests
{
    [TestMethod]
    public void Constructor_NullKey_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new BasicLicense(null!));
    }

    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var expiry = new DateTime(2030, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var license = new BasicLicense("KEY-123", "Alice", expiry);

        Assert.AreEqual("KEY-123", license.LicenseKey);
        Assert.AreEqual("Alice", license.Licensee);
        Assert.AreEqual(expiry, license.ExpiresUtc);
    }

    [TestMethod]
    public void Constructor_DefaultOptionalParameters()
    {
        var license = new BasicLicense("KEY");

        Assert.IsNull(license.Licensee);
        Assert.IsNull(license.ExpiresUtc);
    }

    [TestMethod]
    public void IsExpired_NoExpiry_ReturnsFalse()
    {
        var license = new BasicLicense("KEY");

        Assert.IsFalse(license.IsExpired);
    }

    [TestMethod]
    public void IsExpired_FutureExpiry_ReturnsFalse()
    {
        var license = new BasicLicense("KEY", expiresUtc: DateTime.UtcNow.AddYears(1));

        Assert.IsFalse(license.IsExpired);
    }

    [TestMethod]
    public void IsExpired_PastExpiry_ReturnsTrue()
    {
        var license = new BasicLicense("KEY", expiresUtc: DateTime.UtcNow.AddDays(-1));

        Assert.IsTrue(license.IsExpired);
    }

    [TestMethod]
    public void IsValid_NewPerpetualLicense_ReturnsTrue()
    {
        var license = new BasicLicense("KEY");

        Assert.IsTrue(license.IsValid);
    }

    [TestMethod]
    public void IsValid_ExpiredLicense_ReturnsFalse()
    {
        var license = new BasicLicense("KEY", expiresUtc: DateTime.UtcNow.AddDays(-1));

        Assert.IsFalse(license.IsValid);
    }

    [TestMethod]
    public void IsValid_AfterDispose_ReturnsFalse()
    {
        var license = new BasicLicense("KEY");
        license.Dispose();

        Assert.IsFalse(license.IsValid);
    }

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var license = new BasicLicense("KEY");

        license.Dispose();
        license.Dispose();
    }

    [TestMethod]
    public void LicenseKey_ReturnsConstructorValue()
    {
        var license = new BasicLicense("MY-KEY-456");

        Assert.AreEqual("MY-KEY-456", license.LicenseKey);
    }
}
