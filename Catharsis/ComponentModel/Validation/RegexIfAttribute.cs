using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Catharsis.ComponentModel.Validation;

///<summary>
///Conditionally validates that the annotated property's string value matches a regular expression when a dependent
///property equals a target value.
///</summary>
///<remarks>
///When the condition defined by <see cref="DependentProperty"/> and <see cref="TargetValue"/> is not met, the property
///passes validation regardless of its value. Null values pass validation even when the condition is met; combine with
///<see cref="RequiredIfAttribute"/> to enforce non-null when needed.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class RegexIfAttribute : ValidationAttribute
{
    #region Fields
    readonly Regex _regex;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="RegexIfAttribute"/>.
    ///</summary>
    ///<param name="dependentProperty">The controlling property name.</param>
    ///<param name="targetValue">The value that triggers regex validation.</param>
    ///<param name="pattern">The regular expression pattern.</param>
    ///<param name="options">Optional regex options. Defaults to <see cref="RegexOptions.None"/>.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="dependentProperty"/> or <paramref name="pattern"/> is <c>null</c>.
    ///</exception>
    public RegexIfAttribute(string dependentProperty, object? targetValue, string pattern, RegexOptions options = RegexOptions.None) : base("The field {0} must match the pattern '{1}' when {2} equals {3}.")
    {
        ArgumentNullException.ThrowIfNull(dependentProperty);
        ArgumentNullException.ThrowIfNull(pattern);

        DependentProperty = dependentProperty;
        TargetValue = targetValue;
        Pattern = pattern;
        _regex = new Regex(pattern, options | RegexOptions.Compiled, TimeSpan.FromSeconds(2));
    }
    #endregion

    #region Private methods
    bool IsConditionMet(ValidationContext context)
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

        if(value is null)
        {
            return ValidationResult.Success;
        }

        string? text = value as string ?? value.ToString();

        if((text is not null) && _regex.IsMatch(text))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override string FormatErrorMessage(string name) { return string.Format(ErrorMessageString, name, Pattern, DependentProperty, TargetValue ?? "null"); }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the name of the property whose value determines whether the regex check is applied.
    ///</summary>
    public string DependentProperty { get; }

    ///<summary>
    ///Gets the regular expression pattern.
    ///</summary>
    public string Pattern { get; }

    ///<summary>
    ///Gets the value that <see cref="DependentProperty"/> must equal for the regex check to apply.
    ///</summary>
    public object? TargetValue { get; }
    #endregion
}
