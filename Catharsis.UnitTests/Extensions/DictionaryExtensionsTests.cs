namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class DictionaryExtensionsTests
{
    [TestMethod]
    public void AddOrUpdate_NewKey_AddsEntry()
    {
        IDictionary<string, int> source = new Dictionary<string, int>();
        source.AddOrUpdate("a", 1);
        Assert.AreEqual(1, source["a"]);
    }

    [TestMethod]
    public void AddOrUpdate_ExistingKey_UpdatesEntry()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { { "a", 1 } };
        source.AddOrUpdate("a", 99);
        Assert.AreEqual(99, source["a"]);
    }

    [TestMethod]
    public void AddOrUpdate_WithFactories_AddsNewKey()
    {
        IDictionary<string, int> source = new Dictionary<string, int>();
        var value = source.AddOrUpdate("a", k => 10, (k, v) => v + 1);
        Assert.AreEqual(10, value);
        Assert.AreEqual(10, source["a"]);
    }

    [TestMethod]
    public void AddOrUpdate_WithFactories_UpdatesExistingKey()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { { "a", 10 } };
        var value = source.AddOrUpdate("a", k => 0, (k, v) => v + 5);
        Assert.AreEqual(15, value);
        Assert.AreEqual(15, source["a"]);
    }

    [TestMethod]
    public void AddRange_AddsMultipleEntries()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { { "a", 1 } };
        source.AddRange(new Dictionary<string, int> { { "b", 2 }, { "c", 3 } });
        Assert.AreEqual(3, source.Count);
        Assert.AreEqual(2, source["b"]);
        Assert.AreEqual(3, source["c"]);
    }

    [TestMethod]
    public void AddRange_OverwritesExistingKeys()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { { "a", 1 } };
        source.AddRange(new Dictionary<string, int> { { "a", 99 } });
        Assert.AreEqual(99, source["a"]);
    }

    [TestMethod]
    public void RemoveRange_RemovesMatchingKeys_ReturnsCount()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        int removed = source.RemoveRange(new[] { "a", "c", "z" });
        Assert.AreEqual(2, removed);
        Assert.AreEqual(1, source.Count);
        Assert.IsTrue(source.ContainsKey("b"));
    }

    [TestMethod]
    public void RemoveWhere_RemovesMatchingEntries_ReturnsCount()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        int removed = source.RemoveWhere(kvp => kvp.Value > 1);
        Assert.AreEqual(2, removed);
        Assert.AreEqual(1, source.Count);
        Assert.AreEqual(1, source["a"]);
    }

    [TestMethod]
    public void ReplaceValue_ExistingKey_ReplacesAndReturnsTrue()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { { "a", 1 } };
        bool replaced = source.ReplaceValue("a", 99);
        Assert.IsTrue(replaced);
        Assert.AreEqual(99, source["a"]);
    }

    [TestMethod]
    public void ReplaceValue_NonExistentKey_ReturnsFalse()
    {
        IDictionary<string, int> source = new Dictionary<string, int>();
        bool replaced = source.ReplaceValue("z", 99);
        Assert.IsFalse(replaced);
    }

    [TestMethod]
    public void ModifyAll_TransformsAllValues()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        source.ModifyAll((k, v) => v * 10);
        Assert.AreEqual(10, source["a"]);
        Assert.AreEqual(20, source["b"]);
    }

    [TestMethod]
    public void ModifyWhere_TransformsMatchingOnly()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        source.ModifyWhere(kvp => kvp.Value % 2 == 0, (k, v) => v * 10);
        Assert.AreEqual(1, source["a"]);
        Assert.AreEqual(20, source["b"]);
        Assert.AreEqual(3, source["c"]);
    }

    [TestMethod]
    public void GetOrAdd_ExistingKey_ReturnsExistingValue()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { { "a", 42 } };
        var result = source.GetOrAdd("a", k => 99);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void GetOrAdd_MissingKey_AddsAndReturnsNewValue()
    {
        IDictionary<string, int> source = new Dictionary<string, int>();
        var result = source.GetOrAdd("a", k => 42);
        Assert.AreEqual(42, result);
        Assert.AreEqual(42, source["a"]);
    }

    [TestMethod]
    public void NullSource_ThrowsArgumentNullException()
    {
        IDictionary<string, int>? source = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.AddOrUpdate("a", 1));
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.AddRange(new Dictionary<string, int>()));
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.RemoveRange(new[] { "a" }));
    }
}
