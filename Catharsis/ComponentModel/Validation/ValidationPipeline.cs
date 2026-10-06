using System.ComponentModel.DataAnnotations;

namespace Catharsis.ComponentModel.Validation;

///<summary>
///Executes a sequence of <see cref="IValidationRule"/> instances against a value, collecting all results into a <see
///cref="ValidationResultAggregator"/>.
///</summary>
///<remarks>
///Rules are evaluated in registration order. Use <see cref="StopOnFirstError"/> to short-circuit evaluation after the
///first error-severity failure.
///</remarks>
public sealed class ValidationPipeline
{
    #region Fields
    readonly List<IValidationRule> _rules = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a rule to the end of the pipeline.
    ///</summary>
    ///<param name="rule">The rule to add.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="rule"/> is <c>null</c>.
    ///</exception>
    public ValidationPipeline AddRule(IValidationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _rules.Add(rule);
        return this;
    }

    ///<summary>
    ///Adds multiple rules to the pipeline.
    ///</summary>
    ///<param name="rules">The rules to add.</param>
    ///<returns>This instance, for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="rules"/> is <c>null</c>.
    ///</exception>
    public ValidationPipeline AddRules(IEnumerable<IValidationRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        foreach(IValidationRule rule in rules)
        {
            ArgumentNullException.ThrowIfNull(rule);
            _rules.Add(rule);
        }

        return this;
    }

    ///<summary>
    ///Removes all rules from the pipeline.
    ///</summary>
    public void Clear() { _rules.Clear(); }

    ///<summary>
    ///Executes all rules against the specified value and context.
    ///</summary>
    ///<param name="value">The value to validate.</param>
    ///<param name="context">The validation context.</param>
    ///<returns>
    ///A <see cref="ValidationResultAggregator"/> containing all results.
    ///</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="context"/> is <c>null</c>.
    ///</exception>
    public ValidationResultAggregator Execute(object? value, ValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        ValidationResultAggregator aggregator = new();

        foreach(IValidationRule rule in _rules)
        {
            ValidationResult? result = rule.Validate(value, context);
            aggregator.Add(result, rule.Severity);

            if(StopOnFirstError && (rule.Severity == ValidationSeverity.Error) && (result is not null) && (result != ValidationResult.Success))
            {
                break;
            }
        }

        return aggregator;
    }

    ///<summary>
    ///Executes all rules and returns a value indicating whether all error-severity rules passed.
    ///</summary>
    ///<param name="value">The value to validate.</param>
    ///<param name="context">The validation context.</param>
    ///<returns><c>true</c> if no errors were produced; otherwise, <c>false</c>.</returns>
    public bool IsValid(object? value, ValidationContext context) { return !Execute(value, context).HasErrors; }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of rules in the pipeline.
    ///</summary>
    public int Count => _rules.Count;

    ///<summary>
    ///Gets or sets a value indicating whether the pipeline should stop evaluating rules after the first error-severity
    ///result. Defaults to <c>false</c>.
    ///</summary>
    public bool StopOnFirstError { get; set; }
    #endregion
}
