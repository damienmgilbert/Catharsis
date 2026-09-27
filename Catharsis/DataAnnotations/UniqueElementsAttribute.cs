using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.DataAnnotations;

///<summary>
///Validates that all elements in an <see cref="IEnumerable"/> are unique. Duplicates are detected using the default
///equality comparer for the element type.
///</summary>
///<remarks>
///A <c>null</c> value is considered valid (combine with <see cref="RequiredAttribute"/> to disallow nulls). An empty
///collection is always valid.
///</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class UniqueElementsAttribute : ValidationAttribute
{
    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="UniqueElementsAttribute"/> with the default error message.
    ///</summary>
    public UniqueElementsAttribute() : base("The field {0} must contain only unique elements.")
    {
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

        if(value is not IEnumerable enumerable)
        {
            return new ValidationResult($"The field {validationContext.DisplayName} must be a collection.", (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
        }

        HashSet<object?> seen = [];
        int index = 0;

        foreach(object? element in enumerable)
        {
            if(!seen.Add(element))
            {
                return new ValidationResult($"The field {validationContext.DisplayName} contains a duplicate element at index {index}.", (validationContext.MemberName is not null) ? [ validationContext.MemberName ] : null);
            }

            index++;
        }

        return ValidationResult.Success;
    }
    #endregion
}
