namespace Catharsis.DesignPatterns.Enterprise;

///<summary>
///Implements the Circuit Breaker design pattern: a minimal scaffold showing the pattern's shape (closed → open on
///repeated failure → calls rejected until the breaker resets). For a production-ready implementation with a
///half-open probing state, use <see cref="Catharsis.Resilience.CircuitBreaker"/> instead.
///</summary>
///<param name="failureThreshold">The number of consecutive failures that opens the breaker.</param>
///<exception cref="ArgumentOutOfRangeException"><paramref name="failureThreshold"/> is not positive.</exception>
public sealed class CircuitBreakerPattern(int failureThreshold = 3)
{
    #region Fields
    readonly int _failureThreshold = failureThreshold > 0 ? failureThreshold : throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Failure threshold must be positive.");
    int _consecutiveFailures;
    #endregion

    #region Public methods
    ///<summary>
    ///Runs <paramref name="action"/> if the breaker is closed, tracking failures and opening the breaker once
    ///<see cref="IsOpen"/> would flip after <c>failureThreshold</c> consecutive failures.
    ///</summary>
    ///<typeparam name="T">The type of the result produced by <paramref name="action"/>.</typeparam>
    ///<param name="action">The action to run.</param>
    ///<returns>The result of <paramref name="action"/>.</returns>
    ///<exception cref="InvalidOperationException">The breaker is currently open.</exception>
    public T Execute<T>(Func<T> action)
    {
        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        if(IsOpen)
        {
            throw new InvalidOperationException("The circuit breaker is open.");
        }

        try
        {
            T result = action();
            _consecutiveFailures = 0;
            return result;
        } catch
        {
            _consecutiveFailures++;
            throw;
        }
    }

    ///<summary>
    ///Manually resets the breaker to closed, clearing the consecutive failure count.
    ///</summary>
    public void Reset() => _consecutiveFailures = 0;
    #endregion

    #region Public properties
    ///<summary>
    ///Gets whether the breaker is currently open (rejecting calls) because of consecutive failures.
    ///</summary>
    public bool IsOpen => _consecutiveFailures >= _failureThreshold;
    #endregion
}
