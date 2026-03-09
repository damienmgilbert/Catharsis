using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="QueueStackExtensions"/> class.
///</summary>
[TestClass]
public class QueueStackExtensionsTests
{
    #region Public methods
    [TestMethod]
    public void PriorityQueue_DequeueRange_DequeuesUpToCount()
    {
        PriorityQueue<string, int> source = new PriorityQueue<string, int>();
        source.Enqueue("c", 3);
        source.Enqueue("a", 1);
        source.Enqueue("b", 2);
        List<string> result = source.DequeueRange(2);
        Assert.HasCount(2, result);
        Assert.AreEqual("a", result[0]);
        Assert.AreEqual("b", result[1]);
        Assert.AreEqual(1, source.Count);
    }

    // ── PriorityQueue<TElement, TPriority> ──────────────────────────
    [TestMethod]
    public void PriorityQueue_EnqueueRange_EnqueuesAll()
    {
        PriorityQueue<string, int> source = new PriorityQueue<string, int>();
        source.EnqueueRange(new[] { ("c", 3), ("a", 1), ("b", 2) });
        Assert.AreEqual(3, source.Count);
        Assert.AreEqual("a", source.Dequeue());
    }

    [TestMethod]
    public void Queue_DequeueRange_DequeuesUpToCount()
    {
        Queue<int> source = new Queue<int>(new[] { 1, 2, 3, 4, 5 });
        List<int> result = source.DequeueRange(3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
        Assert.HasCount(2, source);
    }

    [TestMethod]
    public void Queue_DequeueRange_MoreThanAvailable_ReturnsAll()
    {
        Queue<int> source = new Queue<int>(new[] { 1, 2 });
        List<int> result = source.DequeueRange(10);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result);
        Assert.IsEmpty(source);
    }

    [TestMethod]
    public void Queue_DequeueRange_ZeroCount_ReturnsEmpty()
    {
        Queue<int> source = new Queue<int>(new[] { 1, 2, 3 });
        List<int> result = source.DequeueRange(0);
        Assert.IsEmpty(result);
        Assert.HasCount(3, source);
    }

    // ── Queue<T> ────────────────────────────────────────────────────
    [TestMethod]
    public void Queue_EnqueueRange_EnqueuesAllInOrder()
    {
        Queue<int> source = new Queue<int>();
        source.EnqueueRange(new[] { 1, 2, 3 });
        Assert.HasCount(3, source);
        Assert.AreEqual(1, source.Dequeue());
        Assert.AreEqual(2, source.Dequeue());
        Assert.AreEqual(3, source.Dequeue());
    }

    // ── Null source guards ──────────────────────────────────────────
    [TestMethod]
    public void Queue_NullSource_ThrowsArgumentNullException()
    {
        Queue<int>? source = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.EnqueueRange(new[] { 1 }));
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.DequeueRange(1));
    }

    // ── SortedDictionary<TKey, TValue> ──────────────────────────────
    [TestMethod]
    public void SortedDictionary_AddRange_AddsEntries()
    {
        SortedDictionary<string, int> source = new SortedDictionary<string, int> { { "a", 1 } };
        source.AddRange(new Dictionary<string, int> { { "b", 2 }, { "c", 3 } });
        Assert.HasCount(3, source);
    }

    [TestMethod]
    public void SortedDictionary_ModifyAll_TransformsAllValues()
    {
        SortedDictionary<string, int> source = new SortedDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        source.ModifyAll(static (k, v) => v + 100);
        Assert.AreEqual(101, source["a"]);
        Assert.AreEqual(102, source["b"]);
    }

    [TestMethod]
    public void SortedDictionary_RemoveRange_RemovesMatchingKeys()
    {
        SortedDictionary<string, int> source = new SortedDictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        int removed = source.RemoveRange(new[] { "a", "c" });
        Assert.AreEqual(2, removed);
        Assert.HasCount(1, source);
    }

    [TestMethod]
    public void SortedDictionary_RemoveWhere_RemovesMatchingEntries()
    {
        SortedDictionary<string, int> source = new SortedDictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        int removed = source.RemoveWhere(static kvp => kvp.Value <= 2);
        Assert.AreEqual(2, removed);
        Assert.HasCount(1, source);
    }

    // ── SortedList<TKey, TValue> ────────────────────────────────────
    [TestMethod]
    public void SortedList_AddRange_AddsEntries()
    {
        SortedList<string, int> source = new SortedList<string, int> { { "a", 1 } };
        source.AddRange(new Dictionary<string, int> { { "b", 2 }, { "c", 3 } });
        Assert.HasCount(3, source);
        Assert.AreEqual(2, source["b"]);
    }

    [TestMethod]
    public void SortedList_ModifyAll_TransformsAllValues()
    {
        SortedList<string, int> source = new SortedList<string, int> { { "a", 1 }, { "b", 2 } };
        source.ModifyAll(static (k, v) => v * 10);
        Assert.AreEqual(10, source["a"]);
        Assert.AreEqual(20, source["b"]);
    }

    [TestMethod]
    public void SortedList_RemoveRange_RemovesMatchingKeys()
    {
        SortedList<string, int> source = new SortedList<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        int removed = source.RemoveRange(new[] { "a", "c", "z" });
        Assert.AreEqual(2, removed);
        Assert.HasCount(1, source);
    }

    [TestMethod]
    public void SortedList_RemoveWhere_RemovesMatchingEntries()
    {
        SortedList<string, int> source = new SortedList<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        int removed = source.RemoveWhere(static kvp => kvp.Value > 1);
        Assert.AreEqual(2, removed);
        Assert.HasCount(1, source);
    }

    [TestMethod]
    public void Stack_NullSource_ThrowsArgumentNullException()
    {
        Stack<int>? source = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.PushRange(new[] { 1 }));
        Assert.ThrowsExactly<ArgumentNullException>(() => source!.PopRange(1));
    }

    [TestMethod]
    public void Stack_PopRange_MoreThanAvailable_ReturnsAll()
    {
        Stack<int> source = new Stack<int>(new[] { 2, 1 });
        List<int> result = source.PopRange(10);
        Assert.HasCount(2, result);
        Assert.IsEmpty(source);
    }

    [TestMethod]
    public void Stack_PopRange_PopsUpToCount()
    {
        Stack<int> source = new Stack<int>(new[] { 5, 4, 3, 2, 1 });
        List<int> result = source.PopRange(3);
        Assert.HasCount(3, result);
        Assert.HasCount(2, source);
    }

    // ── Stack<T> ────────────────────────────────────────────────────
    [TestMethod]
    public void Stack_PushRange_PushesAllInOrder()
    {
        Stack<int> source = new Stack<int>();
        source.PushRange(new[] { 1, 2, 3 });
        Assert.HasCount(3, source);
        Assert.AreEqual(3, source.Pop());
        Assert.AreEqual(2, source.Pop());
        Assert.AreEqual(1, source.Pop());
    }
    #endregion
}
