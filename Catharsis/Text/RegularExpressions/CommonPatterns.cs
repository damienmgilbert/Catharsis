using System.Text.RegularExpressions;

namespace Catharsis.Text.RegularExpressions;

///<summary>
///Provides pre-compiled, source-generated <see cref="Regex"/> instances for commonly used text patterns such as email
///addresses, URLs, IP addresses, and more. Each pattern is compiled at build time via <see
///cref="GeneratedRegexAttribute"/> for optimal runtime performance.
///</summary>
public static partial class CommonPatterns
{
    #region Public methods

    ///<summary>
    ///Matches a string consisting only of alphanumeric characters.
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for alphanumeric strings.</returns>
    [GeneratedRegex(@"^[a-zA-Z0-9]+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex Alphanumeric();

    ///<summary>
    ///Matches a credit card number consisting of 13–19 digits optionally separated by dashes or spaces.
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for credit card numbers.</returns>
    [GeneratedRegex(@"^[\d\-\s]{13,25}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex CreditCard();

    ///<summary>
    ///Matches a simplified email address (local-part@domain).
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for email addresses.</returns>
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex Email();

    ///<summary>
    ///Matches a file path on Windows (e.g. C:\Folder\File.txt) or Unix (e.g. /home/user/file.txt).
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for file paths.</returns>
    [GeneratedRegex(@"^([a-zA-Z]:\\[\\\S|*\S]?.*|/[^\s]*)$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex FilePath();

    ///<summary>
    ///Matches a GUID in standard format (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for GUIDs.</returns>
    [GeneratedRegex(@"^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex Guid();

    ///<summary>
    ///Matches a hexadecimal color code with an optional leading '#' (e.g. #FF00AA or FF00AA).
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for hex color codes.</returns>
    [GeneratedRegex(@"^#?([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex HexColor();

    ///<summary>
    ///Matches an HTML tag including self-closing tags (e.g. &lt;p&gt;, &lt;br/&gt;, &lt;/div&gt;).
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for HTML tags.</returns>
    [GeneratedRegex(@"</?[a-zA-Z][a-zA-Z0-9]*[^>]*>", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex HtmlTag();

    ///<summary>
    ///Matches an IPv4 address (e.g. 192.168.1.1).
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for IPv4 addresses.</returns>
    [GeneratedRegex(@"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex IPv4();

    ///<summary>
    ///Matches a date in ISO 8601 format (yyyy-MM-dd).
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for ISO dates.</returns>
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex IsoDate();

    ///<summary>
    ///Matches a time in ISO 8601 format (HH:mm or HH:mm:ss).
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for ISO times.</returns>
    [GeneratedRegex(@"^\d{2}:\d{2}(:\d{2})?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex IsoTime();

    ///<summary>
    ///Matches a MAC address in colon-separated or dash-separated format (e.g. 00:1A:2B:3C:4D:5E).
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for MAC addresses.</returns>
    [GeneratedRegex(@"^([0-9A-Fa-f]{2}[:\-]){5}[0-9A-Fa-f]{2}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex MacAddress();

    ///<summary>
    ///Matches an international phone number that starts with an optional plus sign followed by digits, spaces, dashes,
    ///or parentheses.
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for phone numbers.</returns>
    [GeneratedRegex(@"^\+?[\d\s\-\(\)\.]{7,20}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex PhoneNumber();

    ///<summary>
    ///Matches a semantic version string (e.g. 1.2.3 or 1.0.0-beta.1+build.42).
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for semantic versions.</returns>
    [GeneratedRegex(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(-[0-9A-Za-z\-]+(\.[0-9A-Za-z\-]+)*)?(\+[0-9A-Za-z\-]+(\.[0-9A-Za-z\-]+)*)?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex SemanticVersion();

    ///<summary>
    ///Matches a US Social Security Number in the format XXX-XX-XXXX.
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for SSNs.</returns>
    [GeneratedRegex(@"^\d{3}-\d{2}-\d{4}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex SocialSecurityNumber();

    ///<summary>
    ///Matches a strong password requiring at least one uppercase letter, one lowercase letter, one digit, and one
    ///special character with a minimum length of 8 characters.
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for strong passwords.</returns>
    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex StrongPassword();

    ///<summary>
    ///Matches an HTTP or HTTPS URL.
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for URLs.</returns>
    [GeneratedRegex(@"^https?://[^\s/$.?#].[^\s]*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex Url();

    ///<summary>
    ///Matches one or more consecutive whitespace characters. Useful for normalizing or splitting text.
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for whitespace runs.</returns>
    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex Whitespace();

    ///<summary>
    ///Matches a US ZIP code in either 5-digit or 5+4 format.
    ///</summary>
    ///<returns>A source-generated <see cref="Regex"/> for ZIP codes.</returns>
    [GeneratedRegex(@"^\d{5}(-\d{4})?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 2000)]
    public static partial Regex ZipCode();
    #endregion
}
