using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

///<summary>
///Unit tests for the <see cref="LruCache"/> class.
///</summary>
[TestClass]
public class LruCacheTests
{
    #region Public methods
    [TestMethod]
    public void AddOrUpdate_EvictsLRU()
    {
        LruCache<string, int> cache = new LruCache<string, int>(2);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);
        cache.AddOrUpdate("c", 3);
        Assert.IsFalse(cache.ContainsKey("a"));
        Assert.IsTrue(cache.ContainsKey("b"));
        Assert.IsTrue(cache.ContainsKey("c"));
    }

    [TestMethod]
    public void AddOrUpdate_StoresValue()
    {
        LruCache<string, int> cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        Assert.IsTrue(cache.TryGetValue("a", out int val));
        Assert.AreEqual(1, val);
    }

    [TestMethod]
    public void Clear_RemovesAll()
    {
        LruCache<string, int> cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        cache.Clear();
        Assert.IsEmpty(cache);
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new LruCache<string, int>(1, null!)); }
    [TestMethod]
    public void Constructor_ZeroCapacity_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new LruCache<string, int>(0)); }
    [TestMethod]
    public void Indexer_Get_ThrowsWhenNotFound()
    {
        LruCache<string, int> cache = new LruCache<string, int>(2);
        Assert.ThrowsExactly<KeyNotFoundException>(() => _ = cache["missing"]);
    }

    [TestMethod]
    public void Indexer_Set_BehavesLikeAddOrUpdate()
    {
        LruCache<string, int> cache = new LruCache<string, int>(2);
        cache["key"] = 42;
        Assert.AreEqual(42, cache["key"]);
    }

    [TestMethod]
    public void Remove_ExistingKey_ReturnsTrue()
    {
        LruCache<string, int> cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        Assert.IsTrue(cache.Remove("a"));
        Assert.IsEmpty(cache);
    }

    [TestMethod]
    public void TryGetValue_PromotesToMRU()
    {
        LruCache<string, int> cache = new LruCache<string, int>(2);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);
        cache.TryGetValue("a", out _); // promotes "a"
        cache.AddOrUpdate("c", 3);     // evicts "b"
        Assert.IsTrue(cache.ContainsKey("a"));
        Assert.IsFalse(cache.ContainsKey("b"));
    }
    #endregion
}
