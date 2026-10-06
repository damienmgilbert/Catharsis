using System.ComponentModel;

namespace Catharsis.ComponentModel.DTO;

///<summary>
///An immutable record containing metadata for a single property, combining DataAnnotations and <see
///cref="TypeDescriptor"/> information.
///</summary>
///<param name="PropertyName">The property name.</param>
///<param name="DisplayName">The display-friendly name.</param>
///<param name="Description">The property description, or <c>null</c>.</param>
///<param name="PropertyType">The CLR type of the property.</param>
///<param name="IsReadOnly">Whether the property is read-only.</param>
///<param name="IsRequired">Whether the property is required.</param>
///<param name="Category">The category for grouping.</param>
///<param name="Attributes">All attributes applied to the property.</param>
public sealed record PropertyMetadataEntry(string PropertyName, string DisplayName, string? Description, Type PropertyType, bool IsReadOnly, bool IsRequired, string? Category, IReadOnlyList<Attribute> Attributes);
