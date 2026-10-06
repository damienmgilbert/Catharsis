using System.ComponentModel.DataAnnotations;

namespace Catharsis.DataAnnotations;

///<summary>
///Validates that an integral or enum value corresponds to a defined member of the specified enum type, guarding
///against values that were unchecked-cast or deserialized from outside the enum's declared range.
///</summary>
///<remarks>
///A <c>null</c> value is considered valid (combine with <see cref="RequiredAttribute"/> to disallow nulls). Values
///of an enum decorated with <see cref="FlagsAttribute"/> are validated bit-combination-aware via
///<see cref="Enum.IsDefined(Type, object)"/> semantics, so a value composed only of defined flag bits is accepted
///even when that exact combination is not itself a named member.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class EnumRangeAttribute : ValidationAttribute
{
    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="EnumRangeAttribute"/>.
    ///</summary>
    ///<param name="enumType">The enum type that the value must belong to.</param>
    ///<exception cref="ArgumentNullException"><paramref name="enumType"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="enumType"/> is not an enum type.</exception>
    public EnumRangeAttribute(Type enumType) : base("The field {0} does not contain a valid value for {1}.")
    {
        ArgumentNullException.ThrowIfNull(enumType);

        if (!enumType.IsEnum)
        {
            throw new ArgumentException("The specified type must be an enum type.", nameof(enumType));
        }

        EnumType = enumType;
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

        bool isValid;

        try
        {
            isValid = Enum.IsDefined(EnumType, value) || IsFlagsCombinationDefined(value);
        }
        catch (ArgumentException)
        {
            isValid = false;
        }

        return isValid ? ValidationResult.Success : new ValidationResult(FormatErrorMessage(validationContext.DisplayName), (validationContext.MemberName is not null) ? [validationContext.MemberName] : null);
    }
    #endregion

    #region Private methods
    bool IsFlagsCombinationDefined(object value)
    {
        if (Attribute.GetCustomAttribute(EnumType, typeof(FlagsAttribute)) is null)
        {
            return false;
        }

        try
        {
            long numericValue = Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
            long allDefinedBits = 0;

            foreach (object definedValue in Enum.GetValues(EnumType))
            {
                allDefinedBits |= Convert.ToInt64(definedValue, System.Globalization.CultureInfo.InvariantCulture);
            }

            return (numericValue & ~allDefinedBits) == 0;
        }
        catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException)
        {
            return false;
        }
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public override string FormatErrorMessage(string name) { return string.Format(System.Globalization.CultureInfo.CurrentCulture, ErrorMessageString, name, EnumType.Name); }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the enum type that the value must belong to.
    ///</summary>
    public Type EnumType { get; }
    #endregion
}
