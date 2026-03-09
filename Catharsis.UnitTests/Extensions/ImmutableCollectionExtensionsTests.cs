using System.Collections.Immutable;
using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="ImmutableCollectionExtensions"/> class.
///</summary>
[TestClass]
public class ImmutableCollectionExtensionsTests
{
    #region Public methods
    // ── ImmutableArray<T> ───────────────────────────────────────────
    [TestMethod]
    public void ImmutableArray_InsertAt_InsertsElement()
    {
        ImmutableArray<int> source = ImmutableArray.Create(1, 2, 4);
        ImmutableArray<int> result = source.InsertAt(2, 3);
        Assert.HasCount(4, result);
        Assert.AreEqual(3, result[2]);
    }

    [TestMethod]
    public void ImmutableArray_ModifyAll_TransformsAllElements()
    {
        ImmutableArray<int> source = ImmutableArray.Create(1, 2, 3);
        ImmutableArray<int> result = source.ModifyAll(static x => x * 10);
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableArray_ModifyWhere_TransformsMatchingOnly()
    {
        ImmutableArray<int> source = ImmutableArray.Create(1, 2, 3, 4, 5);
        ImmutableArray<int> result = source.ModifyWhere(static x => x % 2 == 0, static x => x * 10);
        CollectionAssert.AreEqual(new[] { 1, 20, 3, 40, 5 }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableArray_RemoveWhere_RemovesMatchingElements()
    {
        ImmutableArray<int> source = ImmutableArray.Create(1, 2, 3, 4, 5, 6);
        ImmutableArray<int> result = source.RemoveWhere(static x => x % 2 == 0);
        CollectionAssert.AreEqual(new[] { 1, 3, 5 }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableArray_ReplaceAt_ReplacesElement()
    {
        ImmutableArray<int> source = ImmutableArray.Create(1, 2, 3);
        ImmutableArray<int> result = source.ReplaceAt(1, 99);
        CollectionAssert.AreEqual(new[] { 1, 99, 3 }, result.ToArray());
    }

    // ── ImmutableDictionary<TKey, TValue> ───────────────────────────
    [TestMethod]
    public void ImmutableDictionary_AddRange_AddsEntries()
    {
        ImmutableDictionary<string, int> source = ImmutableDictionary<string, int>.Empty.Add("a", 1);
        ImmutableDictionary<string, int> result = source.AddRange(new Dictionary<string, int> { { "b", 2 }, { "c", 3 } });
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void ImmutableDictionary_ModifyAll_TransformsAllValues()
    {
        ImmutableDictionary<string, int> source = ImmutableDictionary<string, int>.Empty.Add("a", 1).Add("b", 2);
        ImmutableDictionary<string, int> result = source.ModifyAll(static (k, v) => v * 10);
        Assert.AreEqual(10, result["a"]);
        Assert.AreEqual(20, result["b"]);
    }

    [TestMethod]
    public void ImmutableDictionary_RemoveWhere_RemovesMatchingEntries()
    {
        ImmutableDictionary<string, int> source = ImmutableDictionary<string, int>.Empty.Add("a", 1).Add("b", 2).Add("c", 3);
        ImmutableDictionary<string, int> result = source.RemoveWhere(static kvp => kvp.Value > 1);
        Assert.HasCount(1, result);
        Assert.AreEqual(1, result["a"]);
    }

    [TestMethod]
    public void ImmutableHashSet_ModifyAll_TransformsElements()
    {
        ImmutableHashSet<int> source = ImmutableHashSet.Create(1, 2, 3);
        ImmutableHashSet<int> result = source.ModifyAll(static x => x * 10);
        Assert.HasCount(3, result);
        Assert.Contains(10, result);
        Assert.Contains(20, result);
        Assert.Contains(30, result);
    }

    // ── ImmutableHashSet<T> ─────────────────────────────────────────
    [TestMethod]
    public void ImmutableHashSet_RemoveWhere_RemovesMatchingElements()
    {
        ImmutableHashSet<int> source = ImmutableHashSet.Create(1, 2, 3, 4, 5);
        ImmutableHashSet<int> result = source.RemoveWhere(static x => x % 2 == 0);
        Assert.HasCount(3, result);
        Assert.Contains(1, result);
        Assert.DoesNotContain(2, result);
    }

    [TestMethod]
    public void ImmutableList_ModifyAll_TransformsAllElements()
    {
        ImmutableList<int> source = ImmutableList.Create(1, 2, 3);
        ImmutableList<int> result = source.ModifyAll(static x => x + 100);
        CollectionAssert.AreEqual(new[] { 101, 102, 103 }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableList_ModifyWhere_TransformsMatchingOnly()
    {
        ImmutableList<int> source = ImmutableList.Create(1, 2, 3, 4);
        ImmutableList<int> result = source.ModifyWhere(static x => x % 2 == 0, static x => x * 10);
        CollectionAssert.AreEqual(new[] { 1, 20, 3, 40 }, result.ToArray());
    }

    // ── ImmutableList<T> ────────────────────────────────────────────
    [TestMethod]
    public void ImmutableList_RemoveWhere_RemovesMatchingElements()
    {
        ImmutableList<int> source = ImmutableList.Create(1, 2, 3, 4, 5);
        ImmutableList<int> result = source.RemoveWhere(static x => x > 3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableList_ReplaceAt_ReplacesElement()
    {
        ImmutableList<string> source = ImmutableList.Create("a", "b", "c");
        ImmutableList<string> result = source.ReplaceAt(1, "X");
        CollectionAssert.AreEqual(new[] { "a", "X", "c" }, result.ToArray());
    }

    [TestMethod]
    public void ImmutableQueue_DequeueRange_DequeuesUpToCount()
    {
        ImmutableQueue<int> source = ImmutableQueue<int>.Empty.Enqueue(1).Enqueue(2).Enqueue(3);
        var (items, remaining) = source.DequeueRange(2);
        Assert.HasCount(2, items);
        Assert.AreEqual(1, items[0]);
        Assert.AreEqual(2, items[1]);
        Assert.IsFalse(remaining.IsEmpty);
    }

    // ── ImmutableQueue<T> ───────────────────────────────────────────
    [TestMethod]
    public void ImmutableQueue_EnqueueRange_EnqueuesAll()
    {
        ImmutableQueue<int> source = ImmutableQueue<int>.Empty;
        ImmutableQueue<int> result = source.EnqueueRange(new[] { 1, 2, 3 });
        Assert.IsFalse(result.IsEmpty);
        result = result.Dequeue(out int first);
        Assert.AreEqual(1, first);
    }

    [TestMethod]
    public void ImmutableSortedDictionary_ModifyAll_TransformsAllValues()
    {
        ImmutableSortedDictionary<string, int> source = ImmutableSortedDictionary<string, int>.Empty.Add("a", 1).Add("b", 2);
        ImmutableSortedDictionary<string, int> result = source.ModifyAll(static (k, v) => v + 100);
        Assert.AreEqual(101, result["a"]);
        Assert.AreEqual(102, result["b"]);
    }

    // ── ImmutableSortedDictionary<TKey, TValue> ─────────────────────
    [TestMethod]
    public void ImmutableSortedDictionary_RemoveWhere_RemovesMatchingEntries()
    {
        ImmutableSortedDictionary<string, int> source = ImmutableSortedDictionary<string, int>.Empty.Add("a", 1).Add("b", 2).Add("c", 3);
        ImmutableSortedDictionary<string, int> result = source.RemoveWhere(static kvp => kvp.Value <= 2);
        Assert.HasCount(1, result);
        Assert.AreEqual(3, result["c"]);
    }

    // ── ImmutableSortedSet<T> ───────────────────────────────────────
    [TestMethod]
    public void ImmutableSortedSet_RemoveWhere_RemovesMatchingElements()
    {
        ImmutableSortedSet<int> source = ImmutableSortedSet.Create(1, 2, 3, 4, 5);
        ImmutableSortedSet<int> result = source.RemoveWhere(static x => x > 3);
        Assert.HasCount(3, result);
        Assert.Contains(1, result);
        Assert.DoesNotContain(4, result);
    }

    [TestMethod]
    public void ImmutableStack_PopRange_PopsUpToCount()
    {
        ImmutableStack<int> source = ImmutableStack<int>.Empty.Push(1).Push(2).Push(3);
        var (items, remaining) = source.PopRange(2);
        Assert.HasCount(2, items);
        Assert.AreEqual(3, items[0]);
        Assert.AreEqual(2, items[1]);
        Assert.IsFalse(remaining.IsEmpty);
    }

    // ── ImmutableStack<T> ───────────────────────────────────────────
    [TestMethod]
    public void ImmutableStack_PushRange_PushesAll()
    {
        ImmutableStack<int> source = ImmutableStack<int>.Empty;
        ImmutableStack<int> result = source.PushRange(new[] { 1, 2, 3 });
        Assert.IsFalse(result.IsEmpty);
        result = result.Pop(out int top);
        Assert.AreEqual(3, top);
    }
    #endregion
}
