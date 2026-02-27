namespace Catharsis.ComponentModel.Validation;

/// <summary>
/// Specifies the scope at which a validation rule applies.
/// </summary>
public enum ValidationScope
{
    /// <summary>
    /// The rule validates a single property.
    /// </summary>
    Property = 0,

    /// <summary>
    /// The rule validates the entire object.
    /// </summary>
    Object = 1,

    /// <summary>
    /// The rule validates cross-property relationships.
    /// </summary>
    CrossProperty = 2
}
