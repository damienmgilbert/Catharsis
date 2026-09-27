using Catharsis.Caching;

namespace Catharsis.UnitTests.Caching;

///<summary>
///Unit tests for the <see cref="MemoCache{TKey, TValue}"/> class.
///</summary>
[TestClass]
public class MemoCacheTests
{
    #region GetOrAdd

    [TestMethod]
    public void GetOrAdd_MissingKey_ComputesAndCaches()
    {
        MemoCache<string, int> cache = new();
        int result = cache.GetOrAdd("a", static _ => 42);
        Assert.AreEqual(42, result);
        Assert.AreEqual(1, cache.Count);
    }

    [TestMethod]
    public void GetOrAdd_ExistingKey_ReturnsCachedValueWithoutRecomputing()
    {
        MemoCache<string, int> cache = new();
        int callCount = 0;

        cache.GetOrAdd("a", _ =>
        {
            callCount++;
            return 1;
        });

        cache.GetOrAdd("a", _ =>
        {
            callCount++;
            return 2;
        });

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void GetOrAdd_NullKey_Throws()
    {
        MemoCache<string, int> cache = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => cache.GetOrAdd(null!, static _ => 1));
    }

    [TestMethod]
    public void GetOrAdd_NullFactory_Throws()
    {
        MemoCache<string, int> cache = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => cache.GetOrAdd("a", null!));
    }

    [TestMethod]
    public void GetOrAdd_ConcurrentCallsSameKey_FactoryRunsExactlyOnce()
    {
        MemoCache<string, int> cache = new();
        int callCount = 0;

        Parallel.For(0, 50, _ => cache.GetOrAdd("a", _ =>
        {
            Interlocked.Increment(ref callCount);
            return 1;
        }));

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void GetOrAdd_AfterRemoval_RecomputesValue()
    {
        MemoCache<string, int> cache = new();
        int callCount = 0;

        int First() => cache.GetOrAdd("a", _ =>
        {
            callCount++;
            return callCount;
        });

        Assert.AreEqual(1, First());
        cache.TryRemove("a");
        Assert.AreEqual(2, First());
    }

    #endregion

    #region TryGetValue / TryRemove / ContainsKey / Clear

    [TestMethod]
    public void TryGetValue_MissingKey_ReturnsFalse()
    {
        MemoCache<string, int> cache = new();
        Assert.IsFalse(cache.TryGetValue("a", out int value));
        Assert.AreEqual(0, value);
    }

    [TestMethod]
    public void TryGetValue_ExistingKey_ReturnsCachedValue()
    {
        MemoCache<string, int> cache = new();
        cache.GetOrAdd("a", static _ => 7);
        Assert.IsTrue(cache.TryGetValue("a", out int value));
        Assert.AreEqual(7, value);
    }

    [TestMethod]
    public void ContainsKey_ReflectsPresence()
    {
        MemoCache<string, int> cache = new();
        Assert.IsFalse(cache.ContainsKey("a"));
        cache.GetOrAdd("a", static _ => 1);
        Assert.IsTrue(cache.ContainsKey("a"));
    }

    [TestMethod]
    public void TryRemove_ExistingKey_ReturnsTrueAndRemoves()
    {
        MemoCache<string, int> cache = new();
        cache.GetOrAdd("a", static _ => 1);
        Assert.IsTrue(cache.TryRemove("a"));
        Assert.IsFalse(cache.ContainsKey("a"));
    }

    [TestMethod]
    public void TryRemove_MissingKey_ReturnsFalse()
    {
        MemoCache<string, int> cache = new();
        Assert.IsFalse(cache.TryRemove("a"));
    }

    [TestMethod]
    public void Clear_RemovesAllEntries()
    {
        MemoCache<string, int> cache = new();
        cache.GetOrAdd("a", static _ => 1);
        cache.GetOrAdd("b", static _ => 2);
        cache.Clear();
        Assert.AreEqual(0, cache.Count);
    }

    #endregion
}
