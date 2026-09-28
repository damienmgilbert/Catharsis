using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ExpandoBackedBag{T}"/> class.
///</summary>
[TestClass]
public class ExpandoBackedBagTests
{
    #region Indexer

    [TestMethod]
    public void Indexer_Get_MissingKey_Throws()
    {
        ExpandoBackedBag<int> bag = new();
        Assert.ThrowsExactly<KeyNotFoundException>(() => _ = bag["a"]);
    }

    [TestMethod]
    public void Indexer_Set_ThenGet_ReturnsValue()
    {
        ExpandoBackedBag<int> bag = new() { ["a"] = 1 };
        Assert.AreEqual(1, bag["a"]);
    }

    [TestMethod]
    public void Indexer_SetTwice_OverwritesValue()
    {
        ExpandoBackedBag<int> bag = new() { ["a"] = 1 };
        bag["a"] = 2;

        Assert.AreEqual(2, bag["a"]);
    }

    [TestMethod]
    public void Indexer_Get_NullKey_Throws()
    {
        ExpandoBackedBag<int> bag = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = bag[null!]);
    }

    [TestMethod]
    public void Indexer_Set_NullKey_Throws()
    {
        ExpandoBackedBag<int> bag = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => bag[null!] = 1);
    }

    #endregion

    #region TryGetValue

    [TestMethod]
    public void TryGetValue_PresentKeyMatchingType_ReturnsTrueAndValue()
    {
        ExpandoBackedBag<int> bag = new() { ["a"] = 1 };

        bool found = bag.TryGetValue("a", out int value);

        Assert.IsTrue(found);
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TryGetValue_AbsentKey_ReturnsFalse()
    {
        ExpandoBackedBag<int> bag = new();
        Assert.IsFalse(bag.TryGetValue("a", out _));
    }

    [TestMethod]
    public void TryGetValue_NullKey_Throws()
    {
        ExpandoBackedBag<int> bag = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => bag.TryGetValue(null!, out _));
    }

    #endregion

    #region ContainsKey / Remove / Count

    [TestMethod]
    public void ContainsKey_PresentKey_ReturnsTrue()
    {
        ExpandoBackedBag<int> bag = new() { ["a"] = 1 };
        Assert.IsTrue(bag.ContainsKey("a"));
    }

    [TestMethod]
    public void ContainsKey_AbsentKey_ReturnsFalse()
    {
        ExpandoBackedBag<int> bag = new();
        Assert.IsFalse(bag.ContainsKey("a"));
    }

    [TestMethod]
    public void ContainsKey_NullKey_Throws()
    {
        ExpandoBackedBag<int> bag = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => bag.ContainsKey(null!));
    }

    [TestMethod]
    public void Remove_PresentKey_ReturnsTrueAndRemovesEntry()
    {
        ExpandoBackedBag<int> bag = new() { ["a"] = 1 };

        bool removed = bag.Remove("a");

        Assert.IsTrue(removed);
        Assert.IsFalse(bag.ContainsKey("a"));
    }

    [TestMethod]
    public void Remove_AbsentKey_ReturnsFalse()
    {
        ExpandoBackedBag<int> bag = new();
        Assert.IsFalse(bag.Remove("a"));
    }

    [TestMethod]
    public void Count_ReflectsNumberOfEntries() { Assert.AreEqual(2, new ExpandoBackedBag<int> { ["a"] = 1, ["b"] = 2 }.Count); }

    #endregion

    #region Enumeration

    [TestMethod]
    public void Enumeration_YieldsAllEntries()
    {
        ExpandoBackedBag<int> bag = new() { ["a"] = 1, ["b"] = 2 };
        CollectionAssert.AreEquivalent(
            new[] { new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("b", 2) },
            bag.ToList());
    }

    #endregion

    #region AsDynamic

    [TestMethod]
    public void AsDynamic_SetViaIndexer_IsReadableAsDynamicMember()
    {
        ExpandoBackedBag<int> bag = new() { ["a"] = 1 };
        dynamic dyn = bag.AsDynamic;

        Assert.AreEqual(1, dyn.a);
    }

    [TestMethod]
    public void AsDynamic_SetViaDynamicMember_IsReadableViaIndexer()
    {
        ExpandoBackedBag<int> bag = new();
        dynamic dyn = bag.AsDynamic;
        dyn.a = 5;

        Assert.AreEqual(5, bag["a"]);
    }

    #endregion
}
