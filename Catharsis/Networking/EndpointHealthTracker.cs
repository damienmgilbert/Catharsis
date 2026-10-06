using System.Collections.Concurrent;

namespace Catharsis.Networking;

///<summary>
///Tracks a rolling success/failure count per endpoint, for simple client-side load balancing decisions such as
///preferring the endpoint with the best recent success ratio.
///</summary>
public sealed class EndpointHealthTracker
{
    #region Fields
    private readonly ConcurrentDictionary<Uri, (long Successes, long Failures)> _stats = new();
    #endregion

    #region Public methods
    ///<summary>
    ///Gets the current success ratio for an endpoint: successes divided by total recorded attempts.
    ///</summary>
    ///<param name="endpoint">The endpoint to query.</param>
    ///<returns>
    ///The success ratio, from 0.0 to 1.0. An endpoint with no recorded attempts is treated as fully healthy (1.0).
    ///</returns>
    ///<exception cref="ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
    public double GetSuccessRatio(Uri endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        if(!_stats.TryGetValue(endpoint, out (long Successes, long Failures) stat))
        {
            return 1.0;
        }

        long total = stat.Successes + stat.Failures;
        return (total == 0) ? 1.0 : ((double)stat.Successes / total);
    }

    ///<summary>
    ///Records a failed attempt against an endpoint.
    ///</summary>
    ///<param name="endpoint">The endpoint that failed.</param>
    ///<exception cref="ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
    public void RecordFailure(Uri endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        _stats.AddOrUpdate(endpoint, static _ => (0, 1), static(_, existing) => (existing.Successes, existing.Failures + 1));
    }

    ///<summary>
    ///Records a successful attempt against an endpoint.
    ///</summary>
    ///<param name="endpoint">The endpoint that succeeded.</param>
    ///<exception cref="ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
    public void RecordSuccess(Uri endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        _stats.AddOrUpdate(endpoint, static _ => (1, 0), static(_, existing) => (existing.Successes + 1, existing.Failures));
    }

    ///<summary>
    ///Selects the endpoint with the highest success ratio among <paramref name="candidates"/>, breaking ties by the
    ///order they appear in.
    ///</summary>
    ///<param name="candidates">The endpoints to choose from.</param>
    ///<returns>The healthiest endpoint, or <c>null</c> if <paramref name="candidates"/> is empty.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="candidates"/> is <c>null</c>.</exception>
    public Uri? SelectHealthiest(IEnumerable<Uri> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        return candidates.OrderByDescending(GetSuccessRatio).FirstOrDefault();
    }
    #endregion
}
