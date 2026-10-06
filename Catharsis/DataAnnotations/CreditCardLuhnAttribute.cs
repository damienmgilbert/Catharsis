using System.ComponentModel.DataAnnotations;

namespace Catharsis.DataAnnotations;

///<summary>
///Validates that a string of digits passes the Luhn checksum algorithm used by credit card numbers and several other
///identifier schemes.
///</summary>
///<remarks>
///A <c>null</c> or empty value is considered valid (combine with <see cref="RequiredAttribute"/> to disallow missing
///values). Whitespace and hyphens are ignored before the checksum is computed, so <c>"4111 1111 1111 1111"</c> and
///<c>"4111-1111-1111-1111"</c> both validate the same as <c>"4111111111111111"</c>. This checks the checksum only; it
///does not verify the number is an actually-issued card.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class CreditCardLuhnAttribute : ValidationAttribute
{
    #region Constructors

    ///<summary>
    ///Initializes a new instance of <see cref="CreditCardLuhnAttribute"/>.
    ///</summary>
    public CreditCardLuhnAttribute() : base("The field {0} is not a valid credit card number.")
    {
    }
    #endregion

    #region Private methods
    private static bool PassesLuhnCheck(string digits)
    {
        int sum = 0;
        bool doubleDigit = false;

        for(int index = digits.Length - 1; index >= 0; index--)
        {
            int digit = digits[index] - '0';

            if(doubleDigit)
            {
                digit *= 2;

                if(digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
            doubleDigit = !doubleDigit;
        }

        return (sum % 10) == 0;
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

        if((value is not string text) || !text.All(static c => char.IsDigit(c) || (c == ' ') || (c == '-')))
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
        }

        string digits = new([ .. text.Where(char.IsDigit) ]);

        bool isValid = (digits.Length >= 2) && PassesLuhnCheck(digits);

        return isValid ? ValidationResult.Success : new ValidationResult(FormatErrorMessage(validationContext.DisplayName), (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
    }
    #endregion
}
