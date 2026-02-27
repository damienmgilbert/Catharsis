using System.Collections.Immutable;

namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class ImmutableCollectionExtensionsTests
{
    // ── ImmutableArray<T> ───────────────────────────────────────────

    [TestMethod]
    public void ImmutableArray_InsertAt_InsertsElement()
    {
        var source = ImmutableArray.Create(1, 2, 4);
        var result = source.InsertAt(2, 3);
        Assert.AreEqual(4, result.Length);
        Assert.AreEqual(3, result[2]);
    }

    [TestMethod]
    public void ImmutableArray_RemoveWhere_RemovesMatchingElements()
    {
        var source = ImmutableArray.Create(1, 2, 3, 4, 5, 6);
        var result = source.RemoveWhere(x => x % 2 == 0);
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableArray_ReplaceAt_ReplacesElement()
    {
        var source = ImmutableArray.Create(1, 2, 3);
        var result = source.ReplaceAt(1, 99);
        CollectionAssert.AreEqual(new[] { 1, 99, 3 }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableArray_ModifyAll_TransformsAllElements()
    {
        var source = ImmutableArray.Create(1, 2, 3);
        var result = source.ModifyAll(x => x * 10);
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableArray_ModifyWhere_TransformsMatchingOnly()
    {
        var source = ImmutableArray.Create(1, 2, 3, 4, 5);
        var result = source.ModifyWhere(x => x % 2 == 0, x => x * 10);
        CollectionAssert.AreEqual(new[] { 1, 20, 3, 40, 5 }, result.ToArray());
    }

    // ── ImmutableList<T> ────────────────────────────────────────────

    [TestMethod]
    public void ImmutableList_RemoveWhere_RemovesMatchingElements()
    {
        var source = ImmutableList.Create(1, 2, 3, 4, 5);
        var result = source.RemoveWhere(x => x > 3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableList_ReplaceAt_ReplacesElement()
    {
        var source = ImmutableList.Create("a", "b", "c");
        var result = source.ReplaceAt(1, "X");
        CollectionAssert.AreEqual(new[] { "a", "X", "c" }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableList_ModifyAll_TransformsAllElements()
    {
        var source = ImmutableList.Create(1, 2, 3);
        var result = source.ModifyAll(x => x + 100);
        CollectionAssert.AreEqual(new[] { 101, 102, 103 }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableList_ModifyWhere_TransformsMatchingOnly()
    {
        var source = ImmutableList.Create(1, 2, 3, 4);
        var result = source.ModifyWhere(x => x % 2 == 0, x => x * 10);
        CollectionAssert.AreEqual(new[] { 1, 20, 3, 40 }, result.ToArray());
    }

    // ── ImmutableDictionary<TKey, TValue> ───────────────────────────

    [TestMethod]
    public void ImmutableDictionary_AddRange_AddsEntries()
    {
        var source = ImmutableDictionary<string, int>.Empty.Add("a", 1);
        var result = source.AddRange(new Dictionary<string, int> { { "b", 2 }, { "c", 3 } });
        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public void ImmutableDictionary_RemoveWhere_RemovesMatchingEntries()
    {
        var source = ImmutableDictionary<string, int>.Empty.Add("a", 1).Add("b", 2).Add("c", 3);
        var result = source.RemoveWhere(kvp => kvp.Value > 1);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result["a"]);
    }

    [TestMethod]
    public void ImmutableDictionary_ModifyAll_TransformsAllValues()
    {
        var source = ImmutableDictionary<string, int>.Empty.Add("a", 1).Add("b", 2);
        var result = source.ModifyAll((k, v) => v * 10);
        Assert.AreEqual(10, result["a"]);
        Assert.AreEqual(20, result["b"]);
    }

    // ── ImmutableHashSet<T> ─────────────────────────────────────────

    [TestMethod]
    public void ImmutableHashSet_RemoveWhere_RemovesMatchingElements()
    {
        var source = ImmutableHashSet.Create(1, 2, 3, 4, 5);
        var result = source.RemoveWhere(x => x % 2 == 0);
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.Contains(1));
        Assert.IsFalse(result.Contains(2));
    }

    [TestMethod]
    public void ImmutableHashSet_ModifyAll_TransformsElements()
    {
        var source = ImmutableHashSet.Create(1, 2, 3);
        var result = source.ModifyAll(x => x * 10);
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.Contains(10));
        Assert.IsTrue(result.Contains(20));
        Assert.IsTrue(result.Contains(30));
    }

    // ── ImmutableSortedSet<T> ───────────────────────────────────────

    [TestMethod]
    public void ImmutableSortedSet_RemoveWhere_RemovesMatchingElements()
    {
        var source = ImmutableSortedSet.Create(1, 2, 3, 4, 5);
        var result = source.RemoveWhere(x => x > 3);
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.Contains(1));
        Assert.IsFalse(result.Contains(4));
    }

    // ── ImmutableSortedDictionary<TKey, TValue> ─────────────────────

    [TestMethod]
    public void ImmutableSortedDictionary_RemoveWhere_RemovesMatchingEntries()
    {
        var source = ImmutableSortedDictionary<string, int>.Empty.Add("a", 1).Add("b", 2).Add("c", 3);
        var result = source.RemoveWhere(kvp => kvp.Value <= 2);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(3, result["c"]);
    }

    [TestMethod]
    public void ImmutableSortedDictionary_ModifyAll_TransformsAllValues()
    {
        var source = ImmutableSortedDictionary<string, int>.Empty.Add("a", 1).Add("b", 2);
        var result = source.ModifyAll((k, v) => v + 100);
        Assert.AreEqual(101, result["a"]);
        Assert.AreEqual(102, result["b"]);
    }

    // ── ImmutableQueue<T> ───────────────────────────────────────────

    [TestMethod]
    public void ImmutableQueue_EnqueueRange_EnqueuesAll()
    {
        var source = ImmutableQueue<int>.Empty;
        var result = source.EnqueueRange(new[] { 1, 2, 3 });
        Assert.IsFalse(result.IsEmpty);
        result = result.Dequeue(out var first);
        Assert.AreEqual(1, first);
    }

    [TestMethod]
    public void ImmutableQueue_DequeueRange_DequeuesUpToCount()
    {
        var source = ImmutableQueue<int>.Empty.Enqueue(1).Enqueue(2).Enqueue(3);
        var (items, remaining) = source.DequeueRange(2);
        Assert.AreEqual(2, items.Count);
        Assert.AreEqual(1, items[0]);
        Assert.AreEqual(2, items[1]);
        Assert.IsFalse(remaining.IsEmpty);
    }

    // ── ImmutableStack<T> ───────────────────────────────────────────

    [TestMethod]
    public void ImmutableStack_PushRange_PushesAll()
    {
        var source = ImmutableStack<int>.Empty;
        var result = source.PushRange(new[] { 1, 2, 3 });
        Assert.IsFalse(result.IsEmpty);
        result = result.Pop(out var top);
        Assert.AreEqual(3, top);
    }

    [TestMethod]
    public void ImmutableStack_PopRange_PopsUpToCount()
    {
        var source = ImmutableStack<int>.Empty.Push(1).Push(2).Push(3);
        var (items, remaining) = source.PopRange(2);
        Assert.AreEqual(2, items.Count);
        Assert.AreEqual(3, items[0]);
        Assert.AreEqual(2, items[1]);
        Assert.IsFalse(remaining.IsEmpty);
    }
}
