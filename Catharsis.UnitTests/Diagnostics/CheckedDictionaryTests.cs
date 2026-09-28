using Catharsis.Diagnostics;

namespace Catharsis.UnitTests.Diagnostics;

///<summary>
///Unit tests for the <see cref="CheckedDictionary{TKey, TValue}"/> class.
///</summary>
[TestClass]
public class CheckedDictionaryTests
{
    #region Add / indexer

    [TestMethod]
    public void Add_NullKey_Throws()
    {
        CheckedDictionary<string, int> dictionary = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => dictionary.Add(null!, 1));
    }

    [TestMethod]
    public void Add_DuplicateKey_Throws()
    {
        CheckedDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        Assert.ThrowsExactly<ArgumentException>(() => dictionary.Add("a", 2));
    }

    [TestMethod]
    public void Add_WithinCapacity_Succeeds()
    {
        CheckedDictionary<string, int> dictionary = new(maxCapacity: 2);
        dictionary.Add("a", 1);
        dictionary.Add("b", 2);
        Assert.AreEqual(2, dictionary.Count);
    }

    [TestMethod]
    public void Add_ExceedsCapacity_Throws()
    {
        CheckedDictionary<string, int> dictionary = new(maxCapacity: 1);
        dictionary.Add("a", 1);
        Assert.ThrowsExactly<InvalidOperationException>(() => dictionary.Add("b", 2));
    }

    [TestMethod]
    public void Add_ExceedsCapacity_NoneMode_DoesNotThrow()
    {
        CheckedDictionary<string, int> dictionary = new(maxCapacity: 1, mode: ValidationMode.None);
        dictionary.Add("a", 1);
        dictionary.Add("b", 2);
        Assert.AreEqual(2, dictionary.Count);
    }

    [TestMethod]
    public void Indexer_SetNewKeyExceedsCapacity_Throws()
    {
        CheckedDictionary<string, int> dictionary = new(maxCapacity: 1);
        dictionary["a"] = 1;
        Assert.ThrowsExactly<InvalidOperationException>(() => dictionary["b"] = 2);
    }

    [TestMethod]
    public void Indexer_SetExistingKeyAtCapacity_Succeeds()
    {
        CheckedDictionary<string, int> dictionary = new(maxCapacity: 1);
        dictionary["a"] = 1;
        dictionary["a"] = 2;
        Assert.AreEqual(2, dictionary["a"]);
    }

    [TestMethod]
    public void Indexer_GetMissingKey_ThrowsKeyNotFoundException()
    {
        CheckedDictionary<string, int> dictionary = new();
        Assert.ThrowsExactly<KeyNotFoundException>(() => _ = dictionary["missing"]);
    }

    [TestMethod]
    public void Constructor_NonPositiveMaxCapacity_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CheckedDictionary<string, int>(maxCapacity: 0));
    }

    #endregion

    #region Delegated members

    [TestMethod]
    public void ContainsKey_ExistingKey_ReturnsTrue()
    {
        CheckedDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        Assert.IsTrue(dictionary.ContainsKey("a"));
    }

    [TestMethod]
    public void Remove_ExistingKey_ReturnsTrueAndRemoves()
    {
        CheckedDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        Assert.IsTrue(dictionary.Remove("a"));
        Assert.IsFalse(dictionary.ContainsKey("a"));
    }

    [TestMethod]
    public void TryGetValue_ExistingKey_ReturnsTrueWithValue()
    {
        CheckedDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        Assert.IsTrue(dictionary.TryGetValue("a", out int value));
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void Clear_RemovesAllEntries()
    {
        CheckedDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        dictionary.Clear();
        Assert.AreEqual(0, dictionary.Count);
    }

    [TestMethod]
    public void GetEnumerator_YieldsAllEntries()
    {
        CheckedDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        dictionary.Add("b", 2);

        List<KeyValuePair<string, int>> entries = [.. dictionary];

        Assert.HasCount(2, entries);
    }

    #endregion
}
