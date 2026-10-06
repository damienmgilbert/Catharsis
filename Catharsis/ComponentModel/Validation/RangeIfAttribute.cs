using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace Catharsis.ComponentModel.Validation;

///<summary>
///Conditionally validates that the annotated property's value falls within a specified range when a dependent property
///equals a target value.
///</summary>
///<remarks>
///When the condition defined by <see cref="DependentProperty"/> and <see cref="TargetValue"/> is not met, the property
///passes validation regardless of its value.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class RangeIfAttribute : ValidationAttribute
{
    #region Fields
    private readonly RangeAttribute _inner;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="RangeIfAttribute"/> for integer ranges.
    ///</summary>
    ///<param name="dependentProperty">The controlling property name.</param>
    ///<param name="targetValue">The value that triggers range validation.</param>
    ///<param name="minimum">The minimum allowed value (inclusive).</param>
    ///<param name="maximum">The maximum allowed value (inclusive).</param>
    ///<exception cref="ArgumentNullException">
    public RangeIfAttribute(string dependentProperty, object? targetValue, int minimum, int maximum) : base("The field {0} must be between {1} and {2} when {3} equals {4}.")
    {
        ArgumentNullException.ThrowIfNull(dependentProperty);
        DependentProperty = dependentProperty;
        TargetValue = targetValue;
        _inner = new RangeAttribute(minimum, maximum);
    }

    ///<summary>
    ///Initializes a new instance of <see cref="RangeIfAttribute"/> for double ranges.
    ///</summary>
    ///<param name="dependentProperty">The controlling property name.</param>
    ///<param name="targetValue">The value that triggers range validation.</param>
    ///<param name="minimum">The minimum allowed value (inclusive).</param>
    ///<param name="maximum">The maximum allowed value (inclusive).</param>
    ///<exception cref="ArgumentNullException">
    public RangeIfAttribute(string dependentProperty, object? targetValue, double minimum, double maximum) : base("The field {0} must be between {1} and {2} when {3} equals {4}.")
    {
        ArgumentNullException.ThrowIfNull(dependentProperty);
        DependentProperty = dependentProperty;
        TargetValue = targetValue;
        _inner = new RangeAttribute(minimum, maximum);
    }
    #endregion

    #region Private methods
    private bool IsConditionMet(ValidationContext context)
    {
        PropertyInfo? dependentProp = context.ObjectType.GetProperty(DependentProperty);

        if(dependentProp is null)
        {
            return false;
        }

        object? dependentValue = dependentProp.GetValue(context.ObjectInstance);
        return Equals(dependentValue, TargetValue);
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if(!IsConditionMet(validationContext))
        {
            return ValidationResult.Success;
        }

        ValidationResult? innerResult = _inner.GetValidationResult(value, validationContext);

        if((innerResult is null) || (innerResult == ValidationResult.Success))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override string FormatErrorMessage(string name) => string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Minimum, Maximum, DependentProperty, TargetValue ?? "null");
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the name of the property whose value determines whether the range check is applied.
    ///</summary>
    public string DependentProperty { get; }

    ///<summary>
    ///Gets the maximum allowed value.
    ///</summary>
    public object Maximum => _inner.Maximum;

    ///<summary>
    ///Gets the minimum allowed value.
    ///</summary>
    public object Minimum => _inner.Minimum;

    ///<summary>
    ///Gets the value that <see cref="DependentProperty"/> must equal for the range check to apply.
    ///</summary>
    public object? TargetValue { get; }
    #endregion
}
