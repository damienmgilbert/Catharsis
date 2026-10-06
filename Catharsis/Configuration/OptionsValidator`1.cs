namespace Catharsis.Configuration;

///<summary>
///Builds an <see cref="IOptionsValidator{TOptions}"/> out of a fluently registered set of rules, each a predicate
///paired with the failure message to report when the predicate returns <c>false</c>.
///</summary>
///<typeparam name="TOptions">The type of the options instance to validate.</typeparam>
public sealed class OptionsValidator<TOptions> : IOptionsValidator<TOptions>
{
    #region Fields
    readonly List<(Func<TOptions, bool> Predicate, string FailureMessage)> _rules = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Registers a validation rule.
    ///</summary>
    ///<param name="predicate">A predicate that must return <c>true</c> for the options instance to be considered valid.</param>
    ///<param name="failureMessage">The message reported when <paramref name="predicate"/> returns <c>false</c>.</param>
    ///<returns>This instance, to allow chaining multiple rules.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="failureMessage"/> is <c>null</c>, empty, or whitespace.</exception>
    public OptionsValidator<TOptions> AddRule(Func<TOptions, bool> predicate, string failureMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentException.ThrowIfNullOrWhiteSpace(failureMessage);

        _rules.Add((predicate, failureMessage));
        return this;
    }

    ///<inheritdoc/>
    public OptionsValidationResult Validate(TOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> failures = [.. _rules.Where(rule => !rule.Predicate(options)).Select(static rule => rule.FailureMessage)];
        return (failures.Count == 0) ? OptionsValidationResult.Success : OptionsValidationResult.Fail(failures);
    }
    #endregion
}
