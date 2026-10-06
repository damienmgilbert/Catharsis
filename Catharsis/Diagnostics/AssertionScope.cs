namespace Catharsis.Diagnostics;

///<summary>
///Collects multiple validation failures so they can be reported together instead of stopping at the first one.
///</summary>
///<remarks>
///Disposing the scope throws an <see cref="AggregateException"/> containing every recorded failure, if any were
///recorded. This deliberately throws from <see cref="Dispose"/>: the entire point of a batch-assert scope is that
///the caller never has to remember to call <see cref="ThrowIfAny"/> explicitly — wrapping usage in a <c>using</c>
///block is enough. Call <see cref="ThrowIfAny"/> directly instead if throwing from <see cref="Dispose"/> is
///undesirable in a particular context.
///</remarks>
public sealed class AssertionScope : IDisposable
{
    #region Fields
    readonly List<string> _failures = [];
    bool _thrown;
    #endregion

    #region Public methods
    ///<summary>
    ///Records a failure with the specified message if <paramref name="condition"/> is <c>false</c>.
    ///</summary>
    ///<param name="condition">The condition to check.</param>
    ///<param name="message">The failure message to record when <paramref name="condition"/> is <c>false</c>.</param>
    ///<returns>This instance, to allow chaining multiple checks.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="message"/> is <c>null</c>.</exception>
    public AssertionScope Check(bool condition, string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!condition)
        {
            _failures.Add(message);
        }

        return this;
    }

    ///<summary>
    ///Disposes the scope, throwing an <see cref="AggregateException"/> if any failures were recorded and
    ///<see cref="ThrowIfAny"/> has not already been called.
    ///</summary>
    ///<exception cref="AggregateException">One or more failures were recorded.</exception>
    public void Dispose() => ThrowIfAny();

    ///<summary>
    ///Throws an <see cref="AggregateException"/> containing every recorded failure, if any were recorded. Calling
    ///this more than once, or calling it before <see cref="Dispose"/>, has no further effect once the failures have
    ///already been thrown.
    ///</summary>
    ///<exception cref="AggregateException">One or more failures were recorded.</exception>
    public void ThrowIfAny()
    {
        if (_thrown || (_failures.Count == 0))
        {
            return;
        }

        _thrown = true;
        throw new AggregateException($"{_failures.Count} assertion(s) failed.", _failures.Select(static message => new InvalidOperationException(message)));
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the failure messages recorded so far.
    ///</summary>
    public IReadOnlyList<string> Failures => _failures;

    ///<summary>
    ///Gets a value indicating whether any failures have been recorded.
    ///</summary>
    public bool HasFailures => _failures.Count > 0;
    #endregion
}
