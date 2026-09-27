using Catharsis.Caching;

namespace Catharsis.UnitTests.Caching;

///<summary>
///Unit tests for the <see cref="LayeredCache{TKey, TValue}"/> class.
///</summary>
[TestClass]
public class LayeredCacheTests
{
    #region Construction

    [TestMethod]
    public void Constructor_NullBacking_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new LayeredCache<string, int>(null!));
    }

    #endregion

    #region TryGetAsync

    [TestMethod]
    public async Task TryGetAsync_MissingInBothLayers_ReturnsNotFound()
    {
        FakeCacheLayer backing = new();
        LayeredCache<string, int> cache = new(backing);

        (bool found, int value) = await cache.TryGetAsync("a");

        Assert.IsFalse(found);
        Assert.AreEqual(0, value);
    }

    [TestMethod]
    public async Task TryGetAsync_HitInBackingLayer_PopulatesLocalLayer()
    {
        FakeCacheLayer backing = new();
        backing.Store["a"] = 42;
        LayeredCache<string, int> cache = new(backing);

        (bool found, int value) = await cache.TryGetAsync("a");

        Assert.IsTrue(found);
        Assert.AreEqual(42, value);
        Assert.AreEqual(1, cache.LocalCount);
        Assert.AreEqual(1, backing.GetCallCount);
    }

    [TestMethod]
    public async Task TryGetAsync_HitInLocalLayer_DoesNotCallBackingLayer()
    {
        FakeCacheLayer backing = new();
        backing.Store["a"] = 1;
        LayeredCache<string, int> cache = new(backing);

        await cache.TryGetAsync("a");
        backing.GetCallCount = 0;

        (bool found, int value) = await cache.TryGetAsync("a");

        Assert.IsTrue(found);
        Assert.AreEqual(1, value);
        Assert.AreEqual(0, backing.GetCallCount);
    }

    [TestMethod]
    public async Task TryGetAsync_NullKey_Throws()
    {
        LayeredCache<string, int> cache = new(new FakeCacheLayer());
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await cache.TryGetAsync(null!));
    }

    #endregion

    #region SetAsync

    [TestMethod]
    public async Task SetAsync_WritesToLocalAndBackingLayers()
    {
        FakeCacheLayer backing = new();
        LayeredCache<string, int> cache = new(backing);

        await cache.SetAsync("a", 7);

        Assert.AreEqual(1, cache.LocalCount);
        Assert.AreEqual(7, backing.Store["a"]);
    }

    [TestMethod]
    public async Task SetAsync_NullKey_Throws()
    {
        LayeredCache<string, int> cache = new(new FakeCacheLayer());
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await cache.SetAsync(null!, 1));
    }

    #endregion

    #region InvalidateLocal / ClearLocal

    [TestMethod]
    public async Task InvalidateLocal_RemovesFromLocalOnly()
    {
        FakeCacheLayer backing = new();
        LayeredCache<string, int> cache = new(backing);
        await cache.SetAsync("a", 1);

        Assert.IsTrue(cache.InvalidateLocal("a"));
        Assert.AreEqual(0, cache.LocalCount);
        Assert.AreEqual(1, backing.Store["a"]);
    }

    [TestMethod]
    public async Task ClearLocal_RemovesAllLocalEntries()
    {
        FakeCacheLayer backing = new();
        LayeredCache<string, int> cache = new(backing);
        await cache.SetAsync("a", 1);
        await cache.SetAsync("b", 2);

        cache.ClearLocal();

        Assert.AreEqual(0, cache.LocalCount);
    }

    #endregion

    sealed class FakeCacheLayer : ICacheLayer<string, int>
    {
        public Dictionary<string, int> Store { get; } = [];

        public int GetCallCount { get; set; }

        public ValueTask<(bool Found, int Value)> TryGetAsync(string key, CancellationToken cancellationToken = default)
        {
            GetCallCount++;
            return ValueTask.FromResult(Store.TryGetValue(key, out int value) ? (true, value) : (false, 0));
        }

        public ValueTask SetAsync(string key, int value, CancellationToken cancellationToken = default)
        {
            Store[key] = value;
            return ValueTask.CompletedTask;
        }
    }
}
