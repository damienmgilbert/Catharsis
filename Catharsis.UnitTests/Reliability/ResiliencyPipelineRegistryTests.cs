using Catharsis.Events;
using Catharsis.Reliability;
using Catharsis.Resilience;

namespace Catharsis.UnitTests.Reliability;

///<summary>
///Unit tests for the <see cref="ResiliencyPipelineRegistry"/> class.
///</summary>
[TestClass]
public class ResiliencyPipelineRegistryTests
{
    #region Helpers

    private sealed class DirectPolicy : IAsyncPolicy
    {
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default) { return operation(cancellationToken); }
    }

    #endregion

    #region Constructor

    [TestMethod]
    public void Constructor_NullEventBus_Throws() { Assert.ThrowsExactly<ArgumentNullException>(() => new ResiliencyPipelineRegistry(null!)); }

    [TestMethod]
    public void Constructor_ZeroOrNegativeStaleValueLifetime_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ResiliencyPipelineRegistry(new EventBus(), TimeSpan.Zero)); }

    #endregion

    #region Register

    [TestMethod]
    public void Register_NullOrWhitespaceName_Throws()
    {
        ResiliencyPipelineRegistry registry = new(new EventBus());
        Assert.ThrowsExactly<ArgumentException>(() => registry.Register(" ", new DirectPolicy()));
    }

    [TestMethod]
    public void Register_NullPipeline_Throws()
    {
        ResiliencyPipelineRegistry registry = new(new EventBus());
        Assert.ThrowsExactly<ArgumentNullException>(() => registry.Register("payments", null!));
    }

    [TestMethod]
    public void Register_NewName_AddsToRegisteredPipelineNames()
    {
        ResiliencyPipelineRegistry registry = new(new EventBus());
        registry.Register("payments", new DirectPolicy());

        CollectionAssert.Contains(registry.RegisteredPipelineNames.ToList(), "payments");
    }

    #endregion

    #region Unregister

    [TestMethod]
    public void Unregister_ExistingName_ReturnsTrueAndRemoves()
    {
        ResiliencyPipelineRegistry registry = new(new EventBus());
        registry.Register("payments", new DirectPolicy());

        Assert.IsTrue(registry.Unregister("payments"));
        CollectionAssert.DoesNotContain(registry.RegisteredPipelineNames.ToList(), "payments");
    }

    [TestMethod]
    public void Unregister_UnknownName_ReturnsFalse()
    {
        ResiliencyPipelineRegistry registry = new(new EventBus());
        Assert.IsFalse(registry.Unregister("missing"));
    }

    #endregion

    #region ExecuteAsync

    [TestMethod]
    public async Task ExecuteAsync_UnregisteredName_Throws()
    {
        ResiliencyPipelineRegistry registry = new(new EventBus());
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => registry.ExecuteAsync("missing", static _ => Task.FromResult(1)));
    }

    [TestMethod]
    public async Task ExecuteAsync_SuccessfulOperation_ReturnsResult()
    {
        ResiliencyPipelineRegistry registry = new(new EventBus());
        registry.Register("payments", new DirectPolicy());

        int result = await registry.ExecuteAsync("payments", static _ => Task.FromResult(42));

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task ExecuteAsync_FailureWithNoCachedValue_Throws()
    {
        ResiliencyPipelineRegistry registry = new(new EventBus());
        registry.Register("payments", new DirectPolicy());

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => registry.ExecuteAsync<int>("payments", static _ => throw new InvalidOperationException("downstream failure")));
    }

    [TestMethod]
    public async Task ExecuteAsync_FailureAfterPriorSuccess_ReturnsLastKnownGoodValue()
    {
        ResiliencyPipelineRegistry registry = new(new EventBus());
        registry.Register("payments", new DirectPolicy());

        int firstResult = await registry.ExecuteAsync("payments", static _ => Task.FromResult(7));
        Assert.AreEqual(7, firstResult);

        int fallbackResult = await registry.ExecuteAsync<int>("payments", static _ => throw new InvalidOperationException("downstream failure"));

        Assert.AreEqual(7, fallbackResult);
    }

    [TestMethod]
    public async Task ExecuteAsync_CircuitBreakerTrips_PublishesCircuitStateChangedEvent()
    {
        EventBus eventBus = new();
        CircuitBreaker breaker = new(failureThreshold: 1, breakDuration: TimeSpan.FromMinutes(10));
        ResiliencyPipelineRegistry registry = new(eventBus);
        registry.Register("payments", breaker, breaker);

        List<CircuitStateChangedEvent> published = [];
        using IDisposable subscription = eventBus.Subscribe<CircuitStateChangedEvent>(evt =>
        {
            published.Add(evt);
            return Task.CompletedTask;
        });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => registry.ExecuteAsync<int>("payments", static _ => throw new InvalidOperationException("downstream failure")));

        Assert.HasCount(1, published);
        Assert.AreEqual("payments", published[0].PipelineName);
        Assert.AreEqual(CircuitState.Closed, published[0].Previous);
        Assert.AreEqual(CircuitState.Open, published[0].Current);
    }

    [TestMethod]
    public async Task ExecuteAsync_CircuitBreakerStaysClosed_PublishesNoEvent()
    {
        EventBus eventBus = new();
        CircuitBreaker breaker = new(failureThreshold: 5, breakDuration: TimeSpan.FromMinutes(10));
        ResiliencyPipelineRegistry registry = new(eventBus);
        registry.Register("payments", breaker, breaker);

        int publishCount = 0;
        using IDisposable subscription = eventBus.Subscribe<CircuitStateChangedEvent>(_ =>
        {
            publishCount++;
            return Task.CompletedTask;
        });

        await registry.ExecuteAsync("payments", static _ => Task.FromResult(1));

        Assert.AreEqual(0, publishCount);
    }

    #endregion
}
