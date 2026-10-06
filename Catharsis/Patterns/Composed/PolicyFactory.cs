using Catharsis.Resilience;

namespace Catharsis.Patterns.Composed;

///<summary>
///Builds an <see cref="IAsyncPolicy"/> pipeline from a <see cref="PolicyOptions"/> description, so callers state what
///they want rather than how to wire it. Retry is the outer layer and the timeout the inner one, meaning the timeout
///applies to each attempt separately.
///</summary>
public static class PolicyFactory
{
    #region Public methods

    ///<summary>
    ///Creates a policy for <paramref name="options"/>.
    ///</summary>
    ///<param name="options">The behaviour to assemble.</param>
    ///<returns>
    ///A pipeline of the requested policies, or a pass-through policy when the options ask for neither retry nor
    ///timeout.
    ///</returns>
    ///<exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><see cref="PolicyOptions.MaxAttempts"/> is below 1.</exception>
    public static IAsyncPolicy Create(PolicyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.MaxAttempts, 1);

        List<IAsyncPolicy> layers = [];

        if(options.MaxAttempts > 1)
        {
            RetryPolicy retry = new RetryPolicy().MaxAttempts(options.MaxAttempts).InitialDelay(options.InitialRetryDelay);

            layers.Add(options.UseJitter ? retry.WithJitter() : retry);
        }

        if(options.Timeout is TimeSpan timeout)
        {
            layers.Add(new TimeoutPolicy(timeout));
        }

        return layers.Count switch
        {
            0 => PassThroughPolicy.Instance,
            1 => layers[0],
            _ => new PolicyWrap([ .. layers ])
        };
    }
    #endregion

    private sealed class PassThroughPolicy : IAsyncPolicy
    {
        #region Fields
        public static readonly PassThroughPolicy Instance = new();
        #endregion

        #region Public methods
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            return operation(cancellationToken);
        }
        #endregion
    }
}
