using Catharsis.Resilience;

namespace Catharsis.Services;

///<summary>
///Wraps a delegate with token-bucket rate limiting, delegating the actual limiting to
///<see cref="Catharsis.Resilience.RateLimiter"/> so callers get a single call site instead of manually acquiring a
///token before every invocation.
///</summary>
///<typeparam name="TResult">The type of the result produced by the wrapped delegate.</typeparam>
///<param name="action">The delegate to rate-limit.</param>
///<param name="capacity">The maximum number of calls allowed in a burst.</param>
///<param name="callsPerSecond">The number of calls the bucket refills per second.</param>
///<exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> or <paramref name="callsPerSecond"/> is not greater than zero.</exception>
public sealed class ThrottledService<TResult>(Func<CancellationToken, Task<TResult>> action, double capacity, double callsPerSecond)
{
    #region Fields
    readonly Func<CancellationToken, Task<TResult>> _action = action ?? throw new ArgumentNullException(nameof(action), "Action must not be null.");
    readonly RateLimiter _limiter = new(capacity, callsPerSecond);
    #endregion

    #region Public methods
    ///<summary>
    ///Attempts to invoke the wrapped delegate immediately, without waiting for a token.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token.</param>
    ///<param name="result">The delegate's result, if it was invoked.</param>
    ///<returns><c>true</c> if a token was available and the delegate was invoked; otherwise <c>false</c>.</returns>
    public bool TryInvoke(CancellationToken cancellationToken, out Task<TResult>? result)
    {
        if(_limiter.TryAcquire())
        {
            result = _action(cancellationToken);
            return true;
        }

        result = null;
        return false;
    }

    ///<summary>
    ///Invokes the wrapped delegate, waiting for a rate-limit token to become available first.
    ///</summary>
    ///<param name="cancellationToken">A token that can abandon the wait.</param>
    ///<returns>The delegate's result.</returns>
    public async Task<TResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        await _limiter.AcquireAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _action(cancellationToken).ConfigureAwait(false);
    }
    #endregion
}
