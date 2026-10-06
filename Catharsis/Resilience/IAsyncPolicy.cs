namespace Catharsis.Resilience;

///<summary>
///Represents a resilience policy that can execute an operation, applying its own cross-cutting behavior (retry, circuit
///breaking, timeout, bulkhead isolation, etc.) around the call. Implementations of this interface can be composed
///together with <see cref="PolicyWrap"/>.
///</summary>
public interface IAsyncPolicy
{
    #region Public methods

    ///<summary>
    ///Executes the specified operation under this policy.
    ///</summary>
    ///<typeparam name="TResult">The return type of the operation.</typeparam>
    ///<param name="operation">The async operation to execute. Receives a <see cref="CancellationToken"/>.</param>
    ///<param name="cancellationToken">A cancellation token observed by the policy and passed to the operation.</param>
    ///<returns>The result of the operation, subject to this policy's behavior.</returns>
    Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default);
    #endregion
}
