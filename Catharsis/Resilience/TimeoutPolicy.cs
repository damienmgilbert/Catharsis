namespace Catharsis.Resilience;

///<summary>
///Wraps an async operation with a maximum execution duration, canceling it and raising <see cref="TimeoutException"/>
///if it does not complete in time.
///</summary>
///<example>
public sealed class TimeoutPolicy : IAsyncPolicy
{
    #region Fields
    private readonly TimeSpan _timeout;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a timeout policy with the specified maximum duration.
    ///</summary>
    ///<param name="timeout">The maximum duration allowed for an operation.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is not greater than zero.</exception>
    public TimeoutPolicy(TimeSpan timeout)
    {
        if(timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
        }

        _timeout = timeout;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Asynchronously executes the specified operation, canceling it if it exceeds the configured timeout.
    ///</summary>
    ///<typeparam name="TResult">The return type of the operation.</typeparam>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/> that is canceled on timeout.</param>
    ///<param name="cancellationToken">A cancellation token that can abandon the operation independently of the timeout.</param>
    ///<returns>The result of the successful operation.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="TimeoutException">The operation did not complete within the configured timeout.</exception>
    public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(_timeout);

        try
        {
            return await operation(linked.Token).ConfigureAwait(false);
        } catch(OperationCanceledException) when(!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"The operation did not complete within {_timeout}.");
        }
    }

    ///<summary>
    ///Asynchronously executes the specified operation, canceling it if it exceeds the configured timeout.
    ///</summary>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/> that is canceled on timeout.</param>
    ///<param name="cancellationToken">A cancellation token that can abandon the operation independently of the timeout.</param>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    ///<exception cref="TimeoutException">The operation did not complete within the configured timeout.</exception>
    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteAsync<object?>(
              async ct =>
              {
                  await operation(ct).ConfigureAwait(false);
                  return null;
              },
              cancellationToken)
            .ConfigureAwait(false);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the configured maximum duration.
    ///</summary>
    public TimeSpan Timeout => _timeout;
    #endregion
}
