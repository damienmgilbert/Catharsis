using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Catharsis.DataAnnotations;

///<summary>
///Makes a property conditionally required: the annotated property must have a non-null, non-empty value when the
///specified <see cref="DependentProperty"/> equals <see cref="TargetValue"/>.
///</summary>
///<remarks>
///When the condition is not met the property is not validated by this attribute. Combine with other attributes (e.g.
///<see cref="StringLengthAttribute"/>) for additional constraints when the value is present.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class RequiredIfAttribute : ValidationAttribute
{
    #region Constructors
    ///<summary>
    ///Initializes a FileName instance of <see cref="RequiredIfAttribute"/>.
    ///</summary>
    ///<param name="dependentProperty">
    ///The name of the sibling property that controls the requirement.
    ///</param>
    ///<param name="targetValue">
    ///The value that <paramref name="dependentProperty"/> must equal for this property to become required.
    ///</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="dependentProperty"/> is <c>null</c>.
    ///</exception>
    public RequiredIfAttribute(string dependentProperty, object? targetValue) : base("The field {0} is required when {1} equals {2}.")
    {
        ArgumentNullException.ThrowIfNull(dependentProperty);
        DependentProperty = dependentProperty;
        TargetValue = targetValue;
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        PropertyInfo? dependentPropertyInfo = validationContext.ObjectType.GetProperty(DependentProperty);

        if(dependentPropertyInfo is null)
        {
            return new ValidationResult($"Unknown property: {DependentProperty}.");
        }

        object? dependentValue = dependentPropertyInfo.GetValue(validationContext.ObjectInstance);

        if(!Equals(dependentValue, TargetValue))
        {
            return ValidationResult.Success;
        }

        if((value is null) || (DisallowEmptyStrings && (value is string { Length: 0 })))
        {
            return new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName, DependentProperty, TargetValue ?? "null"), (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
        }

        return ValidationResult.Success;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override string FormatErrorMessage(string name) { return string.Format(ErrorMessageString, name, DependentProperty, TargetValue ?? "null"); }
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
    ///Gets the value that <see cref="DependentProperty"/> must equal for the annotated property to be required.
    ///</summary>
    public object? TargetValue { get; }
    #endregion
}
