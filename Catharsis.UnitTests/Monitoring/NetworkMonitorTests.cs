using Catharsis.Events;
using Catharsis.Monitoring;
using Catharsis.Resilience;

namespace Catharsis.UnitTests.Monitoring;

///<summary>
///Unit tests for the <see cref="NetworkMonitor"/> class.
///</summary>
[TestClass]
public class NetworkMonitorTests
{
    #region Helpers

    private static readonly Uri Loopback = new("https://127.0.0.1");
    private static readonly Uri Unresolvable = new("https://this-host-does-not-exist.invalid");

    private static NetworkMonitor CreateIdleMonitor(IEnumerable<Uri> endpoints, EventBus? eventBus = null, int failureThreshold = 3)
    {
        // A long poll interval keeps the background timer from ever firing during a test; all assertions drive the
        // monitor deterministically through the timer-independent PollOnceAsync entry point instead.
        return new NetworkMonitor(endpoints, TimeSpan.FromMinutes(10), eventBus ?? new EventBus(), failureThreshold: failureThreshold, breakDuration: TimeSpan.FromMinutes(10));
    }

    #endregion

    #region Constructor

    [TestMethod]
    public void Constructor_NullEndpoints_Throws() { Assert.ThrowsExactly<ArgumentNullException>(() => new NetworkMonitor(null!, TimeSpan.FromMinutes(1), new EventBus())); }

    [TestMethod]
    public void Constructor_NullEventBus_Throws() { Assert.ThrowsExactly<ArgumentNullException>(() => new NetworkMonitor([Loopback], TimeSpan.FromMinutes(1), null!)); }

    [TestMethod]
    public void Constructor_EmptyEndpoints_Throws() { Assert.ThrowsExactly<ArgumentException>(() => new NetworkMonitor([], TimeSpan.FromMinutes(1), new EventBus())); }

    [TestMethod]
    public void Constructor_ZeroOrNegativePollInterval_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new NetworkMonitor([Loopback], TimeSpan.Zero, new EventBus())); }

    #endregion

    #region FromCircuitState

    [TestMethod]
    public void FromCircuitState_Closed_ReturnsHealthy() { Assert.AreEqual(EndpointStatus.Healthy, NetworkMonitor.FromCircuitState(CircuitState.Closed)); }

    [TestMethod]
    public void FromCircuitState_HalfOpen_ReturnsDegraded() { Assert.AreEqual(EndpointStatus.Degraded, NetworkMonitor.FromCircuitState(CircuitState.HalfOpen)); }

    [TestMethod]
    public void FromCircuitState_Open_ReturnsDown() { Assert.AreEqual(EndpointStatus.Down, NetworkMonitor.FromCircuitState(CircuitState.Open)); }

    #endregion

    #region PollOnceAsync

    [TestMethod]
    public async Task PollOnceAsync_HealthyEndpointStaysHealthy_PublishesNoEvent()
    {
        EventBus eventBus = new();
        await using NetworkMonitor monitor = CreateIdleMonitor([Loopback], eventBus);

        int publishCount = 0;
        using IDisposable subscription = eventBus.Subscribe<EndpointStatusChangedEvent>(_ =>
        {
            publishCount++;
            return Task.CompletedTask;
        });

        await monitor.PollOnceAsync();

        Assert.AreEqual(0, publishCount);
        Assert.AreEqual(EndpointStatus.Healthy, monitor.GetSnapshot()[Loopback]);
    }

    [TestMethod]
    public async Task PollOnceAsync_UnreachableEndpointTripsBreaker_TransitionsToDownAndPublishesEvent()
    {
        EventBus eventBus = new();
        await using NetworkMonitor monitor = CreateIdleMonitor([Unresolvable], eventBus, failureThreshold: 1);

        List<EndpointStatusChangedEvent> published = [];
        using IDisposable subscription = eventBus.Subscribe<EndpointStatusChangedEvent>(evt =>
        {
            published.Add(evt);
            return Task.CompletedTask;
        });

        await monitor.PollOnceAsync();

        Assert.AreEqual(EndpointStatus.Down, monitor.GetSnapshot()[Unresolvable]);
        Assert.HasCount(1, published);
        Assert.AreEqual(Unresolvable, published[0].Endpoint);
        Assert.AreEqual(EndpointStatus.Healthy, published[0].Previous);
        Assert.AreEqual(EndpointStatus.Down, published[0].Current);
    }

    [TestMethod]
    public async Task PollOnceAsync_StatusUnchangedBetweenSweeps_PublishesEventOnlyOnce()
    {
        EventBus eventBus = new();
        await using NetworkMonitor monitor = CreateIdleMonitor([Unresolvable], eventBus, failureThreshold: 1);

        int publishCount = 0;
        using IDisposable subscription = eventBus.Subscribe<EndpointStatusChangedEvent>(_ =>
        {
            publishCount++;
            return Task.CompletedTask;
        });

        await monitor.PollOnceAsync();
        await monitor.PollOnceAsync();

        Assert.AreEqual(1, publishCount);
    }

    #endregion

    #region GetSnapshot

    [TestMethod]
    public async Task GetSnapshot_BeforeAnySweep_ReportsAllEndpointsHealthy()
    {
        await using NetworkMonitor monitor = CreateIdleMonitor([Loopback, Unresolvable]);

        IReadOnlyDictionary<Uri, EndpointStatus> snapshot = monitor.GetSnapshot();

        Assert.HasCount(2, snapshot);
        Assert.IsTrue(snapshot.Values.All(static status => status == EndpointStatus.Healthy));
    }

    #endregion

    #region Disposal

    [TestMethod]
    public async Task DisposeAsync_CompletesWithoutThrowing()
    {
        NetworkMonitor monitor = CreateIdleMonitor([Loopback]);
        await monitor.DisposeAsync();

        Assert.IsTrue(monitor.Completion.IsCompleted);
    }

    #endregion
}
