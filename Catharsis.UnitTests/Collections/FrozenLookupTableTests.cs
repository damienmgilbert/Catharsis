using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="FrozenLookupTable{TKey,TValue}"/> class.
///</summary>
[TestClass]
public class FrozenLookupTableTests
{
    #region Add

    [TestMethod]
    public void Add_NullKey_Throws()
    {
        FrozenLookupTable<string, int> table = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => table.Add(null!, 1));
    }

    [TestMethod]
    public void Add_ReturnsSameInstanceForChaining()
    {
        FrozenLookupTable<string, int> table = new();
        Assert.AreSame(table, table.Add("a", 1));
    }

    [TestMethod]
    public void Add_AfterBuild_Throws()
    {
        FrozenLookupTable<string, int> table = new FrozenLookupTable<string, int>().Add("a", 1).Build();
        Assert.ThrowsExactly<InvalidOperationException>(() => table.Add("b", 2));
    }

    [TestMethod]
    public void Add_SameKeyTwice_OverwritesValue()
    {
        FrozenLookupTable<string, int> table = new FrozenLookupTable<string, int>().Add("a", 1).Add("a", 2).Build();
        Assert.AreEqual(2, table["a"]);
    }

    #endregion

    #region Build

    [TestMethod]
    public void Build_CalledTwice_DoesNotThrow()
    {
        FrozenLookupTable<string, int> table = new FrozenLookupTable<string, int>().Add("a", 1).Build();
        table.Build();
    }

    [TestMethod]
    public void IsBuilt_BeforeBuild_IsFalse() { Assert.IsFalse(new FrozenLookupTable<string, int>().IsBuilt); }

    [TestMethod]
    public void IsBuilt_AfterBuild_IsTrue() { Assert.IsTrue(new FrozenLookupTable<string, int>().Build().IsBuilt); }

    #endregion

    #region Indexer / TryGetValue

    [TestMethod]
    public void Indexer_BeforeBuild_Throws()
    {
        FrozenLookupTable<string, int> table = new FrozenLookupTable<string, int>().Add("a", 1);
        Assert.ThrowsExactly<InvalidOperationException>(() => _ = table["a"]);
    }

    [TestMethod]
    public void Indexer_AfterBuild_ReturnsValue()
    {
        FrozenLookupTable<string, int> table = new FrozenLookupTable<string, int>().Add("a", 1).Build();
        Assert.AreEqual(1, table["a"]);
    }

    [TestMethod]
    public void Indexer_MissingKey_Throws()
    {
        FrozenLookupTable<string, int> table = new FrozenLookupTable<string, int>().Add("a", 1).Build();
        Assert.ThrowsExactly<KeyNotFoundException>(() => _ = table["b"]);
    }

    [TestMethod]
    public void TryGetValue_NullKey_Throws()
    {
        FrozenLookupTable<string, int> table = new FrozenLookupTable<string, int>().Build();
        Assert.ThrowsExactly<ArgumentNullException>(() => table.TryGetValue(null!, out _));
    }

    [TestMethod]
    public void TryGetValue_BeforeBuild_Throws()
    {
        FrozenLookupTable<string, int> table = new FrozenLookupTable<string, int>().Add("a", 1);
        Assert.ThrowsExactly<InvalidOperationException>(() => table.TryGetValue("a", out _));
    }

    [TestMethod]
    public void TryGetValue_PresentKey_ReturnsTrueAndValue()
    {
        FrozenLookupTable<string, int> table = new FrozenLookupTable<string, int>().Add("a", 1).Build();

        bool found = table.TryGetValue("a", out int value);

        Assert.IsTrue(found);
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TryGetValue_AbsentKey_ReturnsFalse()
    {
        FrozenLookupTable<string, int> table = new FrozenLookupTable<string, int>().Build();
        Assert.IsFalse(table.TryGetValue("a", out _));
    }

    #endregion

    #region Count

    [TestMethod]
    public void Count_BeforeBuild_ReflectsAddedEntries() { Assert.AreEqual(2, new FrozenLookupTable<string, int>().Add("a", 1).Add("b", 2).Count); }

    [TestMethod]
    public void Count_AfterBuild_ReflectsAddedEntries() { Assert.AreEqual(2, new FrozenLookupTable<string, int>().Add("a", 1).Add("b", 2).Build().Count); }

    #endregion

    #region Comparer

    [TestMethod]
    public void Constructor_CaseInsensitiveComparer_TreatsKeysAsEquivalent()
    {
        FrozenLookupTable<string, int> table = new FrozenLookupTable<string, int>(StringComparer.OrdinalIgnoreCase).Add("A", 1).Build();
        Assert.AreEqual(1, table["a"]);
    }

    #endregion
}
