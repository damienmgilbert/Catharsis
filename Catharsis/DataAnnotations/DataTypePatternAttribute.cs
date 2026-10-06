using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Catharsis.DataAnnotations;

///<summary>
///Validates that a string value matches a well-known pattern derived from a <see cref="DataType"/> enumeration value.
///This combines the semantic classification of <see cref="DataTypeAttribute"/> with the enforcement behavior of <see
///cref="RegularExpressionAttribute"/>.
///</summary>
///<remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class DataTypePatternAttribute : ValidationAttribute
{
    #region Fields
    private static readonly FrozenDictionary<DataType, string> Patterns = new Dictionary<DataType, string>
    {
        [DataType.EmailAddress] = @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        [DataType.PhoneNumber] = @"^\+?[\d\s\-\(\)\.]{7,20}$",
        [DataType.PostalCode] = @"^\d{5}(-\d{4})?$",
        [DataType.Url] = @"^https?://[^\s/$.?#].[^\s]*$",
        [DataType.CreditCard] = @"^[\d\-\s]{13,25}$",
        [DataType.Currency] = @"^[\$€£¥]?\s?-?\d{1,3}(,?\d{3})*(\.\d{1,2})?$",
        [DataType.Date] = @"^\d{4}-\d{2}-\d{2}$",
        [DataType.Time] = @"^\d{2}:\d{2}(:\d{2})?$",
        [DataType.DateTime] = @"^\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}(:\d{2})?",
        [DataType.Duration] = @"^P(\d+Y)?(\d+M)?(\d+W)?(\d+D)?(T(\d+H)?(\d+M)?(\d+(\.\d+)?S)?)?$",
        [DataType.ImageUrl] = @"^https?://[^\s]+\.(jpg|jpeg|png|gif|bmp|svg|webp|ico)(\?[^\s]*)?$",
    }.ToFrozenDictionary();
    private readonly Regex _regex;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="DataTypePatternAttribute"/> for the specified <see cref="DataType"/>.
    ///</summary>
    ///<param name="dataType">The data type whose pattern to enforce.</param>
    ///<exception cref="ArgumentException">
    public DataTypePatternAttribute(DataType dataType) : base("The field {0} is not a valid {1}.")
    {
        if(!Patterns.TryGetValue(dataType, out string? pattern))
        {
            throw new ArgumentException($"No pattern is defined for DataType.{dataType}.", nameof(dataType));
        }

        DataType = dataType;
        Pattern = pattern;
        _regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if(value is null)
        {
            return ValidationResult.Success;
        }

        if(value is not string stringValue)
        {
            return new ValidationResult($"The field {validationContext.DisplayName} must be a string.", (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
        }

        if(!_regex.IsMatch(stringValue))
        {
            return new ValidationResult(string.Format(CultureInfo.CurrentCulture, ErrorMessageString, validationContext.DisplayName, DataType), (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
        }

        return ValidationResult.Success;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override string FormatErrorMessage(string name) => string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, DataType);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the <see cref="DataType"/> this attribute validates against.
    ///</summary>
    public DataType DataType { get; }

    ///<summary>
    ///Gets the regex pattern used for validation.
    ///</summary>
    public string Pattern { get; }
    #endregion
}
