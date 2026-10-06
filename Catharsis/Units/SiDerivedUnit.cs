namespace Catharsis.Units;

///<summary>
///Represents an SI derived unit with its physical quantity, unit name, abbreviation, and expression in terms of SI base
///units.
///</summary>
///<param name="Quantity">The physical quantity measured (e.g., "Force").</param>
///<param name="UnitName">The SI unit name (e.g., "newton").</param>
///<param name="Abbreviation">The unit symbol (e.g., "N").</param>
///<param name="BaseUnits">
///The expression in terms of SI base units (kg = kilogram, m = meter, s = second, A = ampere).
///</param>
public sealed record SiDerivedUnit(string Quantity, string UnitName, string Abbreviation, string BaseUnits);
