using System.Collections.Concurrent;
using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

[TestClass]
public class ConcurrentCollectionExtensionsTests
{
    #region Public methods
    [TestMethod]
    public void BlockingCollection_AddRange_AddsAllItems()
    {
        BlockingCollection<int> source = new BlockingCollection<int>();
        source.AddRange(new[] { 1, 2, 3 });
        Assert.AreEqual(3, source.Count);
    }

    [TestMethod]
    public void BlockingCollection_TakeRange_TakesUpToCount()
    {
        BlockingCollection<int> source = new BlockingCollection<int>();
        source.AddRange(new[] { 1, 2, 3, 4, 5 });
        List<int> result = source.TakeRange(3);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(2, source.Count);
    }

    [TestMethod]
    public void ConcurrentBag_AddRange_AddsAllItems()
    {
        ConcurrentBag<int> source = new ConcurrentBag<int>();
        source.AddRange(new[] { 1, 2, 3 });
        Assert.AreEqual(3, source.Count);
    }

    [TestMethod]
    public void ConcurrentDictionary_AddRange_AddsAndUpdates()
    {
        ConcurrentDictionary<string, int> source = new ConcurrentDictionary<string, int>();
        source.TryAdd("a", 1);
        source.AddRange(new Dictionary<string, int> { { "a", 99 }, { "b", 2 } });
        Assert.AreEqual(99, source["a"]);
        Assert.AreEqual(2, source["b"]);
    }

    [TestMethod]
    public void ConcurrentDictionary_ModifyAll_TransformsAllValues()
    {
        ConcurrentDictionary<string, int> source = new ConcurrentDictionary<string, int>();
        source.TryAdd("a", 1);
        source.TryAdd("b", 2);
        source.ModifyAll((k, v) => v * 10);
        Assert.AreEqual(10, source["a"]);
        Assert.AreEqual(20, source["b"]);
    }

    [TestMethod]
    public void ConcurrentDictionary_RemoveRange_RemovesMatchingKeys()
    {
        ConcurrentDictionary<string, int> source = new ConcurrentDictionary<string, int>();
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
        ConcurrentDictionary<string, int> source = new ConcurrentDictionary<string, int>();
        source.TryAdd("a", 1);
        source.TryAdd("b", 2);
        source.TryAdd("c", 3);
        int removed = source.RemoveWhere(kvp => kvp.Value > 1);
        Assert.AreEqual(2, removed);
        Assert.AreEqual(1, source.Count);
    }

    [TestMethod]
    public void ConcurrentQueue_DequeueRange_DequeuesUpToCount()
    {
        ConcurrentQueue<int> source = new ConcurrentQueue<int>();
        source.EnqueueRange(new[] { 1, 2, 3, 4, 5 });
        List<int> result = source.DequeueRange(3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
        Assert.AreEqual(2, source.Count);
    }

    [TestMethod]
    public void ConcurrentQueue_DequeueRange_MoreThanAvailable_ReturnsAll()
    {
        ConcurrentQueue<int> source = new ConcurrentQueue<int>();
        source.EnqueueRange(new[] { 1, 2 });
        List<int> result = source.DequeueRange(10);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result);
        Assert.AreEqual(0, source.Count);
    }

    [TestMethod]
    public void ConcurrentQueue_EnqueueRange_EnqueuesAll()
    {
        ConcurrentQueue<int> source = new ConcurrentQueue<int>();
        source.EnqueueRange(new[] { 1, 2, 3 });
        Assert.AreEqual(3, source.Count);
        source.TryDequeue(out int first);
        Assert.AreEqual(1, first);
    }

    [TestMethod]
    public void ConcurrentStack_PopRange_PopsUpToCount()
    {
        ConcurrentStack<int> source = new ConcurrentStack<int>();
        source.PushRange(new[] { 1, 2, 3, 4, 5 });
        List<int> result = source.PopRange(3);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(2, source.Count);
    }

    [TestMethod]
    public void ConcurrentStack_PushRange_PushesAll()
    {
        ConcurrentStack<int> source = new ConcurrentStack<int>();
        source.PushRange(new[] { 1, 2, 3 });
        Assert.AreEqual(3, source.Count);
    }
    #endregion
}
