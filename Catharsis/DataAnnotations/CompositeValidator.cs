using System.ComponentModel.DataAnnotations;

namespace Catharsis.DataAnnotations;

/// <summary>
/// Provides a fluent, composable entry-point for running
/// <see cref="System.ComponentModel.DataAnnotations"/> validation using the
/// built-in <see cref="Validator"/> infrastructure. It gathers results from
/// <see cref="Validator.TryValidateObject"/>,
/// <see cref="Validator.TryValidateProperty"/>, and
/// <see cref="IValidatableObject.Validate"/> into a single
/// <see cref="CompositeValidationResult"/>.
/// </summary>
public static class CompositeValidator
{
    /// <summary>
    /// Validates all annotated properties and <see cref="IValidatableObject"/>
    /// on the given <paramref name="instance"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to validate.</typeparam>
    /// <param name="instance">The object to validate.</param>
    /// <returns>
    /// A <see cref="CompositeValidationResult"/> containing all
    /// <see cref="ValidationResult"/> instances, if any.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <c>null</c>.</exception>
    public static CompositeValidationResult ValidateObject<T>(T instance) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);

        ValidationContext context = new(instance);
        List<ValidationResult> results = [];
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);

        return new CompositeValidationResult(results);
    }

    /// <summary>
    /// Validates a single property on the given <paramref name="instance"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object that owns the property.</typeparam>
    /// <param name="instance">The object that owns the property.</param>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <returns>
    /// A <see cref="CompositeValidationResult"/> containing results for the property.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="instance"/> or <paramref name="propertyName"/> is <c>null</c>.
    /// </exception>
    public static CompositeValidationResult ValidateProperty<T>(T instance, string propertyName)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(propertyName);

        var propertyInfo = typeof(T).GetProperty(propertyName)
            ?? throw new ArgumentException(
                $"Property '{propertyName}' not found on type '{typeof(T).Name}'.",
                nameof(propertyName));

        object? value = propertyInfo.GetValue(instance);
        ValidationContext context = new(instance) { MemberName = propertyName };
        List<ValidationResult> results = [];
        Validator.TryValidateProperty(value, context, results);

        return new CompositeValidationResult(results);
    }

    /// <summary>
    /// Validates a single value against an explicit set of
    /// <see cref="ValidationAttribute"/> instances.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">A human-readable name used in error messages.</param>
    /// <param name="attributes">The validation attributes to apply.</param>
    /// <returns>
    /// A <see cref="CompositeValidationResult"/> containing all failures, if any.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="displayName"/> or <paramref name="attributes"/> is <c>null</c>.
    /// </exception>
    public static CompositeValidationResult ValidateValue(
        object? value,
        string displayName,
        IEnumerable<ValidationAttribute> attributes)
    {
        ArgumentNullException.ThrowIfNull(displayName);
        ArgumentNullException.ThrowIfNull(attributes);

        ValidationContext context = new(value ?? new object()) { DisplayName = displayName };
        List<ValidationResult> results = [];
        Validator.TryValidateValue(value!, context, results, attributes);

        return new CompositeValidationResult(results);
    }
}

/// <summary>
/// Represents the aggregated outcome of a composite validation operation,
/// providing convenient access to errors, member names, and an overall
/// success/failure indicator.
/// </summary>
public sealed class CompositeValidationResult
{
    private readonly IReadOnlyList<ValidationResult> _results;

    /// <summary>
    /// Initializes a FileName instance of <see cref="CompositeValidationResult"/>
    /// with the collected validation results.
    /// </summary>
    /// <param name="results">The validation results.</param>
    internal CompositeValidationResult(IReadOnlyList<ValidationResult> results)
    {
        _results = results;
    }

    /// <summary>
    /// Gets a value indicating whether validation passed with no errors.
    /// </summary>
    public bool IsValid => _results.Count == 0;

    /// <summary>
    /// Gets all <see cref="ValidationResult"/> instances produced by validation.
    /// </summary>
    public IReadOnlyList<ValidationResult> Results => _results;

    /// <summary>
    /// Gets all distinct error messages.
    /// </summary>
    public IEnumerable<string> ErrorMessages =>
        _results.Select(r => r.ErrorMessage).Where(m => m is not null)!;

    /// <summary>
    /// Gets all distinct member names that had validation failures.
    /// </summary>
    public IEnumerable<string> MemberNames =>
        _results.SelectMany(r => r.MemberNames).Distinct();

    /// <summary>
    /// Throws a <see cref="ValidationException"/> if the result contains
    /// any errors. This mirrors the behavior of
    /// <see cref="Validator.ValidateObject"/>.
    /// </summary>
    /// <exception cref="ValidationException">Validation failed.</exception>
    public void ThrowIfInvalid()
    {
        if (_results.Count > 0)
        {
            throw new ValidationException(
                _results[0],
                validatingAttribute: null,
                value: null);
        }
    }
}
