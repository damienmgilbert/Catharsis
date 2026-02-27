namespace Catharsis.ComponentModel.DTO;

/// <summary>
/// Configuration options for <see cref="ComponentModelDtoMapper"/>,
/// controlling how DTOs are mapped, validated, and annotated.
/// </summary>
public sealed class ComponentModelDtoOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether property change notifications
    /// should be raised during mapping. Defaults to <c>true</c>.
    /// </summary>
    public bool RaisePropertyChangedOnMap { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether validation should be
    /// performed automatically after mapping. Defaults to <c>false</c>.
    /// </summary>
    public bool ValidateAfterMap { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether metadata annotations from
    /// the source should be preserved during mapping. Defaults to <c>true</c>.
    /// </summary>
    public bool PreserveMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether properties that do not exist
    /// on the target should be silently ignored. When <c>false</c>, a missing
    /// target property throws an exception. Defaults to <c>true</c>.
    /// </summary>
    public bool IgnoreMissingProperties { get; set; } = true;

    /// <summary>
    /// Gets or sets the string comparison used when matching property names
    /// between source and target. Defaults to <see cref="StringComparison.Ordinal"/>.
    /// </summary>
    public StringComparison PropertyNameComparison { get; set; } = StringComparison.Ordinal;

    /// <summary>
    /// Gets a default <see cref="ComponentModelDtoOptions"/> instance.
    /// </summary>
    public static ComponentModelDtoOptions Default { get; } = new();
}
