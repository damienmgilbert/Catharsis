using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Catharsis.DataAnnotations;

///<summary>
///Validates that the annotated property's value is <strong>not</strong> equal to the value of another property on the
///same object. This is the logical inverse of <see cref="CompareAttribute"/>.
///</summary>
///<remarks>
///Equality is determined by <see cref="object.Equals(object?)"/>. Both values being <c>null</c> is treated as equal and
///therefore invalid.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class NotEqualToAttribute : ValidationAttribute
{
    #region Constructors
    ///<summary>
    ///Initializes a FileName instance of <see cref="NotEqualToAttribute"/> with the name of the property to compare
    ///against.
    ///</summary>
    ///<param name="otherProperty">The name of the property whose value must differ.</param>
    ///<exception cref="ArgumentNullException"><paramref name="otherProperty"/> is <c>null</c>.</exception>
    public NotEqualToAttribute(string otherProperty) : base("The field {0} must not equal {1}.")
    {
        ArgumentNullException.ThrowIfNull(otherProperty);
        OtherProperty = otherProperty;
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        PropertyInfo? otherPropertyInfo = validationContext.ObjectType.GetProperty(OtherProperty);
        if(otherPropertyInfo is null)
        {
            return new ValidationResult($"Unknown property: {OtherProperty}.");
        }

        object? otherValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance);

        if(Equals(value, otherValue))
        {
            string otherDisplayName = OtherPropertyDisplayName ?? OtherProperty;
            return new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName, otherDisplayName), (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
        }

        return ValidationResult.Success;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override string FormatErrorMessage(string name) { return string.Format(ErrorMessageString, name, OtherPropertyDisplayName ?? OtherProperty); }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the name of the other property to compare against.
    ///</summary>
    public string OtherProperty { get; }

    ///<summary>
    ///Gets or sets the display name of the other property used in error messages. When <c>null</c>, the property name
    ///is used as-is.
    ///</summary>
    public string? OtherPropertyDisplayName { get; set; }
    #endregion
}
