namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class QueueStackExtensionsTests
{
    // ── Queue<T> ────────────────────────────────────────────────────

    [TestMethod]
    public void Queue_EnqueueRange_EnqueuesAllInOrder()
    {
        var source = new Queue<int>();
        source.EnqueueRange(new[] { 1, 2, 3 });
        Assert.AreEqual(3, source.Count);
        Assert.AreEqual(1, source.Dequeue());
        Assert.AreEqual(2, source.Dequeue());
        Assert.AreEqual(3, source.Dequeue());
    }

    [TestMethod]
    public void Queue_DequeueRange_DequeuesUpToCount()
    {
        var source = new Queue<int>(new[] { 1, 2, 3, 4, 5 });
        var result = source.DequeueRange(3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
        Assert.AreEqual(2, source.Count);
    }

    [TestMethod]
    public void Queue_DequeueRange_MoreThanAvailable_ReturnsAll()
    {
        var source = new Queue<int>(new[] { 1, 2 });
        var result = source.DequeueRange(10);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result);
        Assert.AreEqual(0, source.Count);
    }

    [TestMethod]
    public void Queue_DequeueRange_ZeroCount_ReturnsEmpty()
    {
        var source = new Queue<int>(new[] { 1, 2, 3 });
        var result = source.DequeueRange(0);
        Assert.AreEqual(0, result.Count);
        Assert.AreEqual(3, source.Count);
    }

    // ── Stack<T> ────────────────────────────────────────────────────

    [TestMethod]
    public void Stack_PushRange_PushesAllInOrder()
    {
        var source = new Stack<int>();
        source.PushRange(new[] { 1, 2, 3 });
        Assert.AreEqual(3, source.Count);
        Assert.AreEqual(3, source.Pop());
        Assert.AreEqual(2, source.Pop());
        Assert.AreEqual(1, source.Pop());
    }

    [TestMethod]
    public void Stack_PopRange_PopsUpToCount()
    {
        var source = new Stack<int>(new[] { 5, 4, 3, 2, 1 });
        var result = source.PopRange(3);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(2, source.Count);
    }

    [TestMethod]
    public void Stack_PopRange_MoreThanAvailable_ReturnsAll()
    {
        var source = new Stack<int>(new[] { 2, 1 });
        var result = source.PopRange(10);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(0, source.Count);
    }

    // ── PriorityQueue<TElement, TPriority> ──────────────────────────

    [TestMethod]
    public void PriorityQueue_EnqueueRange_EnqueuesAll()
    {
        var source = new PriorityQueue<string, int>();
        source.EnqueueRange(new[] { ("c", 3), ("a", 1), ("b", 2) });
        Assert.AreEqual(3, source.Count);
        Assert.AreEqual("a", source.Dequeue());
    }

    [TestMethod]
    public void PriorityQueue_DequeueRange_DequeuesUpToCount()
    {
        var source = new PriorityQueue<string, int>();
        source.Enqueue("c", 3);
        source.Enqueue("a", 1);
        source.Enqueue("b", 2);
        var result = source.DequeueRange(2);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("a", result[0]);
        Assert.AreEqual("b", result[1]);
        Assert.AreEqual(1, source.Count);
    }

    // ── SortedList<TKey, TValue> ────────────────────────────────────

    [TestMethod]
    public void SortedList_AddRange_AddsEntries()
    {
        var source = new SortedList<string, int> { { "a", 1 } };
        source.AddRange(new Dictionary<string, int> { { "b", 2 }, { "c", 3 } });
        Assert.AreEqual(3, source.Count);
        Assert.AreEqual(2, source["b"]);
    }

    [TestMethod]
    public void SortedList_RemoveRange_RemovesMatchingKeys()
    {
        var source = new SortedList<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        int removed = source.RemoveRange(new[] { "a", "c", "z" });
        Assert.AreEqual(2, removed);
        Assert.AreEqual(1, source.Count);
    }

    [TestMethod]
    public void SortedList_RemoveWhere_RemovesMatchingEntries()
    {
        var source = new SortedList<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        int removed = source.RemoveWhere(kvp => kvp.Value > 1);
        Assert.AreEqual(2, removed);
        Assert.AreEqual(1, source.Count);
    }

    [TestMethod]
    public void SortedList_ModifyAll_TransformsAllValues()
    {
        var source = new SortedList<string, int> { { "a", 1 }, { "b", 2 } };
        source.ModifyAll((k, v) => v * 10);
        Assert.AreEqual(10, source["a"]);
        Assert.AreEqual(20, source["b"]);
    }

    // ── SortedDictionary<TKey, TValue> ──────────────────────────────

    [TestMethod]
    public void SortedDictionary_AddRange_AddsEntries()
    {
        var source = new SortedDictionary<string, int> { { "a", 1 } };
        source.AddRange(new Dictionary<string, int> { { "b", 2 }, { "c", 3 } });
        Assert.AreEqual(3, source.Count);
    }

    [TestMethod]
    public void SortedDictionary_RemoveRange_RemovesMatchingKeys()
    {
        var source = new SortedDictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        int removed = source.RemoveRange(new[] { "a", "c" });
        Assert.AreEqual(2, removed);
        Assert.AreEqual(1, source.Count);
    }

    [TestMethod]
    public void SortedDictionary_RemoveWhere_RemovesMatchingEntries()
    {
        var source = new SortedDictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        int removed = source.RemoveWhere(kvp => kvp.Value <= 2);
        Assert.AreEqual(2, removed);
        Assert.AreEqual(1, source.Count);
    }

    [TestMethod]
    public void SortedDictionary_ModifyAll_TransformsAllValues()
    {
        var source = new SortedDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        source.ModifyAll((k, v) => v + 100);
        Assert.AreEqual(101, source["a"]);
        Assert.AreEqual(102, source["b"]);
    }

    // ── Null source guards ──────────────────────────────────────────

    [TestMethod]
    public void Queue_NullSource_ThrowsArgumentNullException()
    {
        Queue<int>? source = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.EnqueueRange(new[] { 1 }));
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.DequeueRange(1));
    }

    [TestMethod]
    public void Stack_NullSource_ThrowsArgumentNullException()
    {
        Stack<int>? source = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.PushRange(new[] { 1 }));
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.PopRange(1));
    }
}
