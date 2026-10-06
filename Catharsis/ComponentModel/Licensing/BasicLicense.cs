using System.ComponentModel;

namespace Catharsis.ComponentModel.Licensing;

///<summary>
///A simple <see cref="License"/> implementation that carries a license key, expiration date, and licensee name.
///</summary>
///<remarks>
///Dispose this license when it is no longer needed. The <see cref="LicenseKey"/> property returns the key string that
///was used to grant the license.
///</remarks>
public sealed class BasicLicense : License
{
    #region Fields
    private bool _disposed;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="BasicLicense"/>.
    ///</summary>
    ///<param name="licenseKey">The license key string.</param>
    ///<param name="licensee">
    ///The name of the licensee, or <c>null</c> if not applicable.
    ///</param>
    ///<param name="expiresUtc">
    ///The UTC expiration date, or <c>null</c> for a perpetual license.
    ///</param>
    ///<exception cref="ArgumentNullException">
    public BasicLicense(string licenseKey, string? licensee = null, DateTime? expiresUtc = null)
    {
        ArgumentNullException.ThrowIfNull(licenseKey);

        LicenseKey = licenseKey;
        Licensee = licensee;
        ExpiresUtc = expiresUtc;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override void Dispose()
    {
        _disposed = true;
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the UTC expiration date, or <c>null</c> for perpetual licenses.
    ///</summary>
    public DateTime? ExpiresUtc { get; }

    ///<summary>
    ///Gets a value indicating whether this license has expired. Perpetual licenses never expire.
    ///</summary>
    public bool IsExpired => ExpiresUtc.HasValue && (DateTime.UtcNow > ExpiresUtc.Value);

    ///<summary>
    ///Gets a value indicating whether this license is currently valid (not disposed and not expired).
    ///</summary>
    public bool IsValid => !_disposed && !IsExpired;

    ///<summary>
    ///Gets the name of the licensee.
    ///</summary>
    public string? Licensee { get; }

    ///<inheritdoc/>
    public override string LicenseKey { get; }
    #endregion
}
