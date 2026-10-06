using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Catharsis.DataAnnotations;

///<summary>
///Provides a fluent, composable entry-point for running <see cref="System.ComponentModel.DataAnnotations"/> validation
///using the built-in <see cref="Validator"/> infrastructure. It gathers results from <see
///cref="Validator.TryValidateObject"/>, <see cref="Validator.TryValidateProperty"/>, and <see
///cref="IValidatableObject.Validate"/> into a single <see cref="CompositeValidationResult"/>.
///</summary>
public static class CompositeValidator
{
    #region Public methods

    ///<summary>
    ///Validates all annotated properties and <see cref="IValidatableObject"/> on the given <paramref name="instance"/>.
    ///</summary>
    ///<typeparam name="T">The type of the object to validate.</typeparam>
    ///<param name="instance">The object to validate.</param>
    ///<returns>
    ///A <see cref="CompositeValidationResult"/> containing all <see cref="ValidationResult"/> instances, if any.
    ///</returns>
    ///<exception cref="ArgumentNullException"><paramref name="instance"/> is <c>null</c>.</exception>
    public static CompositeValidationResult ValidateObject<T>(T instance) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);

        ValidationContext context = new(instance);
        List<ValidationResult> results = [];
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);

        return new CompositeValidationResult(results);
    }

    ///<summary>
    ///Validates a single property on the given <paramref name="instance"/>.
    ///</summary>
    ///<typeparam name="T">The type of the object that owns the property.</typeparam>
    ///<param name="instance">The object that owns the property.</param>
    ///<param name="propertyName">The name of the property to validate.</param>
    ///<returns>
    ///A <see cref="CompositeValidationResult"/> containing results for the property.
    ///</returns>
    ///<exception cref="ArgumentNullException">
    public static CompositeValidationResult ValidateProperty<T>(T instance, string propertyName) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(propertyName);

        PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName) ?? throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.", nameof(propertyName));

        object? value = propertyInfo.GetValue(instance);
        ValidationContext context = new(instance) { MemberName = propertyName };
        List<ValidationResult> results = [];
        Validator.TryValidateProperty(value, context, results);

        return new CompositeValidationResult(results);
    }

    ///<summary>
    ///Validates a single value against an explicit set of <see cref="ValidationAttribute"/> instances.
    ///</summary>
    ///<param name="value">The value to validate.</param>
    ///<param name="displayName">A human-readable name used in error messages.</param>
    ///<param name="attributes">The validation attributes to apply.</param>
    ///<returns>
    ///A <see cref="CompositeValidationResult"/> containing all failures, if any.
    ///</returns>
    ///<exception cref="ArgumentNullException">
    public static CompositeValidationResult ValidateValue(object? value, string displayName, IEnumerable<ValidationAttribute> attributes)
    {
        ArgumentNullException.ThrowIfNull(displayName);
        ArgumentNullException.ThrowIfNull(attributes);

        ValidationContext context = new(value ?? new object()) { DisplayName = displayName };
        List<ValidationResult> results = [];
        Validator.TryValidateValue(value!, context, results, attributes);

        return new CompositeValidationResult(results);
    }
    #endregion
}
