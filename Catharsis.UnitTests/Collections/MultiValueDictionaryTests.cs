using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="MultiValueDictionary{TKey, TValue}"/> class.
///</summary>
[TestClass]
public class MultiValueDictionaryTests
{
    #region Add

    [TestMethod]
    public void Add_NullKey_Throws()
    {
        MultiValueDictionary<string, int> dictionary = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => dictionary.Add(null!, 1));
    }

    [TestMethod]
    public void Add_FirstValueForKey_CreatesCollection()
    {
        MultiValueDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        Assert.IsTrue(dictionary.ContainsKey("a"));
        Assert.AreEqual(1, dictionary.KeyCount);
        Assert.AreEqual(1, dictionary.ValueCount);
    }

    [TestMethod]
    public void Add_SecondValueForSameKey_AppendsToCollection()
    {
        MultiValueDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        dictionary.Add("a", 2);

        Assert.AreEqual(1, dictionary.KeyCount);
        Assert.AreEqual(2, dictionary.ValueCount);
        CollectionAssert.AreEqual(new[] { 1, 2 }, dictionary["a"].ToList());
    }

    #endregion

    #region TryGetValues / Indexer

    [TestMethod]
    public void TryGetValues_MissingKey_ReturnsFalse()
    {
        MultiValueDictionary<string, int> dictionary = new();
        Assert.IsFalse(dictionary.TryGetValues("a", out IReadOnlyCollection<int> values));
        Assert.IsEmpty(values);
    }

    [TestMethod]
    public void Indexer_MissingKey_Throws()
    {
        MultiValueDictionary<string, int> dictionary = new();
        Assert.ThrowsExactly<KeyNotFoundException>(() => dictionary["missing"]);
    }

    #endregion

    #region Remove / RemoveKey

    [TestMethod]
    public void Remove_OneOfSeveralValues_KeepsKey()
    {
        MultiValueDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        dictionary.Add("a", 2);

        Assert.IsTrue(dictionary.Remove("a", 1));
        Assert.IsTrue(dictionary.ContainsKey("a"));
        CollectionAssert.AreEqual(new[] { 2 }, dictionary["a"].ToList());
    }

    [TestMethod]
    public void Remove_LastValue_RemovesKeyEntirely()
    {
        MultiValueDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);

        Assert.IsTrue(dictionary.Remove("a", 1));
        Assert.IsFalse(dictionary.ContainsKey("a"));
    }

    [TestMethod]
    public void Remove_MissingValue_ReturnsFalse()
    {
        MultiValueDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        Assert.IsFalse(dictionary.Remove("a", 99));
    }

    [TestMethod]
    public void RemoveKey_ExistingKey_RemovesAllValues()
    {
        MultiValueDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        dictionary.Add("a", 2);

        Assert.IsTrue(dictionary.RemoveKey("a"));
        Assert.AreEqual(0, dictionary.KeyCount);
    }

    #endregion

    #region Enumeration / Clear

    [TestMethod]
    public void GetEnumerator_YieldsKeysWithTheirValues()
    {
        MultiValueDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        dictionary.Add("a", 2);
        dictionary.Add("b", 3);

        Dictionary<string, IReadOnlyCollection<int>> snapshot = dictionary.ToDictionary(static kvp => kvp.Key, static kvp => kvp.Value);

        CollectionAssert.AreEqual(new[] { 1, 2 }, snapshot["a"].ToList());
        CollectionAssert.AreEqual(new[] { 3 }, snapshot["b"].ToList());
    }

    [TestMethod]
    public void Clear_RemovesEverything()
    {
        MultiValueDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        dictionary.Clear();
        Assert.AreEqual(0, dictionary.KeyCount);
        Assert.AreEqual(0, dictionary.ValueCount);
    }

    #endregion
}
