namespace Catharsis.Resilience;

///<summary>
///Wraps an operation with a fallback value or delegate that runs when the operation fails, so that callers can degrade
///gracefully instead of propagating the failure.
///</summary>
///<typeparam name="TResult">The result type produced by the operation and its fallback.</typeparam>
///<param name="predicate">
///A predicate that returns <c>true</c> if the exception should trigger the fallback, or <c>null</c> to fall back on any
///exception.
///</param>
///<example>
public sealed class FallbackPolicy<TResult>(Func<Exception, bool>? predicate = null)
{
    #region Fields
    private readonly Func<Exception, bool> _predicate = predicate ?? (static _ => true);
    #endregion

    #region Public methods
    ///<summary>
    ///Executes the specified operation, returning the fallback value on a matching failure.
    ///</summary>
    ///<param name="operation">The operation to execute.</param>
    ///<param name="fallbackValue">The value to return if the operation fails.</param>
    ///<returns>The result of the operation, or <paramref name="fallbackValue"/> on a matching failure.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    public TResult Execute(Func<TResult> operation, TResult fallbackValue) => Execute(operation, _ => fallbackValue);

    ///<summary>
    ///Executes the specified operation, invoking the fallback delegate on a matching failure.
    ///</summary>
    ///<param name="operation">The operation to execute.</param>
    ///<param name="fallback">A delegate that produces the fallback result from the exception that occurred.</param>
    ///<returns>The result of the operation, or the fallback delegate's result on a matching failure.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> or <paramref name="fallback"/> is <c>null</c>.</exception>
    public TResult Execute(Func<TResult> operation, Func<Exception, TResult> fallback)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(fallback);

        try
        {
            return operation();
        } catch(Exception ex) when(_predicate(ex))
        {
            return fallback(ex);
        }
    }

    ///<summary>
    ///Asynchronously executes the specified operation, returning the fallback value on a matching failure.
    ///</summary>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="fallbackValue">The value to return if the operation fails.</param>
    ///<param name="cancellationToken">A cancellation token passed to the operation.</param>
    ///<returns>The result of the operation, or <paramref name="fallbackValue"/> on a matching failure.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> operation, TResult fallbackValue, CancellationToken cancellationToken = default) => ExecuteAsync(operation, _ => Task.FromResult(fallbackValue), cancellationToken);

    ///<summary>
    ///Asynchronously executes the specified operation, invoking the fallback delegate on a matching failure.
    ///</summary>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="fallback">A delegate that produces the fallback result from the exception that occurred.</param>
    ///<param name="cancellationToken">A cancellation token passed to the operation.</param>
    ///<returns>The result of the operation, or the fallback delegate's result on a matching failure.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> or <paramref name="fallback"/> is <c>null</c>.</exception>
    public async Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> operation, Func<Exception, Task<TResult>> fallback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(fallback);

        try
        {
            return await operation(cancellationToken).ConfigureAwait(false);
        } catch(Exception ex) when(ex is not OperationCanceledException && _predicate(ex))
        {
            return await fallback(ex).ConfigureAwait(false);
        }
    }
    #endregion
}
