using System.Collections.Concurrent;
using Catharsis.Extensions;

namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class ConcurrentCollectionExtensionsTests
{
    [TestMethod]
    public void ConcurrentBag_AddRange_AddsAllItems()
    {
        var source = new ConcurrentBag<int>();
        source.AddRange(new[] { 1, 2, 3 });
        Assert.AreEqual(3, source.Count);
    }

    [TestMethod]
    public void ConcurrentDictionary_AddRange_AddsAndUpdates()
    {
        var source = new ConcurrentDictionary<string, int>();
        source.TryAdd("a", 1);
        source.AddRange(new Dictionary<string, int> { { "a", 99 }, { "b", 2 } });
        Assert.AreEqual(99, source["a"]);
        Assert.AreEqual(2, source["b"]);
    }

    [TestMethod]
    public void ConcurrentDictionary_RemoveRange_RemovesMatchingKeys()
    {
        var source = new ConcurrentDictionary<string, int>();
        source.TryAdd("a", 1);
        source.TryAdd("b", 2);
        source.TryAdd("c", 3);
        int removed = source.RemoveRange(new[] { "a", "c", "z" });
        Assert.AreEqual(2, removed);
        Assert.AreEqual(1, source.Count);
    }

    [TestMethod]
    public void ConcurrentDictionary_RemoveWhere_RemovesMatchingEntries()
    {
        var source = new ConcurrentDictionary<string, int>();
        source.TryAdd("a", 1);
        source.TryAdd("b", 2);
        source.TryAdd("c", 3);
        int removed = source.RemoveWhere(kvp => kvp.Value > 1);
        Assert.AreEqual(2, removed);
        Assert.AreEqual(1, source.Count);
    }

    [TestMethod]
    public void ConcurrentDictionary_ModifyAll_TransformsAllValues()
    {
        var source = new ConcurrentDictionary<string, int>();
        source.TryAdd("a", 1);
        source.TryAdd("b", 2);
        source.ModifyAll((k, v) => v * 10);
        Assert.AreEqual(10, source["a"]);
        Assert.AreEqual(20, source["b"]);
    }

    [TestMethod]
    public void ConcurrentQueue_EnqueueRange_EnqueuesAll()
    {
        var source = new ConcurrentQueue<int>();
        source.EnqueueRange(new[] { 1, 2, 3 });
        Assert.AreEqual(3, source.Count);
        source.TryDequeue(out var first);
        Assert.AreEqual(1, first);
    }

    [TestMethod]
    public void ConcurrentQueue_DequeueRange_DequeuesUpToCount()
    {
        var source = new ConcurrentQueue<int>();
        source.EnqueueRange(new[] { 1, 2, 3, 4, 5 });
        var result = source.DequeueRange(3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
        Assert.AreEqual(2, source.Count);
    }

    [TestMethod]
    public void ConcurrentQueue_DequeueRange_MoreThanAvailable_ReturnsAll()
    {
        var source = new ConcurrentQueue<int>();
        source.EnqueueRange(new[] { 1, 2 });
        var result = source.DequeueRange(10);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result);
        Assert.AreEqual(0, source.Count);
    }

    [TestMethod]
    public void ConcurrentStack_PushRange_PushesAll()
    {
        var source = new ConcurrentStack<int>();
        source.PushRange(new[] { 1, 2, 3 });
        Assert.AreEqual(3, source.Count);
    }

    [TestMethod]
    public void ConcurrentStack_PopRange_PopsUpToCount()
    {
        var source = new ConcurrentStack<int>();
        source.PushRange(new[] { 1, 2, 3, 4, 5 });
        var result = source.PopRange(3);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(2, source.Count);
    }

    [TestMethod]
    public void BlockingCollection_AddRange_AddsAllItems()
    {
        var source = new BlockingCollection<int>();
        source.AddRange(new[] { 1, 2, 3 });
        Assert.AreEqual(3, source.Count);
    }

    [TestMethod]
    public void BlockingCollection_TakeRange_TakesUpToCount()
    {
        var source = new BlockingCollection<int>();
        source.AddRange(new[] { 1, 2, 3, 4, 5 });
        var result = source.TakeRange(3);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(2, source.Count);
    }
}
