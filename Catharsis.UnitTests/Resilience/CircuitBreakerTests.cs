using Catharsis.Resilience;

namespace Catharsis.UnitTests.Resilience;

///<summary>
///Unit tests for the <see cref="CircuitBreaker"/> class.
///</summary>
[TestClass]
public class CircuitBreakerTests
{
    #region Construction

    [TestMethod]
    public void Constructor_ZeroOrNegativeFailureThreshold_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CircuitBreaker(0, TimeSpan.Zero));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CircuitBreaker(-1, TimeSpan.Zero));
    }

    [TestMethod]
    public void Constructor_NegativeBreakDuration_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CircuitBreaker(1, TimeSpan.FromMilliseconds(-1)));
    }

    [TestMethod]
    public void Constructor_Default_StateIsClosed()
    {
        CircuitBreaker breaker = new(3, TimeSpan.FromSeconds(1));
        Assert.AreEqual(CircuitState.Closed, breaker.State);
    }

    #endregion

    #region Closed state

    [TestMethod]
    public void Execute_Success_StaysClosed()
    {
        CircuitBreaker breaker = new(3, TimeSpan.FromSeconds(1));
        int result = breaker.Execute(static () => 42);
        Assert.AreEqual(42, result);
        Assert.AreEqual(CircuitState.Closed, breaker.State);
    }

    [TestMethod]
    public void Execute_FailuresBelowThreshold_StaysClosed()
    {
        CircuitBreaker breaker = new(3, TimeSpan.FromSeconds(1));

        for (int i = 0; i < 2; i++)
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));
        }

        Assert.AreEqual(CircuitState.Closed, breaker.State);
    }

    [TestMethod]
    public void Execute_NullOperation_Throws()
    {
        CircuitBreaker breaker = new(3, TimeSpan.FromSeconds(1));
        Assert.ThrowsExactly<ArgumentNullException>(() => breaker.Execute<int>(null!));
    }

    [TestMethod]
    public void Execute_SuccessAfterFailures_ResetsFailureCount()
    {
        CircuitBreaker breaker = new(2, TimeSpan.FromSeconds(1));

        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));
        breaker.Execute(static () => 1);

        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));
        Assert.AreEqual(CircuitState.Closed, breaker.State);
    }

    #endregion

    #region Open state

    [TestMethod]
    public void Execute_FailuresReachThreshold_OpensCircuit()
    {
        CircuitBreaker breaker = new(2, TimeSpan.FromMinutes(1));

        for (int i = 0; i < 2; i++)
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));
        }

        Assert.AreEqual(CircuitState.Open, breaker.State);
    }

    [TestMethod]
    public void Execute_CircuitOpen_RejectsWithoutCallingOperation()
    {
        CircuitBreaker breaker = new(1, TimeSpan.FromMinutes(1));
        int callCount = 0;

        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(() =>
        {
            callCount++;
            throw new InvalidOperationException();
        }));

        Assert.ThrowsExactly<CircuitBreakerOpenException>(() => breaker.Execute(() =>
        {
            callCount++;
            return 1;
        }));

        Assert.AreEqual(1, callCount);
    }

    #endregion

    #region Half-open state

    [TestMethod]
    public async Task Execute_BreakDurationElapsed_AllowsTrialCall()
    {
        CircuitBreaker breaker = new(1, TimeSpan.FromMilliseconds(20));

        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));
        Assert.AreEqual(CircuitState.Open, breaker.State);

        await Task.Delay(60);

        int result = breaker.Execute(static () => 42);
        Assert.AreEqual(42, result);
        Assert.AreEqual(CircuitState.Closed, breaker.State);
    }

    [TestMethod]
    public async Task Execute_TrialCallFails_ReopensCircuit()
    {
        CircuitBreaker breaker = new(1, TimeSpan.FromMilliseconds(20));

        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));

        await Task.Delay(60);

        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));
        Assert.AreEqual(CircuitState.Open, breaker.State);
    }

    #endregion

    #region Async execution

    [TestMethod]
    public async Task ExecuteAsync_Success_ReturnsResult()
    {
        CircuitBreaker breaker = new(3, TimeSpan.FromSeconds(1));
        int result = await breaker.ExecuteAsync(static ct => Task.FromResult(42));
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task ExecuteAsync_FailuresReachThreshold_OpensCircuit()
    {
        CircuitBreaker breaker = new(1, TimeSpan.FromMinutes(1));

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await breaker.ExecuteAsync<int>(static ct => throw new InvalidOperationException()));

        await Assert.ThrowsExactlyAsync<CircuitBreakerOpenException>(async () =>
            await breaker.ExecuteAsync(static ct => Task.FromResult(1)));
    }

    [TestMethod]
    public async Task ExecuteAsync_OperationCanceled_DoesNotCountAsFailure()
    {
        CircuitBreaker breaker = new(1, TimeSpan.FromMinutes(1));

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
            await breaker.ExecuteAsync<int>(static ct => throw new OperationCanceledException()));

        Assert.AreEqual(CircuitState.Closed, breaker.State);
    }

    #endregion

    #region Reset

    [TestMethod]
    public void Reset_ClosesCircuitAndClearsFailures()
    {
        CircuitBreaker breaker = new(1, TimeSpan.FromMinutes(1));
        Assert.ThrowsExactly<InvalidOperationException>(() => breaker.Execute<int>(static () => throw new InvalidOperationException()));
        Assert.AreEqual(CircuitState.Open, breaker.State);

        breaker.Reset();

        Assert.AreEqual(CircuitState.Closed, breaker.State);
        int result = breaker.Execute(static () => 1);
        Assert.AreEqual(1, result);
    }

    #endregion
}
