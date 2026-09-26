using Catharsis.Resilience;

namespace Catharsis.UnitTests.Resilience;

///<summary>
///Unit tests for the <see cref="BulkheadPolicy"/> class.
///</summary>
[TestClass]
public class BulkheadPolicyTests
{
    #region Construction

    [TestMethod]
    public void Constructor_ZeroOrNegativeMaxConcurrency_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BulkheadPolicy(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BulkheadPolicy(-1));
    }

    [TestMethod]
    public void Constructor_NegativeMaxQueueLength_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BulkheadPolicy(1, -1));
    }

    [TestMethod]
    public void Constructor_SetsProperties()
    {
        using BulkheadPolicy bulkhead = new(2, 3);
        Assert.AreEqual(2, bulkhead.MaxConcurrency);
        Assert.AreEqual(3, bulkhead.MaxQueueLength);
        Assert.AreEqual(2, bulkhead.AvailableConcurrency);
    }

    #endregion

    #region ExecuteAsync

    [TestMethod]
    public async Task ExecuteAsync_WithinConcurrencyLimit_ReturnsResult()
    {
        using BulkheadPolicy bulkhead = new(2);
        int result = await bulkhead.ExecuteAsync(static ct => Task.FromResult(42));
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task ExecuteAsync_NullOperation_Throws()
    {
        using BulkheadPolicy bulkhead = new(1);
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await bulkhead.ExecuteAsync<int>(null!));
    }

    [TestMethod]
    public async Task ExecuteAsync_NoQueueCapacity_RejectsBeyondConcurrencyLimit()
    {
        using BulkheadPolicy bulkhead = new(1);
        using SemaphoreSlim gate = new(0);

        Task<int> first = bulkhead.ExecuteAsync(async ct =>
        {
            await gate.WaitAsync(ct);
            return 1;
        });

        await Task.Delay(20);

        await Assert.ThrowsExactlyAsync<BulkheadRejectedException>(async () =>
            await bulkhead.ExecuteAsync(static ct => Task.FromResult(2)));

        gate.Release();
        await first;
    }

    [TestMethod]
    public async Task ExecuteAsync_WithQueueCapacity_WaitsInsteadOfRejecting()
    {
        using BulkheadPolicy bulkhead = new(1, maxQueueLength: 1);
        using SemaphoreSlim gate = new(0);

        Task<int> first = bulkhead.ExecuteAsync(async ct =>
        {
            await gate.WaitAsync(ct);
            return 1;
        });

        await Task.Delay(20);

        Task<int> second = bulkhead.ExecuteAsync(static ct => Task.FromResult(2));
        await Task.Delay(20);
        Assert.IsFalse(second.IsCompleted);

        gate.Release();

        int[] results = await Task.WhenAll(first, second);
        Assert.AreEqual(1, results[0]);
        Assert.AreEqual(2, results[1]);
    }

    [TestMethod]
    public async Task ExecuteAsync_QueueAlsoFull_Rejects()
    {
        using BulkheadPolicy bulkhead = new(1, maxQueueLength: 1);
        using SemaphoreSlim gate = new(0);

        Task<int> first = bulkhead.ExecuteAsync(async ct =>
        {
            await gate.WaitAsync(ct);
            return 1;
        });
        await Task.Delay(20);

        Task<int> queued = bulkhead.ExecuteAsync(async ct =>
        {
            await gate.WaitAsync(ct);
            return 2;
        });
        await Task.Delay(20);

        await Assert.ThrowsExactlyAsync<BulkheadRejectedException>(async () =>
            await bulkhead.ExecuteAsync(static ct => Task.FromResult(3)));

        gate.Release();
        gate.Release();
        await Task.WhenAll(first, queued);
    }

    [TestMethod]
    public async Task ExecuteAsync_AfterDisposal_Throws()
    {
        BulkheadPolicy bulkhead = new(1);
        bulkhead.Dispose();

        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(async () =>
            await bulkhead.ExecuteAsync(static ct => Task.FromResult(1)));
    }

    #endregion

    #region Disposal

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        BulkheadPolicy bulkhead = new(1);
        bulkhead.Dispose();
        bulkhead.Dispose();
    }

    #endregion
}
