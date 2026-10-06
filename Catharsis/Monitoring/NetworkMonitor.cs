using System.Collections.Concurrent;
using Catharsis.Events;
using Catharsis.Networking;
using Catharsis.Resilience;
using Catharsis.Scheduling;

namespace Catharsis.Monitoring;

///<summary>
///A background service that continuously watches a fixed set of endpoints: on a fixed interval, it sweeps every
///endpoint via <see cref="PingSweepHealthTracker"/>, drives a per-endpoint <see cref="CircuitBreaker"/> from each
///sweep's outcome, and publishes an <see cref="EndpointStatusChangedEvent"/> on an <see cref="EventBus"/> whenever an
///endpoint's aggregate <see cref="EndpointStatus"/> changes. The underlying <see cref="EndpointHealthTracker"/>'s
///success-ratio bookkeeping and <see cref="PingSweepHealthTracker"/>'s ICMP checks are passive recorders; this type is
///what turns them into something actively watching for change. Endpoints are compared by <see cref="Uri"/> value
///equality; duplicate endpoints in the constructor's list are treated as one.
///</summary>
public sealed class NetworkMonitor : IAsyncDisposable
{
    #region Fields
    private readonly ConcurrentDictionary<Uri, CircuitBreaker> _breakers;
    private readonly IReadOnlyList<Uri> _endpoints;
    private readonly EventBus _eventBus;
    private readonly EndpointHealthTracker _healthTracker;
    private readonly ConcurrentDictionary<Uri, EndpointStatus> _lastStatus;
    private readonly PingSweepHealthTracker _pingSweep;
    private readonly RecurringTimer _timer;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates and starts a network monitor that sweeps the specified endpoints on the given interval.
    ///</summary>
    ///<param name="endpoints">The endpoints to watch. Must contain at least one distinct endpoint.</param>
    ///<param name="pollInterval">How often to sweep every endpoint. Must be greater than zero.</param>
    ///<param name="eventBus">The event bus that <see cref="EndpointStatusChangedEvent"/> notifications are published to.</param>
    ///<param name="healthTracker">
    ///An existing health tracker to record sweep results into, or <c>null</c> to create a private one.
    ///</param>
    ///<param name="failureThreshold">The number of consecutive failed sweeps that trips an endpoint's circuit. Defaults to 3.</param>
    ///<param name="breakDuration">How long a tripped endpoint's circuit stays open before a trial sweep. Defaults to 30 seconds.</param>
    ///<exception cref="ArgumentNullException"><paramref name="endpoints"/> or <paramref name="eventBus"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="endpoints"/> contains no distinct endpoints.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="pollInterval"/> is not greater than zero.</exception>
    public NetworkMonitor(IEnumerable<Uri> endpoints, TimeSpan pollInterval, EventBus eventBus, EndpointHealthTracker? healthTracker = null, int failureThreshold = 3, TimeSpan? breakDuration = null)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(eventBus);

        _endpoints = [ .. endpoints.Distinct() ];

        if(_endpoints.Count == 0)
        {
            throw new ArgumentException("At least one distinct endpoint must be supplied.", nameof(endpoints));
        }

        _eventBus = eventBus;
        _healthTracker = healthTracker ?? new EndpointHealthTracker();
        _pingSweep = new PingSweepHealthTracker(_healthTracker);
        _breakers = new ConcurrentDictionary<Uri, CircuitBreaker>(_endpoints.ToDictionary(static endpoint => endpoint, _ => new CircuitBreaker(failureThreshold, breakDuration ?? TimeSpan.FromSeconds(30))));
        _lastStatus = new ConcurrentDictionary<Uri, EndpointStatus>(_endpoints.ToDictionary(static endpoint => endpoint, static _ => EndpointStatus.Healthy));
        _timer = new RecurringTimer(pollInterval, PollOnceAsync);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Determines an endpoint's aggregate status from its circuit breaker state and recent success ratio: an open
    ///circuit is always <see cref="EndpointStatus.Down"/>; otherwise, a success ratio below 1.0 (recent sweeps have not
    ///all succeeded, even though the circuit hasn't tripped) is <see cref="EndpointStatus.Degraded"/>, and a perfect
    ///ratio is <see cref="EndpointStatus.Healthy"/>.
    ///</summary>
    ///<param name="circuitState">The endpoint's current circuit breaker state.</param>
    ///<param name="successRatio">The endpoint's recent success ratio, e.g. from <see cref="EndpointHealthTracker.GetSuccessRatio"/>.</param>
    ///<returns>The corresponding endpoint status.</returns>
    public static EndpointStatus DetermineStatus(CircuitState circuitState, double successRatio)
    {
        if(circuitState == CircuitState.Open)
        {
            return EndpointStatus.Down;
        }

        return (successRatio < 1.0) ? EndpointStatus.Degraded : EndpointStatus.Healthy;
    }

    ///<summary>
    ///Stops the monitor's background sweep loop and waits for it to finish. Disposal never throws; a sweep failure
    ///remains observable via <see cref="Completion"/>.
    ///</summary>
    public async ValueTask DisposeAsync() => await _timer.DisposeAsync().ConfigureAwait(false);

    ///<summary>
    ///Returns a snapshot of every watched endpoint's current status.
    ///</summary>
    ///<returns>A dictionary mapping each watched endpoint to its most recently observed status.</returns>
    public IReadOnlyDictionary<Uri, EndpointStatus> GetSnapshot() => new Dictionary<Uri, EndpointStatus>(_lastStatus);

    ///<summary>
    ///Sweeps every watched endpoint once, outside of the background timer loop. Each endpoint's ping result is fed
    ///through its <see cref="CircuitBreaker"/>; if the resulting <see cref="EndpointStatus"/> differs from the last
    ///observed status, an <see cref="EndpointStatusChangedEvent"/> is published.
    ///</summary>
    ///<param name="cancellationToken">A token that can abandon the sweep between endpoints.</param>
    public async Task PollOnceAsync(CancellationToken cancellationToken = default)
    {
        foreach(Uri endpoint in _endpoints)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CircuitBreaker breaker = _breakers[endpoint];

            try
            {
                await breaker.ExecuteAsync(
                      async ct =>
                      {
                          if(!await _pingSweep.PingAsync(endpoint, ct).ConfigureAwait(false))
                          {
                              throw new PingSweepFailedSignal();
                          }
                      },
                      cancellationToken)
                    .ConfigureAwait(false);
            } catch(PingSweepFailedSignal)
            {
                // Expected: the ping failed; the circuit breaker has already recorded the failure.
            } catch(CircuitBreakerOpenException)
            {
                // Expected: the circuit is already open for this endpoint, so no ping was attempted.
            }

            EndpointStatus previous = _lastStatus[endpoint];
            EndpointStatus current = DetermineStatus(breaker.State, _healthTracker.GetSuccessRatio(endpoint));

            if(current != previous)
            {
                _lastStatus[endpoint] = current;
                await _eventBus.PublishAsync(new EndpointStatusChangedEvent(endpoint, previous, current, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
            }
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a task that completes when the background sweep loop stops, either because the monitor was disposed or
    ///because a sweep threw an exception other than a ping failure or an open circuit (both handled internally).
    ///Awaiting this surfaces such a failure without needing to dispose first.
    ///</summary>
    public Task Completion => _timer.Completion;
    #endregion

    ///<summary>
    ///An internal-only signal used to route a failed ping through <see cref="CircuitBreaker.ExecuteAsync{TResult}"/>,
    ///which determines success or failure solely from whether the executed operation throws.
    ///</summary>
    private sealed class PingSweepFailedSignal : Exception;
}
