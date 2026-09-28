using Catharsis.Concurrency;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="AsyncLazy{T}"/> class.
///</summary>
[TestClass]
public class AsyncLazyTests
{
    #region Construction

    [TestMethod]
    public void Constructor_NullValueFactory_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new AsyncLazy<int>((Func<int>)null!));
    }

    [TestMethod]
    public void Constructor_NullTaskFactory_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new AsyncLazy<int>((Func<Task<int>>)null!));
    }

    [TestMethod]
    public void IsValueCreated_BeforeAccess_IsFalse()
    {
        AsyncLazy<int> lazy = new(static () => 42);
        Assert.IsFalse(lazy.IsValueCreated);
    }

    #endregion

    #region Value production

    [TestMethod]
    public async Task Value_SyncFactory_ProducesResult()
    {
        AsyncLazy<int> lazy = new(static () => 42);
        int result = await lazy.Value;
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task Value_AsyncFactory_ProducesResult()
    {
        AsyncLazy<int> lazy = new(static () => Task.FromResult(99));
        int result = await lazy.Value;
        Assert.AreEqual(99, result);
    }

    [TestMethod]
    public async Task Await_Directly_ProducesResult()
    {
        AsyncLazy<int> lazy = new(static () => 7);
        int result = await lazy;
        Assert.AreEqual(7, result);
    }

    [TestMethod]
    public async Task Value_FactoryRunsAtMostOnce()
    {
        int callCount = 0;
        AsyncLazy<int> lazy = new(() =>
        {
            Interlocked.Increment(ref callCount);
            return 1;
        });

        int[] results = await Task.WhenAll(lazy.Value, lazy.Value, lazy.Value);

        Assert.AreEqual(1, callCount);
        Assert.IsTrue(Array.TrueForAll(results, static r => r == 1));
    }

    [TestMethod]
    public void IsValueCreated_AfterAccess_IsTrue()
    {
        AsyncLazy<int> lazy = new(static () => 42);
        _ = lazy.Value;
        Assert.IsTrue(lazy.IsValueCreated);
    }

    #endregion
}
