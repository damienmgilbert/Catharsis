namespace Catharsis.Mathematics;

/// <summary>
/// Represents a mathematical sign or symbol together with its meaning.
/// </summary>
/// <param name="Symbol">The symbol character or string (e.g., "∝").</param>
/// <param name="Description">The meaning of the symbol (e.g., "is proportional to").</param>
public sealed record MathSymbol(string Symbol, string Description);
