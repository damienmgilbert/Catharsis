using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="OrderedNameValueCollection"/> class.
///</summary>
[TestClass]
public class OrderedNameValueCollectionTests
{
    #region Add

    [TestMethod]
    public void Add_NullKey_Throws()
    {
        OrderedNameValueCollection collection = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => collection.Add(null!, "value"));
    }

    [TestMethod]
    public void Add_NullValue_Throws()
    {
        OrderedNameValueCollection collection = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => collection.Add("key", null!));
    }

    [TestMethod]
    public void Add_DuplicateKey_Throws()
    {
        OrderedNameValueCollection collection = new();
        collection.Add("a", "1");

        Assert.ThrowsExactly<ArgumentException>(() => collection.Add("a", "2"));
    }

    #endregion

    #region Indexer

    [TestMethod]
    public void Indexer_Get_MissingKey_Throws()
    {
        OrderedNameValueCollection collection = new();
        Assert.ThrowsExactly<KeyNotFoundException>(() => _ = collection["a"]);
    }

    [TestMethod]
    public void Indexer_Set_NewKey_AppendsEntry()
    {
        OrderedNameValueCollection collection = new() { ["a"] = "1" };
        Assert.AreEqual("1", collection["a"]);
    }

    [TestMethod]
    public void Indexer_Set_ExistingKey_OverwritesValue()
    {
        OrderedNameValueCollection collection = new() { ["a"] = "1" };
        collection["a"] = "2";

        Assert.AreEqual("2", collection["a"]);
    }

    #endregion

    #region Ordering

    [TestMethod]
    public void GetAt_ReturnsEntriesInInsertionOrder()
    {
        OrderedNameValueCollection collection = new() { ["b"] = "2", ["a"] = "1" };

        Assert.AreEqual(new KeyValuePair<string, string>("b", "2"), collection.GetAt(0));
        Assert.AreEqual(new KeyValuePair<string, string>("a", "1"), collection.GetAt(1));
    }

    [TestMethod]
    public void Enumeration_YieldsEntriesInInsertionOrder()
    {
        OrderedNameValueCollection collection = new() { ["b"] = "2", ["a"] = "1" };

        CollectionAssert.AreEqual(
            new[] { new KeyValuePair<string, string>("b", "2"), new KeyValuePair<string, string>("a", "1") },
            collection.ToList());
    }

    [TestMethod]
    public void RemoveAt_RemovesEntryAtPosition()
    {
        OrderedNameValueCollection collection = new() { ["a"] = "1", ["b"] = "2" };

        collection.RemoveAt(0);

        Assert.IsFalse(collection.ContainsKey("a"));
        Assert.IsTrue(collection.ContainsKey("b"));
    }

    #endregion

    #region ContainsKey / Remove / Clear / Count

    [TestMethod]
    public void ContainsKey_NullKey_Throws()
    {
        OrderedNameValueCollection collection = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => collection.ContainsKey(null!));
    }

    [TestMethod]
    public void Remove_PresentKey_ReturnsTrueAndRemovesEntry()
    {
        OrderedNameValueCollection collection = new() { ["a"] = "1" };

        bool removed = collection.Remove("a");

        Assert.IsTrue(removed);
        Assert.IsFalse(collection.ContainsKey("a"));
    }

    [TestMethod]
    public void Remove_AbsentKey_ReturnsFalse()
    {
        OrderedNameValueCollection collection = new();
        Assert.IsFalse(collection.Remove("a"));
    }

    [TestMethod]
    public void Clear_RemovesAllEntries()
    {
        OrderedNameValueCollection collection = new() { ["a"] = "1", ["b"] = "2" };

        collection.Clear();

        Assert.AreEqual(0, collection.Count);
    }

    [TestMethod]
    public void Count_ReflectsNumberOfEntries() { Assert.AreEqual(2, new OrderedNameValueCollection { ["a"] = "1", ["b"] = "2" }.Count); }

    #endregion
}
