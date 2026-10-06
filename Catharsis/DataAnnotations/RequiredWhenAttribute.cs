using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace Catharsis.DataAnnotations;

///<summary>
///Makes a property conditionally required: the annotated property must have a non-null, non-empty value when the
///specified <see cref="DependentProperty"/> equals any of a set of target values.
///</summary>
///<remarks>
///This differs from <see cref="RequiredIfAttribute"/> by accepting multiple target values, which reads more
///naturally when the dependent property is an enum with several members that all trigger the requirement (e.g.
///<c>[RequiredWhen(nameof(Status), Status.Approved, Status.Shipped)]</c>) instead of stacking several
///<see cref="RequiredIfAttribute"/> instances.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class RequiredWhenAttribute : ValidationAttribute
{
    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="RequiredWhenAttribute"/>.
    ///</summary>
    ///<param name="dependentProperty">The name of the sibling property that controls the requirement.</param>
    ///<param name="targetValues">The values that <paramref name="dependentProperty"/> may equal for this property to become required.</param>
    ///<exception cref="ArgumentNullException"><paramref name="dependentProperty"/> or <paramref name="targetValues"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="targetValues"/> is empty.</exception>
    public RequiredWhenAttribute(string dependentProperty, params object?[] targetValues) : base("The field {0} is required when {1} is one of the configured values.")
    {
        ArgumentNullException.ThrowIfNull(dependentProperty);
        ArgumentNullException.ThrowIfNull(targetValues);

        if (targetValues.Length == 0)
        {
            throw new ArgumentException("At least one target value must be specified.", nameof(targetValues));
        }

        DependentProperty = dependentProperty;
        TargetValues = targetValues;
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        PropertyInfo? dependentPropertyInfo = validationContext.ObjectType.GetProperty(DependentProperty);

        if (dependentPropertyInfo is null)
        {
            return new ValidationResult($"Unknown property: {DependentProperty}.");
        }

        object? dependentValue = dependentPropertyInfo.GetValue(validationContext.ObjectInstance);

        if (!TargetValues.Any(target => Equals(dependentValue, target)))
        {
            return ValidationResult.Success;
        }

        if ((value is null) || (DisallowEmptyStrings && (value is string { Length: 0 })))
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), (validationContext.MemberName is not null) ? [validationContext.MemberName] : null);
        }

        return ValidationResult.Success;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override string FormatErrorMessage(string name) { return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, DependentProperty); }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the name of the property whose value determines whether the annotated property is required.
    ///</summary>
    public string DependentProperty { get; }

    ///<summary>
    ///Gets or sets a value indicating whether empty strings are treated as missing values. Defaults to <c>true</c>.
    ///</summary>
    public bool DisallowEmptyStrings { get; set; } = true;

    ///<summary>
    ///Gets the values that <see cref="DependentProperty"/> may equal for the annotated property to be required.
    ///</summary>
    public IReadOnlyList<object?> TargetValues { get; }
    #endregion
}
