using Catharsis.Caching;

namespace Catharsis.UnitTests.Caching;

///<summary>
///Unit tests for the <see cref="TtlCache{TKey, TValue}"/> class.
///</summary>
[TestClass]
public class TtlCacheTests
{
    #region Construction

    [TestMethod]
    public void Constructor_ZeroOrNegativeDefaultTtl_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new TtlCache<string, int>(TimeSpan.Zero));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new TtlCache<string, int>(TimeSpan.FromMilliseconds(-1)));
    }

    #endregion

    #region Set / TryGetValue

    [TestMethod]
    public void Set_ThenTryGetValue_ReturnsValue()
    {
        TtlCache<string, int> cache = new(TimeSpan.FromMinutes(1));
        cache.Set("a", 42);
        Assert.IsTrue(cache.TryGetValue("a", out int value));
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryGetValue_MissingKey_ReturnsFalse()
    {
        TtlCache<string, int> cache = new(TimeSpan.FromMinutes(1));
        Assert.IsFalse(cache.TryGetValue("a", out int value));
        Assert.AreEqual(0, value);
    }

    [TestMethod]
    public void Set_NullKey_Throws()
    {
        TtlCache<string, int> cache = new(TimeSpan.FromMinutes(1));
        Assert.ThrowsExactly<ArgumentNullException>(() => cache.Set(null!, 1));
    }

    [TestMethod]
    public void Set_ZeroOrNegativeTtl_Throws()
    {
        TtlCache<string, int> cache = new(TimeSpan.FromMinutes(1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => cache.Set("a", 1, TimeSpan.Zero));
    }

    [TestMethod]
    public void Set_ExistingKey_OverwritesValueAndTtl()
    {
        TtlCache<string, int> cache = new(TimeSpan.FromMinutes(1));
        cache.Set("a", 1);
        cache.Set("a", 2);
        Assert.IsTrue(cache.TryGetValue("a", out int value));
        Assert.AreEqual(2, value);
    }

    #endregion

    #region Expiration

    [TestMethod]
    public void TryGetValue_BeforeExpiration_ReturnsValue()
    {
        ManualTimeProvider time = new(DateTimeOffset.UtcNow);
        TtlCache<string, int> cache = new(TimeSpan.FromMinutes(1), time);
        cache.Set("a", 42);

        time.Advance(TimeSpan.FromSeconds(30));

        Assert.IsTrue(cache.TryGetValue("a", out int value));
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryGetValue_AfterExpiration_ReturnsFalseAndRemovesEntry()
    {
        ManualTimeProvider time = new(DateTimeOffset.UtcNow);
        TtlCache<string, int> cache = new(TimeSpan.FromMinutes(1), time);
        cache.Set("a", 42);

        time.Advance(TimeSpan.FromMinutes(2));

        Assert.IsFalse(cache.TryGetValue("a", out int value));
        Assert.AreEqual(0, value);
        Assert.AreEqual(0, cache.Count);
    }

    [TestMethod]
    public void Set_PerEntryTtlOverridesDefault()
    {
        ManualTimeProvider time = new(DateTimeOffset.UtcNow);
        TtlCache<string, int> cache = new(TimeSpan.FromMinutes(10), time);
        cache.Set("a", 42, TimeSpan.FromSeconds(5));

        time.Advance(TimeSpan.FromSeconds(10));

        Assert.IsFalse(cache.TryGetValue("a", out _));
    }

    [TestMethod]
    public void RemoveExpired_RemovesOnlyExpiredEntries()
    {
        ManualTimeProvider time = new(DateTimeOffset.UtcNow);
        TtlCache<string, int> cache = new(TimeSpan.FromMinutes(10), time);
        cache.Set("expired", 1, TimeSpan.FromSeconds(5));
        cache.Set("fresh", 2, TimeSpan.FromMinutes(10));

        time.Advance(TimeSpan.FromSeconds(10));

        int removed = cache.RemoveExpired();

        Assert.AreEqual(1, removed);
        Assert.AreEqual(1, cache.Count);
        Assert.IsTrue(cache.TryGetValue("fresh", out int value));
        Assert.AreEqual(2, value);
    }

    #endregion

    #region TryRemove / Clear

    [TestMethod]
    public void TryRemove_ExistingKey_ReturnsTrueAndRemoves()
    {
        TtlCache<string, int> cache = new(TimeSpan.FromMinutes(1));
        cache.Set("a", 1);
        Assert.IsTrue(cache.TryRemove("a"));
        Assert.IsFalse(cache.TryGetValue("a", out _));
    }

    [TestMethod]
    public void Clear_RemovesAllEntries()
    {
        TtlCache<string, int> cache = new(TimeSpan.FromMinutes(1));
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Clear();
        Assert.AreEqual(0, cache.Count);
    }

    #endregion

    sealed class ManualTimeProvider(DateTimeOffset start) : TimeProvider
    {
        DateTimeOffset _now = start;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
