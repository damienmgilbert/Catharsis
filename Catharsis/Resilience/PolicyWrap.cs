namespace Catharsis.Resilience;

///<summary>
///Composes multiple <see cref="IAsyncPolicy"/> instances into a single pipeline, applying them outermost-first around
///the wrapped operation. For example, wrapping a <see cref="CircuitBreaker"/> around a ///<see cref="RetryPolicy"/>
///means each individual retry attempt passes through the circuit breaker.
///</summary>
///<example>
public sealed class PolicyWrap : IAsyncPolicy
{
    #region Fields
    private readonly IAsyncPolicy[] _policies;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a policy that applies the specified policies in order, outermost first.
    ///</summary>
    ///<param name="policies">The policies to compose, from outermost to innermost.</param>
    ///<exception cref="ArgumentNullException"><paramref name="policies"/>, or one of its elements, is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="policies"/> is empty.</exception>
    public PolicyWrap(params IAsyncPolicy[] policies)
    {
        ArgumentNullException.ThrowIfNull(policies);

        if(policies.Length == 0)
        {
            throw new ArgumentException("At least one policy must be supplied.", nameof(policies));
        }

        foreach(IAsyncPolicy policy in policies)
        {
            ArgumentNullException.ThrowIfNull(policy);
        }

        _policies = policies;
    }
    #endregion

    #region Private methods
    private Task<TResult> ExecuteFrom<TResult>(int index, Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
    {
        if(index == _policies.Length)
        {
            return operation(cancellationToken);
        }

        return _policies[index].ExecuteAsync(ct => ExecuteFrom(index + 1, operation, ct), cancellationToken);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Executes the specified operation through every composed policy, outermost first.
    ///</summary>
    ///<typeparam name="TResult">The return type of the operation.</typeparam>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token observed by the composed policies and passed to the operation.</param>
    ///<returns>The result of the operation, subject to every composed policy's behavior.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
    public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return ExecuteFrom(0, operation, cancellationToken);
    }

    ///<summary>
    ///Executes the specified operation through every composed policy, outermost first.
    ///</summary>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token observed by the composed policies and passed to the operation.</param>
    ///<exception cref="ArgumentNullException"><paramref name="operation"/> is <c>null</c>.</exception>
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
    ///Gets the number of policies composed by this wrap.
    ///</summary>
    public int PolicyCount => _policies.Length;
    #endregion
}
