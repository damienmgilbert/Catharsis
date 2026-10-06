using System.ComponentModel.DataAnnotations;

namespace Catharsis.DataAnnotations;

///<summary>
///Validates that a string value is a legal file name, free of characters returned by <see
///cref="Path.GetInvalidFileNameChars"/>. Optionally restricts the file name to a maximum length and a set of allowed
///extensions.
///</summary>
///<remarks>
///A <c>null</c> value is considered valid (combine with <see cref="RequiredAttribute"/> to disallow nulls).
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class FileNameAttribute : ValidationAttribute
{
    #region Fields
    static readonly HashSet<char> InvalidChars =
        [with(Path.GetInvalidFileNameChars())];
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="FileNameAttribute"/> with the default error message.
    ///</summary>
    public FileNameAttribute() : base("The field {0} must be a valid file name.")
    {
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not string fileName)
        {
            return new ValidationResult("The field must be a string.", (validationContext.MemberName is not null) ? [validationContext.MemberName] : null);
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), (validationContext.MemberName is not null) ? [validationContext.MemberName] : null);
        }

        foreach (char c in fileName)
        {
            if (InvalidChars.Contains(c))
            {
                return new ValidationResult($"The field {validationContext.DisplayName} contains an invalid character '{c}'.", (validationContext.MemberName is not null) ? [validationContext.MemberName] : null);
            }
        }

        if ((MaxLength > 0) && (fileName.Length > MaxLength))
        {
            return new ValidationResult($"The field {validationContext.DisplayName} must be at most {MaxLength} characters long.", (validationContext.MemberName is not null) ? [validationContext.MemberName] : null);
        }

        if (AllowedExtensions is { Length: > 0 })
        {
            string extension = Path.GetExtension(fileName);
            bool found = false;
            foreach (string allowed in AllowedExtensions)
            {
                if (string.Equals(extension, allowed, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return new ValidationResult($"The field {validationContext.DisplayName} must have one of the following extensions: {string.Join(", ", AllowedExtensions)}.", (validationContext.MemberName is not null) ? [validationContext.MemberName] : null);
            }
        }

        return ValidationResult.Success;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets or sets an optional set of permitted file extensions (e.g. <c>".txt", ".csv"</c>). When <c>null</c> or
    ///empty, any extension is accepted. Extensions are compared in an ordinal, case-insensitive manner.
    ///</summary>
    public string[]? AllowedExtensions { get; set; }

    ///<summary>
    ///Gets or sets the maximum allowed length of the file name (including extension). A value of <c>0</c> means no
    ///limit is enforced. Defaults to <c>0</c>.
    ///</summary>
    public int MaxLength { get; set; }
    #endregion
}
