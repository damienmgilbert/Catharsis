using System.Globalization;

namespace Catharsis.Operators;

///<summary>
///A simplified semantic version (<c>major.minor.patch</c> with an optional <c>-prerelease</c> label). Ordering follows
///semver: a pre-release sorts before its release, and build metadata is not supported. The indexer exposes the three
///numeric parts (0 = major, 1 = minor, 2 = patch).
///</summary>
public readonly struct SemanticVersion : IEquatable<SemanticVersion>, IComparable<SemanticVersion>
{
    #region Struct fields
    private readonly string? _prerelease;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="SemanticVersion"/>.
    ///</summary>
    ///<param name="major">The major part. Must not be negative.</param>
    ///<param name="minor">The minor part. Must not be negative.</param>
    ///<param name="patch">The patch part. Must not be negative.</param>
    ///<param name="prerelease">An optional pre-release label such as <c>beta</c>; null or empty for a release.</param>
    ///<exception cref="ArgumentOutOfRangeException">A numeric part is negative.</exception>
    public SemanticVersion(int major, int minor, int patch, string? prerelease = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(major);
        ArgumentOutOfRangeException.ThrowIfNegative(minor);
        ArgumentOutOfRangeException.ThrowIfNegative(patch);

        Major = major;
        Minor = minor;
        Patch = patch;
        _prerelease = string.IsNullOrEmpty(prerelease) ? string.Empty : prerelease;
    }
    #endregion

    #region Operators
    ///<summary>
    ///Determines whether two versions differ.
    ///</summary>
    public static bool operator !=(SemanticVersion left, SemanticVersion right)
    {
        return !left.Equals(right);
    }

    ///<summary>
    ///Determines whether one version precedes another.
    ///</summary>
    public static bool operator <(SemanticVersion left, SemanticVersion right)
    {
        return left.CompareTo(right) < 0;
    }

    ///<summary>
    ///Determines whether one version precedes or equals another.
    ///</summary>
    public static bool operator <=(SemanticVersion left, SemanticVersion right)
    {
        return left.CompareTo(right) <= 0;
    }

    ///<summary>
    ///Determines whether two versions are equal.
    ///</summary>
    public static bool operator ==(SemanticVersion left, SemanticVersion right)
    {
        return left.Equals(right);
    }

    ///<summary>
    ///Determines whether one version follows another.
    ///</summary>
    public static bool operator >(SemanticVersion left, SemanticVersion right)
    {
        return left.CompareTo(right) > 0;
    }

    ///<summary>
    ///Determines whether one version follows or equals another.
    ///</summary>
    public static bool operator >=(SemanticVersion left, SemanticVersion right)
    {
        return left.CompareTo(right) >= 0;
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets a numeric part (0 = major, 1 = minor, 2 = patch).
    ///</summary>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="part"/> is not 0, 1 or 2.</exception>
    public int this[int part] => part switch
    {
        0 => Major,
        1 => Minor,
        2 => Patch,
        _ => throw new ArgumentOutOfRangeException(nameof(part), part, "Part must be 0, 1 or 2.")
    };
    #endregion

    #region Private properties
    // default(SemanticVersion) leaves the backing field null, so every read goes through this null-safe accessor.
    private string PrereleaseLabel => _prerelease ?? string.Empty;
    #endregion

    #region Public methods
    ///<summary>
    ///Returns the next major version (minor and patch reset, pre-release cleared).
    ///</summary>
    public SemanticVersion BumpMajor() => new(Major + 1, 0, 0);

    ///<summary>
    ///Returns the next minor version (patch reset, pre-release cleared).
    ///</summary>
    public SemanticVersion BumpMinor() => new(Major, Minor + 1, 0);

    ///<summary>
    ///Returns the next patch version (pre-release cleared).
    ///</summary>
    public SemanticVersion BumpPatch() => new(Major, Minor, Patch + 1);

    ///<inheritdoc/>
    public int CompareTo(SemanticVersion other)
    {
        int result = Major.CompareTo(other.Major);

        if(result == 0)
        {
            result = Minor.CompareTo(other.Minor);
        }

        if(result == 0)
        {
            result = Patch.CompareTo(other.Patch);
        }

        if(result != 0)
        {
            return result;
        }

        if(PrereleaseLabel.Length == 0)
        {
            return other.PrereleaseLabel.Length == 0 ? 0 : 1;
        }

        return other.PrereleaseLabel.Length == 0 ? -1 : string.CompareOrdinal(PrereleaseLabel, other.PrereleaseLabel);
    }

    ///<inheritdoc/>
    public bool Equals(SemanticVersion other) => CompareTo(other) == 0;

    ///<inheritdoc/>
    public override bool Equals(object? obj) => (obj is SemanticVersion other) && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, PrereleaseLabel);

        ///<summary>
///Parses a string such as <c>1.2.3</c> or <c>1.2.3-beta</c>.
///</summary>
    ///<exception cref="FormatException"><paramref name="text"/> is not a valid version.</exception>
    public static SemanticVersion Parse(string text) { return TryParse(text, out SemanticVersion version) ? version : throw new FormatException($"'{text}' is not a valid semantic version."); }
    ///<inheritdoc/>
    public override string ToString() => PrereleaseLabel.Length == 0 ? string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor}.{Patch}") : string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor}.{Patch}-{PrereleaseLabel}");

    ///<summary>
    ///Attempts to parse a string such as <c>1.2.3</c> or <c>1.2.3-beta</c>.
    ///</summary>
    ///<returns>True if <paramref name="text"/> was valid.</returns>
    public static bool TryParse(string? text, out SemanticVersion version)
    {
        version = default;

        if(string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string core = text;
        string? prerelease = null;
        int dash = text.IndexOf('-', StringComparison.Ordinal);

        if(dash >= 0)
        {
            core = text[..dash];
            prerelease = text[(dash + 1)..];

            if(prerelease.Length == 0)
            {
                return false;
            }
        }

        string[] parts = core.Split('.');

        if(parts.Length != 3 ||
           !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int major) ||
           !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int minor) ||
           !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out int patch))
        {
            return false;
        }

        version = new SemanticVersion(major, minor, patch, prerelease);
        return true;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a value indicating whether this is a pre-release.
    ///</summary>
    public bool IsPrerelease => PrereleaseLabel.Length > 0;

        ///<summary>
///Gets the major part.
///</summary>
    public int Major { get; }

    ///<summary>
    ///Gets the minor part.
    ///</summary>
    public int Minor { get; }

    ///<summary>
    ///Gets the patch part.
    ///</summary>
    public int Patch { get; }

    ///<summary>
    ///Gets the pre-release label, or an empty string for a release.
    ///</summary>
    public string Prerelease => PrereleaseLabel;
    #endregion
}
