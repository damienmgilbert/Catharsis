using System.ComponentModel.DataAnnotations;

namespace Catharsis.DataAnnotations;

///<summary>
///Validates that a <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, or <see cref="DateOnly"/> value lies in the
///past relative to the moment of validation.
///</summary>
///<remarks>
///A <c>null</c> value is considered valid (combine with <see cref="RequiredAttribute"/> to disallow nulls). If the
///value is none of the supported date types, validation fails.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class PastDateAttribute : ValidationAttribute
{
    #region Constructors

    ///<summary>
    ///Initializes a new instance of <see cref="PastDateAttribute"/>.
    ///</summary>
    public PastDateAttribute() : base("The field {0} must be a date in the past.")
    {
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        bool isValid = value switch
        {
            null => true,
            DateTime dateTime => dateTime < (UseUtc ? DateTime.UtcNow : DateTime.Now),
            DateTimeOffset dateTimeOffset => dateTimeOffset < DateTimeOffset.Now,
            DateOnly dateOnly => dateOnly < DateOnly.FromDateTime(UseUtc ? DateTime.UtcNow : DateTime.Now),
            _ => false
        };

        return isValid ? ValidationResult.Success : new ValidationResult(FormatErrorMessage(validationContext.DisplayName), (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets or sets a value indicating whether <see cref="DateTime"/> and <see cref="DateOnly"/> values are compared
    ///against <see cref="DateTime.UtcNow"/> instead of <see cref="DateTime.Now"/>. Defaults to <c>false</c>.
    ///</summary>
    public bool UseUtc { get; set; }
    #endregion
}
