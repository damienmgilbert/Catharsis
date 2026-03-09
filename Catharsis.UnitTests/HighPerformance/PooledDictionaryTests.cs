using Catharsis.HighPerformance;

namespace Catharsis.UnitTests.HighPerformance;

///<summary>
///Unit tests for the <see cref="PooledDictionary"/> class.
///</summary>
[TestClass]
public class PooledDictionaryTests
{
    #region Public methods
    [TestMethod]
    public void Add_And_Retrieve()
    {
        using PooledDictionary<string, int> dict = new PooledDictionary<string, int>();
        dict.Add("key", 42);
        Assert.AreEqual(42, dict["key"]);
        Assert.HasCount(1, dict);
    }

    [TestMethod]
    public void Clear_RemovesAll()
    {
        using PooledDictionary<string, int> dict = new PooledDictionary<string, int>();
        dict.Add("a", 1);
        dict.Clear();
        Assert.IsEmpty(dict);
    }

    [TestMethod]
    public void ContainsKey_ReturnsCorrectResult()
    {
        using PooledDictionary<string, int> dict = new PooledDictionary<string, int>();
        dict.Add("a", 1);
        Assert.IsTrue(dict.ContainsKey("a"));
        Assert.IsFalse(dict.ContainsKey("b"));
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        PooledDictionary<string, int> dict = new PooledDictionary<string, int>();
        dict.Dispose();
        dict.Dispose();
    }

    [TestMethod]
    public void Indexer_AfterDispose_Throws()
    {
        PooledDictionary<string, int> dict = new PooledDictionary<string, int>();
        dict.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = dict["key"]);
    }

    [TestMethod]
    public void Remove_ExistingKey_ReturnsTrue()
    {
        using PooledDictionary<string, int> dict = new PooledDictionary<string, int>();
        dict.Add("key", 1);
        Assert.IsTrue(dict.Remove("key"));
        Assert.IsEmpty(dict);
    }

    [TestMethod]
    public void TryAdd_Duplicate_ReturnsFalse()
    {
        using PooledDictionary<string, int> dict = new PooledDictionary<string, int>();
        dict.Add("key", 1);
        Assert.IsFalse(dict.TryAdd("key", 2));
    }

    [TestMethod]
    public void TryGetValue_ReturnsCorrectResult()
    {
        using PooledDictionary<string, int> dict = new PooledDictionary<string, int>();
        dict.Add("a", 42);
        Assert.IsTrue(dict.TryGetValue("a", out int val));
        Assert.AreEqual(42, val);
    }
    #endregion
}
