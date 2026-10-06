namespace Catharsis.Mathematics;

///<summary>
///Represents a letter of the Greek alphabet with its English name and Unicode characters.
///</summary>
///<param name="Name">The English name of the letter (e.g., "Alpha").</param>
///<param name="Uppercase">The uppercase Greek character (e.g., 'Α').</param>
///<param name="Lowercase">The primary lowercase Greek character (e.g., 'α').</param>
///<param name="AlternateLowercase">
///An alternate lowercase form where one exists (e.g., 'ϕ' for Phi); otherwise <c>null</c>.
///</param>
public sealed record GreekLetter(string Name, char Uppercase, char Lowercase, char? AlternateLowercase = null);
