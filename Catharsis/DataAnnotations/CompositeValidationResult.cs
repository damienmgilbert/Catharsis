using System.ComponentModel.DataAnnotations;

namespace Catharsis.DataAnnotations;

///<summary>
///Represents the aggregated outcome of a composite validation operation, providing convenient access to errors, member
///names, and an overall success/failure indicator.
///</summary>
public sealed class CompositeValidationResult
{
    #region Fields
    private readonly IReadOnlyList<ValidationResult> _results;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="CompositeValidationResult"/> with the collected validation results.
    ///</summary>
    ///<param name="results">The validation results.</param>
    internal CompositeValidationResult(IReadOnlyList<ValidationResult> results) { _results = results; }
    #endregion

    #region Public methods
    ///<summary>
    ///Throws a <see cref="ValidationException"/> if the result contains any errors. This mirrors the behavior of <see
    ///cref="Validator.ValidateObject"/>.
    ///</summary>
    ///<exception cref="ValidationException">Validation failed.</exception>
    public void ThrowIfInvalid()
    {
        if(_results.Count > 0)
        {
            throw new ValidationException(_results[0], validatingAttribute: null, value: null);
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets all distinct error messages.
    ///</summary>
    public IEnumerable<string> ErrorMessages => _results.Select(static r => r.ErrorMessage).Where(static m => m is not null)!;

    ///<summary>
    ///Gets a value indicating whether validation passed with no errors.
    ///</summary>
    public bool IsValid => _results.Count == 0;

    ///<summary>
    ///Gets all distinct member names that had validation failures.
    ///</summary>
    public IEnumerable<string> MemberNames => _results.SelectMany(static r => r.MemberNames).Distinct();

    ///<summary>
    ///Gets all <see cref="ValidationResult"/> instances produced by validation.
    ///</summary>
    public IReadOnlyList<ValidationResult> Results => _results;
    #endregion
}
