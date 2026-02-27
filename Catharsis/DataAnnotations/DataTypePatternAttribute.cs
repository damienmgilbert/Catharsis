using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Catharsis.DataAnnotations;

/// <summary>
/// Validates that a string value matches a well-known pattern derived from a
/// <see cref="DataType"/> enumeration value. This combines the semantic
/// classification of <see cref="DataTypeAttribute"/> with the enforcement
/// behavior of <see cref="RegularExpressionAttribute"/>.
/// </summary>
/// <remarks>
/// <para>
/// Supported <see cref="DataType"/> values and their patterns:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>DataType</term>
///     <description>Pattern</description>
///   </listheader>
///   <item><term><see cref="DataType.EmailAddress"/></term><description>RFC 5322 simplified</description></item>
///   <item><term><see cref="DataType.PhoneNumber"/></term><description>Digits, spaces, dashes, parens, plus</description></item>
///   <item><term><see cref="DataType.PostalCode"/></term><description>US 5-digit or 5+4 ZIP codes</description></item>
///   <item><term><see cref="DataType.Url"/></term><description>http/https URL</description></item>
///   <item><term><see cref="DataType.CreditCard"/></term><description>13-19 digit Luhn-eligible card numbers</description></item>
///   <item><term><see cref="DataType.Currency"/></term><description>Currency amount with optional symbol</description></item>
///   <item><term><see cref="DataType.Date"/></term><description>ISO 8601 date (yyyy-MM-dd)</description></item>
///   <item><term><see cref="DataType.Time"/></term><description>ISO 8601 time (HH:mm or HH:mm:ss)</description></item>
///   <item><term><see cref="DataType.DateTime"/></term><description>ISO 8601 date-time</description></item>
///   <item><term><see cref="DataType.Duration"/></term><description>ISO 8601 duration (e.g. P1DT2H)</description></item>
///   <item><term><see cref="DataType.ImageUrl"/></term><description>URL ending in image extension</description></item>
/// </list>
/// <para>A <c>null</c> value is considered valid.</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public sealed class DataTypePatternAttribute : ValidationAttribute
{
    private static readonly Dictionary<DataType, string> Patterns = new()
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
    };

    private readonly Regex _regex;

    /// <summary>
    /// Gets the <see cref="DataType"/> this attribute validates against.
    /// </summary>
    public DataType DataType { get; }

    /// <summary>
    /// Gets the regex pattern used for validation.
    /// </summary>
    public string Pattern { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DataTypePatternAttribute"/>
    /// for the specified <see cref="DataType"/>.
    /// </summary>
    /// <param name="dataType">The data type whose pattern to enforce.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="dataType"/> does not have a known pattern.
    /// </exception>
    public DataTypePatternAttribute(DataType dataType)
        : base("The field {0} is not a valid {1}.")
    {
        if (!Patterns.TryGetValue(dataType, out string? pattern))
        {
            throw new ArgumentException(
                $"No pattern is defined for DataType.{dataType}.",
                nameof(dataType));
        }

        DataType = dataType;
        Pattern = pattern;
        _regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(2));
    }

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string stringValue)
        {
            return new ValidationResult(
                $"The field {validationContext.DisplayName} must be a string.",
                validationContext.MemberName is not null
                    ? [validationContext.MemberName]
                    : null);
        }

        if (!_regex.IsMatch(stringValue))
        {
            return new ValidationResult(
                string.Format(ErrorMessageString, validationContext.DisplayName, DataType),
                validationContext.MemberName is not null
                    ? [validationContext.MemberName]
                    : null);
        }

        return ValidationResult.Success;
    }

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) =>
        string.Format(ErrorMessageString, name, DataType);
}
