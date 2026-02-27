using System.ComponentModel.DataAnnotations;

namespace Catharsis.ComponentModel.Validation;

///<summary>
///Defines a single, reusable validation rule that can be composed into a <see cref="ValidationPipeline"/>.
///</summary>
///<remarks>
///Implementations should be stateless and thread-safe. Use <see cref="Scope"/> and <see cref="Severity"/> to describe
///the nature of the rule so consumers can filter or group results accordingly.
///</remarks>
public interface IValidationRule
{
    #region Public methods
    ///<summary>
    ///Validates the specified value within the given context.
    ///</summary>
    ///<param name="value">The value to validate.</param>
    ///<param name="context">The validation context providing metadata.</param>
    ///<returns>
    ///<see cref="ValidationResult.Success"/> if the rule is satisfied; otherwise, a <see cref="ValidationResult"/>
    ///describing the violation.
    ///</returns>
    ValidationResult? Validate(object? value, ValidationContext context);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the scope at which this rule operates.
    ///</summary>
    ValidationScope Scope { get; }

    ///<summary>
    ///Gets the default severity for violations produced by this rule.
    ///</summary>
    ValidationSeverity Severity { get; }
    #endregion
}
