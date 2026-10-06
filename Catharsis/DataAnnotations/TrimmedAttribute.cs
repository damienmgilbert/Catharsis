using System.ComponentModel.DataAnnotations;

namespace Catharsis.DataAnnotations;

///<summary>
///Validates that a string has no leading or trailing whitespace, i.e. that it is already equal to its own ///<see
///cref="string.Trim()"/> result.
///</summary>
///<remarks>
///A <c>null</c> or empty value is considered valid (combine with <see cref="RequiredAttribute"/> to disallow missing
///values). This differs from <see cref="NoWhitespaceAttribute"/>, which forbids whitespace anywhere in the string:
///interior whitespace such as <c>"user name"</c> passes this attribute but fails ///<see
///cref="NoWhitespaceAttribute"/>.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class TrimmedAttribute : ValidationAttribute
{
    #region Constructors

    ///<summary>
    ///Initializes a new instance of <see cref="TrimmedAttribute"/>.
    ///</summary>
    public TrimmedAttribute() : base("The field {0} must not have leading or trailing whitespace.")
    {
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if(value is null or string { Length: 0 })
        {
            return ValidationResult.Success;
        }

        if((value is not string text) || (text.AsSpan().Trim().Length != text.Length))
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
        }

        return ValidationResult.Success;
    }
    #endregion
}
