using System.ComponentModel.DataAnnotations;

namespace Catharsis.DataAnnotations;

///<summary>
///Validates that a string contains no whitespace characters at all, anywhere in the string.
///</summary>
///<remarks>
///A <c>null</c> or empty value is considered valid (combine with <see cref="RequiredAttribute"/> to disallow missing
///values). This differs from <see cref="TrimmedAttribute"/>, which only forbids leading and trailing whitespace:
///<c>"user name"</c> fails this attribute but passes <see cref="TrimmedAttribute"/>.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class NoWhitespaceAttribute : ValidationAttribute
{
    #region Constructors

    ///<summary>
    ///Initializes a new instance of <see cref="NoWhitespaceAttribute"/>.
    ///</summary>
    public NoWhitespaceAttribute() : base("The field {0} must not contain whitespace.")
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

        if((value is not string text) || text.Any(char.IsWhiteSpace))
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
        }

        return ValidationResult.Success;
    }
    #endregion
}
