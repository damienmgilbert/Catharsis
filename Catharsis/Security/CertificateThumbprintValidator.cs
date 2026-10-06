using System.Collections.Frozen;
using System.Security.Cryptography.X509Certificates;

namespace Catharsis.Security;

///<summary>
///Validates an <see cref="X509Certificate2"/> against a fixed set of expected thumbprints and its validity period,
///for certificate-pinning scenarios. Delegates entirely to the BCL's own certificate APIs; performs no ASN.1
///parsing of its own.
///</summary>
public sealed class CertificateThumbprintValidator
{
    #region Fields
    readonly FrozenSet<string> _expectedThumbprints;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new validator trusting the given thumbprints. Colons, spaces, and casing are normalized away, so
    ///thumbprints can be supplied in whatever format they were copied from (e.g. <c>"AB:CD:EF..."</c> or lowercase hex).
    ///</summary>
    ///<param name="expectedThumbprints">The trusted certificate thumbprints.</param>
    ///<exception cref="ArgumentNullException"><paramref name="expectedThumbprints"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="expectedThumbprints"/> contains no thumbprints.</exception>
    public CertificateThumbprintValidator(IEnumerable<string> expectedThumbprints)
    {
        ArgumentNullException.ThrowIfNull(expectedThumbprints);

        _expectedThumbprints = expectedThumbprints
            .Select(static thumbprint => thumbprint.Replace(":", "", StringComparison.Ordinal).Replace(" ", "", StringComparison.Ordinal))
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        if(_expectedThumbprints.Count == 0)
        {
            throw new ArgumentException("At least one expected thumbprint must be provided.", nameof(expectedThumbprints));
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Determines whether <paramref name="certificate"/>'s thumbprint is one of the expected thumbprints.
    ///</summary>
    ///<param name="certificate">The certificate to check.</param>
    ///<exception cref="ArgumentNullException"><paramref name="certificate"/> is <c>null</c>.</exception>
    public bool IsThumbprintTrusted(X509Certificate2 certificate)
    {
        ArgumentNullException.ThrowIfNull(certificate);
        return _expectedThumbprints.Contains(certificate.Thumbprint);
    }

    ///<summary>
    ///Determines whether <paramref name="certificate"/> is within its validity period at <paramref name="when"/>.
    ///</summary>
    ///<param name="certificate">The certificate to check.</param>
    ///<param name="when">The point in time to check against, in local time. Defaults to now if not specified.</param>
    ///<exception cref="ArgumentNullException"><paramref name="certificate"/> is <c>null</c>.</exception>
    public static bool IsWithinValidityPeriod(X509Certificate2 certificate, DateTime? when = null)
    {
        ArgumentNullException.ThrowIfNull(certificate);

        DateTime effectiveWhen = when ?? DateTime.Now;
        return (effectiveWhen >= certificate.NotBefore) && (effectiveWhen <= certificate.NotAfter);
    }

    ///<summary>
    ///Determines whether <paramref name="certificate"/> is trusted (per <see cref="IsThumbprintTrusted"/>) and
    ///within its validity period (per <see cref="IsWithinValidityPeriod"/>).
    ///</summary>
    ///<param name="certificate">The certificate to validate.</param>
    ///<param name="when">The point in time to check validity against, in local time. Defaults to now if not specified.</param>
    ///<exception cref="ArgumentNullException"><paramref name="certificate"/> is <c>null</c>.</exception>
    public bool Validate(X509Certificate2 certificate, DateTime? when = null) => IsThumbprintTrusted(certificate) && IsWithinValidityPeriod(certificate, when);
    #endregion
}
