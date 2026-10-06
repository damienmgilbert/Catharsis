namespace Catharsis.Mathematics;

/// <summary>
/// Provides a catalogue of common mathematical signs and symbols with their descriptions.
/// </summary>
public static class MathSymbols
{
    /// <summary>Gets the catalogue of common mathematical signs and symbols.</summary>
    public static IReadOnlyList<MathSymbol> All { get; } =
    [
        new("∝",      "is proportional to"),
        new("=",      "is equal to"),
        new("≈",      "is approximately equal to"),
        new("≠",      "is not equal to"),
        new(">",      "is greater than"),
        new(">>",     "is much greater than"),
        new("<",      "is less than"),
        new("<<",     "is much less than"),
        new("≤",      "is less than or equal to"),
        new("≥",      "is greater than or equal to"),
        new("Σ",      "sum of"),
        new("x̄",      "average value of x"),
        new("Δx",     "change in x"),
        new("Δx → 0", "Δx approaches zero"),
        new("n!",     "n(n − 1)(n − 2) … (1)"),
    ];
}
