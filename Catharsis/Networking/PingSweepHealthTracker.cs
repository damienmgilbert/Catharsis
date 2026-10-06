using System.Net.NetworkInformation;

namespace Catharsis.Networking;

///<summary>
///Extends an <see cref="EndpointHealthTracker"/> with ICMP-based liveness checks via <see cref="Ping"/>, as an
///alternative signal to that tracker's existing HTTP-success-based recording.
///</summary>
///<param name="healthTracker">The tracker to record ICMP results into.</param>
///<param name="timeout">The timeout for each ping attempt. Defaults to 4 seconds if not specified.</param>
///<exception cref="ArgumentNullException"><paramref name="healthTracker"/> is <c>null</c>.</exception>
///<exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is not positive.</exception>
public sealed class PingSweepHealthTracker(EndpointHealthTracker healthTracker, TimeSpan? timeout = null)
{
    #region Fields
    private readonly EndpointHealthTracker _healthTracker = healthTracker ?? throw new ArgumentNullException(nameof(healthTracker));
    private readonly TimeSpan _timeout = ValidateTimeout(timeout ?? TimeSpan.FromSeconds(4));
    #endregion

    #region Private methods
    private static TimeSpan ValidateTimeout(TimeSpan timeout)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);
        return timeout;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Pings a single endpoint's host and records the result into the underlying <see cref="EndpointHealthTracker"/>.
    ///</summary>
    ///<param name="endpoint">The endpoint whose host to ping.</param>
    ///<param name="cancellationToken">A token to cancel the operation.</param>
    ///<returns><c>true</c> if the ping succeeded; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
    public async Task<bool> PingAsync(Uri endpoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        bool success;

        try
        {
            using Ping ping = new();
            PingReply reply = await ping.SendPingAsync(endpoint.Host, _timeout, cancellationToken: cancellationToken).ConfigureAwait(false);
            success = reply.Status == IPStatus.Success;
        } catch(PingException)
        {
            success = false;
        }

        if(success)
        {
            _healthTracker.RecordSuccess(endpoint);
        } else
        {
            _healthTracker.RecordFailure(endpoint);
        }

        return success;
    }

    ///<summary>
    ///Pings every endpoint in <paramref name="endpoints"/> concurrently and records each result.
    ///</summary>
    ///<param name="endpoints">The endpoints to sweep.</param>
    ///<param name="cancellationToken">A token to cancel the operation.</param>
    ///<returns>A dictionary mapping each endpoint to whether it responded successfully.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="endpoints"/> is <c>null</c>.</exception>
    public async Task<IReadOnlyDictionary<Uri, bool>> SweepAsync(IEnumerable<Uri> endpoints, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        Uri[] endpointArray = [ .. endpoints ];
        bool[] results = await Task.WhenAll(endpointArray.Select(endpoint => PingAsync(endpoint, cancellationToken))).ConfigureAwait(false);

        Dictionary<Uri, bool> map = new(endpointArray.Length);
        for(int index = 0; index < endpointArray.Length; index++)
        {
            map[endpointArray[index]] = results[index];
        }

        return map;
    }
    #endregion
}
