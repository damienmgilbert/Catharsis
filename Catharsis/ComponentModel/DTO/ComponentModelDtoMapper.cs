using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Catharsis.ComponentModel.DTO;

/// <summary>
/// Maps property values between objects using <see cref="TypeDescriptor"/>
/// and <see cref="PropertyInfo"/> with configurable options for notification,
/// validation, and metadata handling.
/// </summary>
/// <remarks>
/// <para>
/// The mapper copies all readable source properties to matching writable
/// target properties. Configure behavior through <see cref="ComponentModelDtoOptions"/>.
/// </para>
/// <para>
/// Use <see cref="Map{TSource, TTarget}(TSource, TTarget)"/> to copy into
/// an existing target, or <see cref="Map{TSource, TTarget}(TSource)"/> to
/// create a new instance (requires a parameterless constructor).
/// </para>
/// </remarks>
public sealed class ComponentModelDtoMapper
{
    private readonly ComponentModelDtoOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="ComponentModelDtoMapper"/>
    /// with the specified options.
    /// </summary>
    /// <param name="options">
    /// Mapping options. If <c>null</c>, <see cref="ComponentModelDtoOptions.Default"/>
    /// is used.
    /// </param>
    public ComponentModelDtoMapper(ComponentModelDtoOptions? options = null)
    {
        _options = options ?? ComponentModelDtoOptions.Default;
    }

    /// <summary>
    /// Maps property values from <paramref name="source"/> to
    /// <paramref name="target"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object to populate.</param>
    /// <returns>The populated <paramref name="target"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> or <paramref name="target"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// A source property has no matching target property and
    /// <see cref="ComponentModelDtoOptions.IgnoreMissingProperties"/> is <c>false</c>.
    /// </exception>
    public TTarget Map<TSource, TTarget>(TSource source, TTarget target)
        where TSource : notnull
        where TTarget : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var sourceProperties = TypeDescriptor.GetProperties(source);
        var targetProperties = TypeDescriptor.GetProperties(target);

        foreach (PropertyDescriptor sourceProp in sourceProperties)
        {
            if (!sourceProp.CanResetValue(source) && sourceProp.IsReadOnly)
                continue;

            var targetProp = FindProperty(targetProperties, sourceProp.Name);

            if (targetProp is null)
            {
                if (!_options.IgnoreMissingProperties)
                {
                    throw new InvalidOperationException(
                        $"Property '{sourceProp.Name}' exists on source type " +
                        $"'{typeof(TSource).Name}' but not on target type '{typeof(TTarget).Name}'.");
                }

                continue;
            }

            if (targetProp.IsReadOnly)
                continue;

            var value = sourceProp.GetValue(source);
            targetProp.SetValue(target, value);
        }

        if (_options.ValidateAfterMap)
            ValidateTarget(target);

        return target;
    }

    /// <summary>
    /// Maps property values from <paramref name="source"/> to a new instance
    /// of <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">
    /// The target type. Must have a parameterless constructor.
    /// </typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>A new populated instance of <typeparamref name="TTarget"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <c>null</c>.
    /// </exception>
    public TTarget Map<TSource, TTarget>(TSource source)
        where TSource : notnull
        where TTarget : notnull, new()
    {
        ArgumentNullException.ThrowIfNull(source);
        return Map<TSource, TTarget>(source, new TTarget());
    }

    /// <summary>
    /// Maps property values from <paramref name="source"/> into a new
    /// <see cref="BindableRecord{T}"/>.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="source">The source record.</param>
    /// <returns>A <see cref="BindableRecord{T}"/> wrapping the source.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <c>null</c>.
    /// </exception>
    public BindableRecord<T> ToBindable<T>(T source) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        return new BindableRecord<T>(source);
    }

    /// <summary>
    /// Maps property values from <paramref name="source"/> into a new
    /// <see cref="ValidatedRecord{T}"/>, running validation.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="source">The source record.</param>
    /// <returns>A <see cref="ValidatedRecord{T}"/> wrapping the source.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <c>null</c>.
    /// </exception>
    public ValidatedRecord<T> ToValidated<T>(T source) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValidatedRecord<T>(source);
    }

    /// <summary>
    /// Maps property values from <paramref name="source"/> into a new
    /// <see cref="EditableRecord{T}"/>.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="source">The source record.</param>
    /// <returns>An <see cref="EditableRecord{T}"/> wrapping the source.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <c>null</c>.
    /// </exception>
    public EditableRecord<T> ToEditable<T>(T source) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        return new EditableRecord<T>(source);
    }

    /// <summary>
    /// Maps property values from <paramref name="source"/> into a new
    /// <see cref="MetadataAnnotatedRecord{T}"/>.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="source">The source record.</param>
    /// <returns>A <see cref="MetadataAnnotatedRecord{T}"/> wrapping the source.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <c>null</c>.
    /// </exception>
    public MetadataAnnotatedRecord<T> ToMetadataAnnotated<T>(T source) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        return new MetadataAnnotatedRecord<T>(source);
    }

    private PropertyDescriptor? FindProperty(
        PropertyDescriptorCollection properties,
        string name)
    {
        foreach (PropertyDescriptor prop in properties)
        {
            if (string.Equals(prop.Name, name, _options.PropertyNameComparison))
                return prop;
        }

        return null;
    }

    private static void ValidateTarget<TTarget>(TTarget target) where TTarget : notnull
    {
        var context = new ValidationContext(target);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(target, context, results, validateAllProperties: true))
        {
            var errors = string.Join("; ", results.Select(r => r.ErrorMessage));
            throw new ValidationException(
                $"Validation failed after mapping to '{typeof(TTarget).Name}': {errors}");
        }
    }
}
