using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.ComponentModel.Validation;

/// <summary>
/// Validates objects using <see cref="System.ComponentModel.DataAnnotations"/>
/// attributes and exposes results through <see cref="INotifyDataErrorInfo"/>
/// backed by an <see cref="ErrorDictionary"/>.
/// </summary>
/// <remarks>
/// <para>
/// Call <see cref="ValidateObject"/> to validate all annotated properties, or
/// <see cref="ValidateProperty"/> to validate a single property. Results are
/// stored in the <see cref="Errors"/> dictionary and change notifications are
/// raised through <see cref="ErrorsChanged"/>.
/// </para>
/// <para>
/// The <see cref="ValidationContextFactory"/> can be provided at construction
/// time to control service-provider and item configuration on contexts.
/// </para>
/// </remarks>
public sealed class DataAnnotationValidator : INotifyDataErrorInfo
{
    private readonly ErrorDictionary _errors = new();
    private readonly ValidationContextFactory _contextFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="DataAnnotationValidator"/>.
    /// </summary>
    /// <param name="contextFactory">
    /// An optional factory for creating <see cref="ValidationContext"/> instances.
    /// If <c>null</c>, a default factory is used.
    /// </param>
    public DataAnnotationValidator(ValidationContextFactory? contextFactory = null)
    {
        _contextFactory = contextFactory ?? new ValidationContextFactory();
        _errors.ErrorsChanged += (_, e) => ErrorsChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Gets the underlying error dictionary.
    /// </summary>
    public ErrorDictionary Errors => _errors;

    /// <inheritdoc />
    public bool HasErrors => _errors.HasErrors;

    /// <inheritdoc />
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <inheritdoc />
    public IEnumerable GetErrors(string? propertyName) =>
        _errors.GetErrors(propertyName);

    /// <summary>
    /// Validates all annotated properties on the specified object and populates
    /// the <see cref="Errors"/> dictionary with results.
    /// </summary>
    /// <param name="instance">The object to validate.</param>
    /// <returns><c>true</c> if the object is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="instance"/> is <c>null</c>.
    /// </exception>
    public bool ValidateObject(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        _errors.ClearAll();

        var context = _contextFactory.CreateContext(instance);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(instance, context, results, validateAllProperties: true);

        foreach (var result in results)
        {
            var members = result.MemberNames.ToList();

            if (members.Count == 0)
            {
                _errors.AddError(string.Empty,
                    new ErrorInfo(result.ErrorMessage ?? "Validation failed."));
            }
            else
            {
                foreach (var member in members)
                {
                    _errors.AddError(member,
                        new ErrorInfo(result.ErrorMessage ?? "Validation failed.", PropertyName: member));
                }
            }
        }

        return isValid;
    }

    /// <summary>
    /// Validates a single property on the specified object and updates the
    /// <see cref="Errors"/> dictionary for that property.
    /// </summary>
    /// <param name="instance">The object that owns the property.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="value">The property value to validate.</param>
    /// <returns><c>true</c> if the property is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="instance"/> or <paramref name="propertyName"/> is <c>null</c>.
    /// </exception>
    public bool ValidateProperty(object instance, string propertyName, object? value)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(propertyName);

        _errors.ClearErrors(propertyName);

        var context = _contextFactory.CreatePropertyContext(instance, propertyName);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateProperty(value, context, results);

        foreach (var result in results)
        {
            _errors.AddError(propertyName,
                new ErrorInfo(result.ErrorMessage ?? "Validation failed.",
                    PropertyName: propertyName));
        }

        return isValid;
    }

    /// <summary>
    /// Clears all validation errors.
    /// </summary>
    public void ClearAll() => _errors.ClearAll();
}
