using Catharsis.Common;

namespace Catharsis.UnitTests.Common;

[TestClass]
public class BufferWeakReferenceCacheTests
{
    #region Public methods
    [TestMethod]
    public void Clear_RemovesAll()
    {
        BufferWeakReferenceCache<string> cache = new BufferWeakReferenceCache<string>();
        cache.Set("a", "1");
        cache.Set("b", "2");
        cache.Clear();
        Assert.AreEqual(0, cache.Count);
    }

    [TestMethod]
    public void Count_ReflectsEntries()
    {
        BufferWeakReferenceCache<string> cache = new BufferWeakReferenceCache<string>();
        cache.Set("a", "1");
        cache.Set("b", "2");
        Assert.AreEqual(2, cache.Count);
    }

    [TestMethod]
    public void GetOrCreate_CreatesOnMiss()
    {
        BufferWeakReferenceCache<string> cache = new BufferWeakReferenceCache<string>();
        string val = cache.GetOrCreate("k", () => "created");
        Assert.AreEqual("created", val);
    }

    [TestMethod]
    public void GetOrCreate_ReturnsCachedOnHit()
    {
        BufferWeakReferenceCache<string> cache = new BufferWeakReferenceCache<string>();
        cache.Set("k", "original");
        string val = cache.GetOrCreate("k", () => "new");
        Assert.AreEqual("original", val);
    }

    [TestMethod]
    public void Remove_ReturnsCorrectResult()
    {
        BufferWeakReferenceCache<string> cache = new BufferWeakReferenceCache<string>();
        cache.Set("k", "v");
        Assert.IsTrue(cache.Remove("k"));
        Assert.IsFalse(cache.Remove("k"));
    }

    [TestMethod]
    public void Set_And_TryGet_ReturnsValue()
    {
        BufferWeakReferenceCache<string> cache = new BufferWeakReferenceCache<string>();
        cache.Set("key", "value");
        Assert.IsTrue(cache.TryGet("key", out string? val));
        Assert.AreEqual("value", val);
    }

    [TestMethod]
    public void TryGet_MissingKey_ReturnsFalse()
    {
        BufferWeakReferenceCache<string> cache = new BufferWeakReferenceCache<string>();
        Assert.IsFalse(cache.TryGet("missing", out _));
    }
    #endregion
}
