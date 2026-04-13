namespace Catharsis.Units;

/// <summary>
/// Represents a Metric (SI) prefix with its name, abbreviation, and power-of-ten exponent.
/// </summary>
/// <param name="Name">The prefix name (e.g., "giga").</param>
/// <param name="Abbreviation">The prefix symbol (e.g., "G").</param>
/// <param name="Exponent">The power-of-ten exponent (e.g., 9 represents 10⁹).</param>
public sealed record MetricPrefix(string Name, string Abbreviation, int Exponent)
{
    /// <summary>Gets the numeric multiplier value: 10^<see cref="Exponent"/>.</summary>
    public double Value => Math.Pow(10.0, Exponent);
}
