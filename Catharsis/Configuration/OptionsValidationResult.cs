namespace Catharsis.Configuration;

///<summary>
///The outcome of validating an options instance: either success, or a list of human-readable failure messages.
///</summary>
public sealed class OptionsValidationResult
{
    #region Fields
    private static readonly OptionsValidationResult _success = new(true, []);
    #endregion

    #region Constructors
    private OptionsValidationResult(bool succeeded, IReadOnlyList<string> failures)
    {
        Succeeded = succeeded;
        Failures = failures;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a failed result with the specified failure messages.
    ///</summary>
    ///<param name="failures">The failure messages describing why validation failed.</param>
    ///<returns>A failed <see cref="OptionsValidationResult"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="failures"/> is <c>null</c>.</exception>
    public static OptionsValidationResult Fail(IReadOnlyList<string> failures)
    {
        ArgumentNullException.ThrowIfNull(failures);
        return new OptionsValidationResult(false, failures);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the failure messages. Empty when <see cref="Succeeded"/> is <c>true</c>.
    ///</summary>
    public IReadOnlyList<string> Failures { get; }

    ///<summary>
    ///Gets whether validation succeeded.
    ///</summary>
    public bool Succeeded { get; }

    ///<summary>
    ///Gets a successful result with no failure messages.
    ///</summary>
    public static OptionsValidationResult Success => _success;
    #endregion
}
