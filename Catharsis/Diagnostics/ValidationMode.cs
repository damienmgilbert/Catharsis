namespace Catharsis.Diagnostics;

/// <summary>
/// Specifies how validation is performed on diagnostic-enhanced buffer operations.
/// </summary>
public enum ValidationMode
{
    /// <summary>No validation is performed. Maximum performance, no safety checks.</summary>
    None,

    /// <summary>Validates only on boundary conditions (e.g., capacity exceeded, null references).</summary>
    BoundsOnly,

    /// <summary>Full validation including bounds checks, state verification, and argument validation.</summary>
    Full
}
