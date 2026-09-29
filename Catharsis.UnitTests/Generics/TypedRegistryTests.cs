using Catharsis.Generics;

namespace Catharsis.UnitTests.Generics;

///<summary>
///Unit tests for the <see cref="TypedRegistry{TKey, TValue}"/> class.
///</summary>
[TestClass]
public class TypedRegistryTests
{
    [TestMethod]
    public void Register_Duplicate_Throws()
    {
        TypedRegistry<string, int> registry = new();
        registry.Register("a", 1);

        Assert.ThrowsExactly<InvalidOperationException>(() => registry.Register("a", 2));
        Assert.AreEqual(1, registry["a"]);
    }

    [TestMethod]
    public void Set_ReplacesExisting()
    {
        TypedRegistry<string, int> registry = new();
        registry.Register("a", 1);
        registry.Set("a", 2);

        Assert.AreEqual(2, registry["a"]);
    }

    [TestMethod]
    public void Indexer_Missing_Throws() { Assert.ThrowsExactly<KeyNotFoundException>(static () => new TypedRegistry<string, int>()["x"]); }

    [TestMethod]
    public void NullKey_Throws()
    {
        TypedRegistry<string, int> registry = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => registry.Register(null!, 1));
        Assert.ThrowsExactly<ArgumentNullException>(() => registry.TryGet(null!, out _));
    }

    [TestMethod]
    public void TryGet_ReportsPresence()
    {
        TypedRegistry<string, int> registry = new();
        registry.Register("a", 1);

        Assert.IsTrue(registry.TryGet("a", out int value));
        Assert.AreEqual(1, value);
        Assert.IsFalse(registry.TryGet("b", out _));
    }

    [TestMethod]
    public void GetOrAdd_CreatesOnceThenReuses()
    {
        TypedRegistry<string, int> registry = new();
        int creations = 0;

        registry.GetOrAdd("a", _ => ++creations);
        registry.GetOrAdd("a", _ => ++creations);

        Assert.AreEqual(1, creations);
    }

    [TestMethod]
    public void Remove_And_Contains_And_Count()
    {
        TypedRegistry<string, int> registry = new();
        registry.Register("a", 1);

        Assert.IsTrue(registry.Contains("a"));
        Assert.IsTrue(registry.Remove("a"));
        Assert.IsFalse(registry.Remove("a"));
        Assert.AreEqual(0, registry.Count);
    }

    [TestMethod]
    public void Comparer_IsHonored()
    {
        TypedRegistry<string, int> registry = new(StringComparer.OrdinalIgnoreCase);
        registry.Register("Key", 1);

        Assert.AreEqual(1, registry["KEY"]);
        Assert.ThrowsExactly<InvalidOperationException>(() => registry.Register("kEY", 2));
    }

    [TestMethod]
    public void Enumeration_And_Keys_ReflectEntries()
    {
        TypedRegistry<string, int> registry = new();
        registry.Register("a", 1);
        registry.Register("b", 2);

        Assert.AreEqual(3, registry.Sum(static pair => pair.Value));
        CollectionAssert.AreEquivalent(new[] { "a", "b" }, registry.Keys.ToArray());
    }
}
