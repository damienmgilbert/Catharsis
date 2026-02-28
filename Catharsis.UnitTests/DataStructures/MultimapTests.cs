using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

[TestClass]
public class MultimapTests
{
    #region Public methods
    [TestMethod]
    public void Add_StoresValueUnderKey()
    {
        Multimap<string, int> mm = new Multimap<string, int>();
        mm.Add("key", 1);
        mm.Add("key", 2);
        Assert.AreEqual(1, mm.KeyCount);
        Assert.AreEqual(2, mm.ValueCount);
    }

    [TestMethod]
    public void AddRange_NullValues_Throws()
    {
        Multimap<string, int> mm = new Multimap<string, int>();
        Assert.ThrowsExactly<ArgumentNullException>(() => mm.AddRange("key", null!));
    }

    [TestMethod]
    public void AddRange_StoresMultipleValues()
    {
        Multimap<string, int> mm = new Multimap<string, int>();
        mm.AddRange("key", [ 1, 2, 3 ]);
        Assert.AreEqual(3, mm.ValueCount);
    }

    [TestMethod]
    public void Clear_RemovesAll()
    {
        Multimap<string, int> mm = new Multimap<string, int>();
        mm.Add("a", 1);
        mm.Clear();
        Assert.AreEqual(0, mm.KeyCount);
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws() { Assert.ThrowsExactly<ArgumentNullException>(() => new Multimap<string, int>(null!)); }
    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        Multimap<string, int> mm = new Multimap<string, int>();
        mm.Add("k", 42);
        Assert.IsTrue(mm.Contains("k", 42));
        Assert.IsFalse(mm.Contains("k", 99));
    }

    [TestMethod]
    public void Indexer_ReturnsValuesForKey()
    {
        Multimap<string, int> mm = new Multimap<string, int>();
        mm.Add("x", 1);
        mm.Add("x", 2);
        IReadOnlyCollection<int> vals = mm["x"];
        Assert.HasCount(2, vals);
    }

    [TestMethod]
    public void Remove_LastValue_RemovesKey()
    {
        Multimap<string, int> mm = new Multimap<string, int>();
        mm.Add("key", 1);
        mm.Remove("key", 1);
        Assert.IsFalse(mm.ContainsKey("key"));
    }

    [TestMethod]
    public void Remove_SpecificValue_ReturnsTrue()
    {
        Multimap<string, int> mm = new Multimap<string, int>();
        mm.Add("key", 1);
        mm.Add("key", 2);
        Assert.IsTrue(mm.Remove("key", 1));
        Assert.AreEqual(1, mm.ValueCount);
    }

    [TestMethod]
    public void RemoveAll_RemovesKey()
    {
        Multimap<string, int> mm = new Multimap<string, int>();
        mm.Add("key", 1);
        mm.Add("key", 2);
        Assert.IsTrue(mm.RemoveAll("key"));
        Assert.AreEqual(0, mm.KeyCount);
    }

    [TestMethod]
    public void TryGetValues_ReturnsCorrectResult()
    {
        Multimap<string, int> mm = new Multimap<string, int>();
        mm.Add("k", 42);
        Assert.IsTrue(mm.TryGetValues("k", out IReadOnlyCollection<int>? vals));
        Assert.HasCount(1, vals);
        Assert.IsFalse(mm.TryGetValues("missing", out _));
    }
    #endregion
}
