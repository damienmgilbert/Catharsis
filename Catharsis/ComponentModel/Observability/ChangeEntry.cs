namespace Catharsis.ComponentModel.Observability;

/// <summary>
/// An immutable record representing a single property change, capturing the
/// property name, old value, new value, and the timestamp of the change.
/// </summary>
/// <param name="PropertyName">The name of the property that changed.</param>
/// <param name="OldValue">The value before the change.</param>
/// <param name="NewValue">The value after the change.</param>
/// <param name="Timestamp">
/// The UTC timestamp when the change occurred. Defaults to the current time.
/// </param>
public sealed record ChangeEntry(
    string PropertyName,
    object? OldValue,
    object? NewValue,
    DateTime? Timestamp = null)
{
    /// <summary>
    /// Gets the UTC timestamp when the change occurred.
    /// </summary>
    public DateTime Timestamp { get; } = Timestamp ?? DateTime.UtcNow;

    /// <inheritdoc />
    public override string ToString() =>
        $"{PropertyName}: '{OldValue}' -> '{NewValue}' at {Timestamp:O}";
}
