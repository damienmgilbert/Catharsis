using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace Catharsis.DataAnnotations;

///<summary>
///A class-level validation attribute that ensures at most one of the specified properties has a non-null (non-default)
///value. This is useful for modelling exclusive option groups (e.g. "specify either <c>FilePath</c> or <c>Url</c>, but
///not both").
///</summary>
///<remarks>
///<para> Apply this attribute to a class and supply two or more property names. Validation fails when more than one of
///those properties has a non-null value. For strings, empty/whitespace-only values are treated as absent.</para> <para>
///This attribute works in conjunction with <see cref="Validator.TryValidateObject"/> when <c>validateAllProperties</c>
///is <c>true</c>.</para>
///</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class MutuallyExclusiveAttribute : ValidationAttribute
{
    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="MutuallyExclusiveAttribute"/> with the property names that must be
    ///mutually exclusive.
    ///</summary>
    ///<param name="propertyNames">
    ///Two or more property names. At most one may have a value at any time.
    ///</param>
    ///<exception cref="ArgumentException">
    ///Fewer than two property names were supplied.
    ///</exception>
    public MutuallyExclusiveAttribute(params string[] propertyNames) : base("Only one of the following may be specified: {0}.")
    {
        if(propertyNames.Length < 2)
        {
            throw new ArgumentException("At least two property names are required for mutual exclusion.", nameof(propertyNames));
        }

        PropertyNames = propertyNames;
    }
    #endregion

    #region Private methods
    static bool HasValue(object? value)
    {
        return value switch
        {
            null => false,
            string s => !string.IsNullOrWhiteSpace(s),
            _ => true
        };
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

        List<string> populated = [];

        foreach(string propertyName in PropertyNames)
        {
            PropertyInfo? property = value.GetType().GetProperty(propertyName);
            if(property is null)
            {
                return new ValidationResult($"Unknown property: {propertyName}.");
            }

            object? propertyValue = property.GetValue(value);

            if(HasValue(propertyValue))
            {
                populated.Add(propertyName);
            }
        }

        if(populated.Count > 1)
        {
            string group = GroupName ?? string.Join(", ", PropertyNames);
            return new ValidationResult(string.Format(CultureInfo.CurrentCulture, ErrorMessageString, group), populated);
        }

        return ValidationResult.Success;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override string FormatErrorMessage(string name) { return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, GroupName ?? string.Join(", ", PropertyNames)); }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets or sets the logical group name for this set of exclusive properties. Used in error messages to describe the
    ///group.
    ///</summary>
    public string? GroupName { get; set; }

    ///<summary>
    ///Gets the names of the mutually exclusive properties.
    ///</summary>
    public string[] PropertyNames { get; }
    #endregion
}
