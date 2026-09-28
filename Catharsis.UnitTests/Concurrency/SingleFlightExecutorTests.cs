using Catharsis.Concurrency;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="SingleFlightExecutor{TKey, TResult}"/> class.
///</summary>
[TestClass]
public class SingleFlightExecutorTests
{
    #region ExecuteAsync

    [TestMethod]
    public void ExecuteAsync_NullKey_Throws()
    {
        SingleFlightExecutor<string, int> executor = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => executor.ExecuteAsync(null!, static () => Task.FromResult(1)));
    }

    [TestMethod]
    public void ExecuteAsync_NullOperation_Throws()
    {
        SingleFlightExecutor<string, int> executor = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => executor.ExecuteAsync("a", null!));
    }

    [TestMethod]
    public async Task ExecuteAsync_SingleCall_ReturnsResult()
    {
        SingleFlightExecutor<string, int> executor = new();
        int result = await executor.ExecuteAsync("a", static () => Task.FromResult(42));
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task ExecuteAsync_ConcurrentCallsSameKey_ShareSingleExecution()
    {
        SingleFlightExecutor<string, int> executor = new();
        int callCount = 0;
        using SemaphoreSlim gate = new(0);

        async Task<int> Operation()
        {
            Interlocked.Increment(ref callCount);
            await gate.WaitAsync();
            return 1;
        }

        Task<int> call1 = executor.ExecuteAsync("a", Operation);
        Task<int> call2 = executor.ExecuteAsync("a", Operation);

        Assert.AreEqual(1, executor.InFlightCount);
        gate.Release();

        int[] results = await Task.WhenAll(call1, call2);

        Assert.AreEqual(1, callCount);
        Assert.IsTrue(Array.TrueForAll(results, static r => r == 1));
    }

    [TestMethod]
    public async Task ExecuteAsync_ConcurrentCallsDifferentKeys_ExecuteIndependently()
    {
        SingleFlightExecutor<string, int> executor = new();
        int callCount = 0;

        Task<int> Operation() => Task.FromResult(Interlocked.Increment(ref callCount));

        int[] results = await Task.WhenAll(
            executor.ExecuteAsync("a", Operation),
            executor.ExecuteAsync("b", Operation));

        Assert.AreEqual(2, callCount);
        Assert.HasCount(2, results);
    }

    [TestMethod]
    public async Task ExecuteAsync_AfterCompletion_StartsFreshExecution()
    {
        SingleFlightExecutor<string, int> executor = new();
        int callCount = 0;

        Task<int> Operation() => Task.FromResult(Interlocked.Increment(ref callCount));

        await executor.ExecuteAsync("a", Operation);
        await executor.ExecuteAsync("a", Operation);

        Assert.AreEqual(2, callCount);
    }

    [TestMethod]
    public async Task ExecuteAsync_OperationThrows_PropagatesToAllCallers()
    {
        SingleFlightExecutor<string, int> executor = new();

        static Task<int> Operation() => throw new InvalidOperationException("boom");

        Task<int> call1 = executor.ExecuteAsync("a", Operation);
        Task<int> call2 = executor.ExecuteAsync("a", Operation);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await call1);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await call2);

        Assert.AreEqual(0, executor.InFlightCount);
    }

    [TestMethod]
    public void InFlightCount_NoExecutions_IsZero()
    {
        SingleFlightExecutor<string, int> executor = new();
        Assert.AreEqual(0, executor.InFlightCount);
    }

    #endregion
}
