using Catharsis.Events;
using Catharsis.Networking;
using Catharsis.Resilience;
using Catharsis.Scheduling;
using System.Collections.Concurrent;

namespace Catharsis.Monitoring;

///<summary>
///A background service that continuously watches a fixed set of endpoints: on a fixed interval, it sweeps every
///endpoint via <see cref="PingSweepHealthTracker"/>, drives a per-endpoint <see cref="CircuitBreaker"/> from each
///sweep's outcome, and publishes an <see cref="EndpointStatusChangedEvent"/> on an <see cref="EventBus"/> whenever
///an endpoint's aggregate <see cref="EndpointStatus"/> changes. The underlying <see cref="EndpointHealthTracker"/>'s
///success-ratio bookkeeping and <see cref="PingSweepHealthTracker"/>'s ICMP checks are passive recorders; this type
///is what turns them into something actively watching for change.
///</summary>
public sealed class NetworkMonitor : IAsyncDisposable
{
    #region Fields
    readonly IReadOnlyList<Uri> _endpoints;
    readonly PingSweepHealthTracker _pingSweep;
    readonly EventBus _eventBus;
    readonly ConcurrentDictionary<Uri, CircuitBreaker> _breakers;
    readonly ConcurrentDictionary<Uri, EndpointStatus> _lastStatus;
    readonly RecurringTimer _timer;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates and starts a network monitor that sweeps the specified endpoints on the given interval.
    ///</summary>
    ///<param name="endpoints">The endpoints to watch. Must contain at least one endpoint.</param>
    ///<param name="pollInterval">How often to sweep every endpoint. Must be greater than zero.</param>
    ///<param name="eventBus">The event bus that <see cref="EndpointStatusChangedEvent"/> notifications are published to.</param>
    ///<param name="healthTracker">
    ///An existing health tracker to record sweep results into, or <c>null</c> to create a private one.
    ///</param>
    ///<param name="failureThreshold">The number of consecutive failed sweeps that trips an endpoint's circuit. Defaults to 3.</param>
    ///<param name="breakDuration">How long a tripped endpoint's circuit stays open before a trial sweep. Defaults to 30 seconds.</param>
    ///<exception cref="ArgumentNullException"><paramref name="endpoints"/> or <paramref name="eventBus"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="endpoints"/> is empty.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="pollInterval"/> is not greater than zero.</exception>
    public NetworkMonitor(IEnumerable<Uri> endpoints, TimeSpan pollInterval, EventBus eventBus, EndpointHealthTracker? healthTracker = null, int failureThreshold = 3, TimeSpan? breakDuration = null)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(eventBus);

        _endpoints = [.. endpoints];

        if(_endpoints.Count == 0)
        {
            throw new ArgumentException("At least one endpoint must be supplied.", nameof(endpoints));
        }

        _eventBus = eventBus;
        _pingSweep = new PingSweepHealthTracker(healthTracker ?? new EndpointHealthTracker());
        _breakers = new ConcurrentDictionary<Uri, CircuitBreaker>(_endpoints.ToDictionary(static endpoint => endpoint, _ => new CircuitBreaker(failureThreshold, breakDuration ?? TimeSpan.FromSeconds(30))));
        _lastStatus = new ConcurrentDictionary<Uri, EndpointStatus>(_endpoints.ToDictionary(static endpoint => endpoint, static _ => EndpointStatus.Healthy));
        _timer = new RecurringTimer(pollInterval, PollOnceAsync);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Maps a <see cref="CircuitState"/> to the <see cref="EndpointStatus"/> it represents: closed is healthy,
    ///half-open is degraded, and open is down.
    ///</summary>
    ///<param name="circuitState">The circuit state to map.</param>
    ///<returns>The corresponding endpoint status.</returns>
    public static EndpointStatus FromCircuitState(CircuitState circuitState)
    {
        return circuitState switch
        {
            CircuitState.Closed => EndpointStatus.Healthy,
            CircuitState.HalfOpen => EndpointStatus.Degraded,
            CircuitState.Open => EndpointStatus.Down,
            _ => EndpointStatus.Down
        };
    }

    ///<summary>
    ///Stops the monitor's background sweep loop and waits for it to finish. Disposal never throws; a sweep failure
    ///remains observable via <see cref="Completion"/>.
    ///</summary>
    public async ValueTask DisposeAsync() { await _timer.DisposeAsync().ConfigureAwait(false); }

    ///<summary>
    ///Returns a snapshot of every watched endpoint's current status.
    ///</summary>
    ///<returns>A dictionary mapping each watched endpoint to its most recently observed status.</returns>
    public IReadOnlyDictionary<Uri, EndpointStatus> GetSnapshot() { return new Dictionary<Uri, EndpointStatus>(_lastStatus); }

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
                await breaker.ExecuteAsync(async ct =>
                {
                    if(!await _pingSweep.PingAsync(endpoint, ct).ConfigureAwait(false))
                    {
                        throw new PingSweepFailedSignal();
                    }
                }, cancellationToken).ConfigureAwait(false);
            } catch(Exception ex) when(ex is not OperationCanceledException)
            {
                // The failure has already been recorded by the circuit breaker; its resulting State drives the
                // status transition below.
            }

            EndpointStatus previous = _lastStatus[endpoint];
            EndpointStatus current = FromCircuitState(breaker.State);

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
    ///because a sweep threw. Awaiting this surfaces a sweep failure without needing to dispose first.
    ///</summary>
    public Task Completion => _timer.Completion;
    #endregion

    ///<summary>
    ///An internal-only signal used to route a failed ping through <see cref="CircuitBreaker.ExecuteAsync{TResult}"/>,
    ///which determines success or failure solely from whether the executed operation throws.
    ///</summary>
    sealed class PingSweepFailedSignal : Exception;
}
