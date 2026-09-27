using Catharsis.Caching;

namespace Catharsis.UnitTests.Caching;

///<summary>
///Unit tests for the <see cref="CacheStampedeGuard{TKey, TValue}"/> class.
///</summary>
[TestClass]
public class CacheStampedeGuardTests
{
    #region GetOrAddAsync

    [TestMethod]
    public async Task GetOrAddAsync_NullKey_Throws()
    {
        CacheStampedeGuard<string, int> guard = new();
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await guard.GetOrAddAsync(null!, static () => Task.FromResult(1)));
    }

    [TestMethod]
    public async Task GetOrAddAsync_NullFactory_Throws()
    {
        CacheStampedeGuard<string, int> guard = new();
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await guard.GetOrAddAsync("a", null!));
    }

    [TestMethod]
    public async Task GetOrAddAsync_MissingKey_FetchesAndCaches()
    {
        CacheStampedeGuard<string, int> guard = new();
        int result = await guard.GetOrAddAsync("a", static () => Task.FromResult(42));
        Assert.AreEqual(42, result);
        Assert.IsTrue(guard.TryGetValue("a", out int cached));
        Assert.AreEqual(42, cached);
    }

    [TestMethod]
    public async Task GetOrAddAsync_ExistingKey_ReturnsCachedValueWithoutFetching()
    {
        CacheStampedeGuard<string, int> guard = new();
        int callCount = 0;

        Task<int> Fetch()
        {
            callCount++;
            return Task.FromResult(1);
        }

        await guard.GetOrAddAsync("a", Fetch);
        await guard.GetOrAddAsync("a", Fetch);

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public async Task GetOrAddAsync_ConcurrentMissesSameKey_ShareSingleFetch()
    {
        CacheStampedeGuard<string, int> guard = new();
        int callCount = 0;
        using SemaphoreSlim gate = new(0);

        async Task<int> Fetch()
        {
            Interlocked.Increment(ref callCount);
            await gate.WaitAsync();
            return 1;
        }

        Task<int> call1 = guard.GetOrAddAsync("a", Fetch);
        Task<int> call2 = guard.GetOrAddAsync("a", Fetch);

        gate.Release();

        int[] results = await Task.WhenAll(call1, call2);

        Assert.AreEqual(1, callCount);
        Assert.IsTrue(Array.TrueForAll(results, static r => r == 1));
    }

    [TestMethod]
    public async Task GetOrAddAsync_ConcurrentMissesDifferentKeys_FetchIndependently()
    {
        CacheStampedeGuard<string, int> guard = new();
        int callCount = 0;

        Task<int> Fetch() => Task.FromResult(Interlocked.Increment(ref callCount));

        await Task.WhenAll(guard.GetOrAddAsync("a", Fetch), guard.GetOrAddAsync("b", Fetch));

        Assert.AreEqual(2, callCount);
    }

    #endregion

    #region Invalidate / TryGetValue / Clear

    [TestMethod]
    public void TryGetValue_MissingKey_ReturnsFalse()
    {
        CacheStampedeGuard<string, int> guard = new();
        Assert.IsFalse(guard.TryGetValue("a", out int value));
        Assert.AreEqual(0, value);
    }

    [TestMethod]
    public async Task Invalidate_RemovesEntrySoNextCallRefetches()
    {
        CacheStampedeGuard<string, int> guard = new();
        int callCount = 0;

        Task<int> Fetch() => Task.FromResult(Interlocked.Increment(ref callCount));

        await guard.GetOrAddAsync("a", Fetch);
        Assert.IsTrue(guard.Invalidate("a"));

        await guard.GetOrAddAsync("a", Fetch);

        Assert.AreEqual(2, callCount);
    }

    [TestMethod]
    public void Invalidate_MissingKey_ReturnsFalse()
    {
        CacheStampedeGuard<string, int> guard = new();
        Assert.IsFalse(guard.Invalidate("a"));
    }

    [TestMethod]
    public async Task Clear_RemovesAllEntries()
    {
        CacheStampedeGuard<string, int> guard = new();
        await guard.GetOrAddAsync("a", static () => Task.FromResult(1));
        guard.Clear();
        Assert.AreEqual(0, guard.Count);
    }

    #endregion
}
