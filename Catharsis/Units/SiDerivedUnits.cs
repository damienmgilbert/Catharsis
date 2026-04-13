namespace Catharsis.Units;

/// <summary>
/// Provides a catalogue of SI derived units and their abbreviations.
/// Base unit notation: kg = kilogram (mass), m = meter (length), s = second (time), A = ampere (electric current).
/// </summary>
public static class SiDerivedUnits
{
    /// <summary>
    /// Gets all SI derived units with their quantity, name, abbreviation, and base-unit expression.
    /// </summary>
    public static IReadOnlyList<SiDerivedUnit> All { get; } =
    [
        new("Force",               "newton",  "N",  "kg·m/s²"),
        new("Energy and work",     "joule",   "J",  "kg·m²/s²"),
        new("Power",               "watt",    "W",  "kg·m²/s³"),
        new("Pressure",            "pascal",  "Pa", "kg/(m·s²)"),
        new("Frequency",           "hertz",   "Hz", "s⁻¹"),
        new("Electric charge",     "coulomb", "C",  "A·s"),
        new("Electric potential",  "volt",    "V",  "kg·m²/(A·s³)"),
        new("Electric resistance", "ohm",     "Ω",  "kg·m²/(A²·s³)"),
        new("Capacitance",         "farad",   "F",  "A²·s⁴/(kg·m²)"),
        new("Magnetic field",      "tesla",   "T",  "kg/(A·s²)"),
        new("Magnetic flux",       "weber",   "Wb", "kg·m²/(A·s²)"),
        new("Inductance",          "henry",   "H",  "kg·m²/(s²·A²)"),
    ];
}
