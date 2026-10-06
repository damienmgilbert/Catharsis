using Catharsis.Resilience;

namespace Catharsis.UnitTests.Resilience;

///<summary>
///Unit tests for the <see cref="TimeoutPolicy"/> class.
///</summary>
[TestClass]
public class TimeoutPolicyTests
{
    #region Construction

    [TestMethod]
    public void Constructor_ZeroOrNegativeTimeout_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new TimeoutPolicy(TimeSpan.Zero));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new TimeoutPolicy(TimeSpan.FromMilliseconds(-1)));
    }

    [TestMethod]
    public void Constructor_SetsTimeout()
    {
        TimeoutPolicy policy = new(TimeSpan.FromSeconds(5));
        Assert.AreEqual(TimeSpan.FromSeconds(5), policy.Timeout);
    }

    #endregion

    #region ExecuteAsync<TResult>

    [TestMethod]
    public async Task ExecuteAsync_CompletesWithinTimeout_ReturnsResult()
    {
        TimeoutPolicy policy = new(TimeSpan.FromSeconds(5));
        int result = await policy.ExecuteAsync(static ct => Task.FromResult(42));
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task ExecuteAsync_ExceedsTimeout_ThrowsTimeoutException()
    {
        TimeoutPolicy policy = new(TimeSpan.FromMilliseconds(20));

        await Assert.ThrowsExactlyAsync<TimeoutException>(async () =>
            await policy.ExecuteAsync(async ct => await Task.Delay(TimeSpan.FromSeconds(5), ct)));
    }

    [TestMethod]
    public async Task ExecuteAsync_OperationObservesLinkedToken()
    {
        TimeoutPolicy policy = new(TimeSpan.FromMilliseconds(20));
        bool observedCancellation = false;

        await Assert.ThrowsExactlyAsync<TimeoutException>(async () =>
            await policy.ExecuteAsync<int>(async ct =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), ct);
                }
                catch (OperationCanceledException)
                {
                    observedCancellation = true;
                    throw;
                }

                return 1;
            }));

        Assert.IsTrue(observedCancellation);
    }

    [TestMethod]
    public async Task ExecuteAsync_NullOperation_Throws()
    {
        TimeoutPolicy policy = new(TimeSpan.FromSeconds(1));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await policy.ExecuteAsync<int>(null!));
    }

    [TestMethod]
    public async Task ExecuteAsync_ExternalCancellation_ThrowsOperationCanceledExceptionNotTimeout()
    {
        TimeoutPolicy policy = new(TimeSpan.FromSeconds(5));
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () =>
            await policy.ExecuteAsync(async ct => await Task.Delay(TimeSpan.FromSeconds(5), ct), cts.Token));
    }

    #endregion

    #region ExecuteAsync (void)

    [TestMethod]
    public async Task ExecuteAsyncVoid_CompletesWithinTimeout_Completes()
    {
        TimeoutPolicy policy = new(TimeSpan.FromSeconds(5));
        bool executed = false;
        await policy.ExecuteAsync(ct =>
        {
            executed = true;
            return Task.CompletedTask;
        });
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task ExecuteAsyncVoid_ExceedsTimeout_ThrowsTimeoutException()
    {
        TimeoutPolicy policy = new(TimeSpan.FromMilliseconds(20));

        await Assert.ThrowsExactlyAsync<TimeoutException>(async () =>
            await policy.ExecuteAsync(async ct => await Task.Delay(TimeSpan.FromSeconds(5), ct)));
    }

    #endregion
}
