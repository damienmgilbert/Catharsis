namespace Catharsis.Collections.UnitTests;

[TestClass]
public class CircularBufferTests
{
    [TestMethod]
    public void Constructor_ValidCapacity_CreatesBuffer()
    {
        var buffer = new CircularBuffer<int>(5);
        Assert.AreEqual(5, buffer.Capacity);
        Assert.AreEqual(0, buffer.Count);
        Assert.IsFalse(buffer.IsFull);
    }

    [TestMethod]
    public void Constructor_ZeroCapacity_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CircularBuffer<int>(0));
    }

    [TestMethod]
    public void Add_BelowCapacity_IncreasesCount()
    {
        var buffer = new CircularBuffer<int>(3);
        buffer.Add(1);
        buffer.Add(2);
        Assert.AreEqual(2, buffer.Count);
        Assert.IsFalse(buffer.IsFull);
    }

    [TestMethod]
    public void Add_AtCapacity_OverwritesOldest()
    {
        var buffer = new CircularBuffer<int>(3);
        buffer.Add(1); buffer.Add(2); buffer.Add(3);
        Assert.IsTrue(buffer.IsFull);
        buffer.Add(4);
        Assert.AreEqual(3, buffer.Count);
        Assert.AreEqual(2, buffer.Peek());
    }

    [TestMethod]
    public void Peek_ReturnsOldestItem()
    {
        var buffer = new CircularBuffer<int>(3);
        buffer.Add(10); buffer.Add(20);
        Assert.AreEqual(10, buffer.Peek());
    }

    [TestMethod]
    public void Peek_EmptyBuffer_Throws()
    {
        var buffer = new CircularBuffer<int>(3);
        Assert.ThrowsExactly<InvalidOperationException>(() => buffer.Peek());
    }

    [TestMethod]
    public void Remove_ReturnsAndRemovesOldest()
    {
        var buffer = new CircularBuffer<int>(3);
        buffer.Add(1); buffer.Add(2); buffer.Add(3);
        Assert.AreEqual(1, buffer.Remove());
        Assert.AreEqual(2, buffer.Count);
        Assert.AreEqual(2, buffer.Peek());
    }

    [TestMethod]
    public void Remove_EmptyBuffer_Throws()
    {
        var buffer = new CircularBuffer<int>(3);
        Assert.ThrowsExactly<InvalidOperationException>(() => buffer.Remove());
    }

    [TestMethod]
    public void Clear_ResetsBuffer()
    {
        var buffer = new CircularBuffer<int>(3);
        buffer.Add(1); buffer.Add(2);
        buffer.Clear();
        Assert.AreEqual(0, buffer.Count);
        Assert.IsFalse(buffer.IsFull);
    }

    [TestMethod]
    public void Indexer_ReturnsCorrectLogicalOrder()
    {
        var buffer = new CircularBuffer<int>(3);
        buffer.Add(10); buffer.Add(20); buffer.Add(30);
        Assert.AreEqual(10, buffer[0]);
        Assert.AreEqual(20, buffer[1]);
        Assert.AreEqual(30, buffer[2]);
    }

    [TestMethod]
    public void Indexer_OutOfRange_Throws()
    {
        var buffer = new CircularBuffer<int>(3);
        buffer.Add(1);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = buffer[5]);
    }

    [TestMethod]
    public void Enumeration_ReturnsItemsInOrder()
    {
        var buffer = new CircularBuffer<int>(3);
        buffer.Add(1); buffer.Add(2); buffer.Add(3); buffer.Add(4);
        CollectionAssert.AreEqual(new[] { 2, 3, 4 }, buffer.ToList());
    }
}

[TestClass]
public class BoundedCollectionTests
{
    [TestMethod]
    public void Constructor_ValidCapacity_Creates()
    {
        var c = new BoundedCollection<int>(5);
        Assert.AreEqual(5, c.MaxCapacity);
        Assert.AreEqual(0, c.Count);
    }

    [TestMethod]
    public void Constructor_ZeroCapacity_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BoundedCollection<int>(0));
    }

    [TestMethod]
    public void Add_BelowCapacity_Succeeds()
    {
        var c = new BoundedCollection<int>(2);
        c.Add(1);
        Assert.AreEqual(1, c.Count);
        Assert.IsFalse(c.IsFull);
    }

    [TestMethod]
    public void Add_AtCapacity_Throws()
    {
        var c = new BoundedCollection<int>(1);
        c.Add(1);
        Assert.ThrowsExactly<InvalidOperationException>(() => c.Add(2));
    }

    [TestMethod]
    public void TryAdd_AtCapacity_ReturnsFalse()
    {
        var c = new BoundedCollection<int>(1);
        c.Add(1);
        Assert.IsFalse(c.TryAdd(2));
    }

    [TestMethod]
    public void TryAdd_BelowCapacity_ReturnsTrue()
    {
        var c = new BoundedCollection<int>(2);
        Assert.IsTrue(c.TryAdd(1));
        Assert.AreEqual(1, c.Count);
    }

    [TestMethod]
    public void Remove_ExistingItem_ReturnsTrue()
    {
        var c = new BoundedCollection<int>(3);
        c.Add(1);
        Assert.IsTrue(c.Remove(1));
        Assert.AreEqual(0, c.Count);
    }

    [TestMethod]
    public void Contains_ExistingItem_ReturnsTrue()
    {
        var c = new BoundedCollection<string>(3);
        c.Add("hello");
        Assert.IsTrue(c.Contains("hello"));
        Assert.IsFalse(c.Contains("world"));
    }

    [TestMethod]
    public void Clear_ResetsCollection()
    {
        var c = new BoundedCollection<int>(3);
        c.Add(1); c.Add(2);
        c.Clear();
        Assert.AreEqual(0, c.Count);
        Assert.IsFalse(c.IsFull);
    }
}

[TestClass]
public class OrderedSetTests
{
    [TestMethod]
    public void TryAdd_UniqueItems_ReturnsTrue()
    {
        var set = new OrderedSet<int>();
        Assert.IsTrue(set.TryAdd(1));
        Assert.IsTrue(set.TryAdd(2));
        Assert.AreEqual(2, set.Count);
    }

    [TestMethod]
    public void TryAdd_Duplicate_ReturnsFalse()
    {
        var set = new OrderedSet<int>();
        set.TryAdd(1);
        Assert.IsFalse(set.TryAdd(1));
        Assert.AreEqual(1, set.Count);
    }

    [TestMethod]
    public void Indexer_ReturnsInsertionOrder()
    {
        var set = new OrderedSet<string>();
        set.TryAdd("b"); set.TryAdd("a"); set.TryAdd("c");
        Assert.AreEqual("b", set[0]);
        Assert.AreEqual("a", set[1]);
        Assert.AreEqual("c", set[2]);
    }

    [TestMethod]
    public void Remove_ExistingItem_ReturnsTrue()
    {
        var set = new OrderedSet<int>();
        set.TryAdd(1); set.TryAdd(2);
        Assert.IsTrue(set.Remove(1));
        Assert.AreEqual(1, set.Count);
        Assert.IsFalse(set.Contains(1));
    }

    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        var set = new OrderedSet<int>();
        set.TryAdd(42);
        Assert.IsTrue(set.Contains(42));
        Assert.IsFalse(set.Contains(99));
    }

    [TestMethod]
    public void Clear_RemovesAllItems()
    {
        var set = new OrderedSet<int>();
        set.TryAdd(1); set.TryAdd(2);
        set.Clear();
        Assert.AreEqual(0, set.Count);
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new OrderedSet<int>(null!));
    }
}

[TestClass]
public class DequeTests
{
    [TestMethod]
    public void Constructor_Default_CreatesEmptyDeque()
    {
        var d = new Deque<int>();
        Assert.AreEqual(0, d.Count);
        Assert.IsTrue(d.IsEmpty);
    }

    [TestMethod]
    public void Constructor_NegativeCapacity_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Deque<int>(-1));
    }

    [TestMethod]
    public void AddFirst_AddsToFront()
    {
        var d = new Deque<int>();
        d.AddFirst(1); d.AddFirst(2);
        Assert.AreEqual(2, d.PeekFirst());
        Assert.AreEqual(1, d.PeekLast());
    }

    [TestMethod]
    public void AddLast_AddsToBack()
    {
        var d = new Deque<int>();
        d.AddLast(1); d.AddLast(2);
        Assert.AreEqual(1, d.PeekFirst());
        Assert.AreEqual(2, d.PeekLast());
    }

    [TestMethod]
    public void RemoveFirst_RemovesFromFront()
    {
        var d = new Deque<int>();
        d.AddLast(1); d.AddLast(2); d.AddLast(3);
        Assert.AreEqual(1, d.RemoveFirst());
        Assert.AreEqual(2, d.Count);
    }

    [TestMethod]
    public void RemoveLast_RemovesFromBack()
    {
        var d = new Deque<int>();
        d.AddLast(1); d.AddLast(2); d.AddLast(3);
        Assert.AreEqual(3, d.RemoveLast());
        Assert.AreEqual(2, d.Count);
    }

    [TestMethod]
    public void RemoveFirst_EmptyDeque_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => new Deque<int>().RemoveFirst());
    }

    [TestMethod]
    public void RemoveLast_EmptyDeque_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => new Deque<int>().RemoveLast());
    }

    [TestMethod]
    public void PeekFirst_EmptyDeque_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => new Deque<int>().PeekFirst());
    }

    [TestMethod]
    public void PeekLast_EmptyDeque_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => new Deque<int>().PeekLast());
    }

    [TestMethod]
    public void Indexer_ReturnsCorrectLogicalOrder()
    {
        var d = new Deque<int>();
        d.AddLast(10); d.AddLast(20); d.AddLast(30);
        Assert.AreEqual(10, d[0]);
        Assert.AreEqual(30, d[2]);
    }

    [TestMethod]
    public void Clear_ResetsDeque()
    {
        var d = new Deque<int>();
        d.AddLast(1); d.AddLast(2);
        d.Clear();
        Assert.AreEqual(0, d.Count);
        Assert.IsTrue(d.IsEmpty);
    }

    [TestMethod]
    public void AddFirst_GrowsCapacityWhenFull()
    {
        var d = new Deque<int>(1);
        d.AddFirst(1); d.AddFirst(2); d.AddFirst(3);
        Assert.AreEqual(3, d.Count);
        Assert.AreEqual(3, d.PeekFirst());
    }

    [TestMethod]
    public void Enumeration_ReturnsItemsFrontToBack()
    {
        var d = new Deque<int>();
        d.AddLast(1); d.AddLast(2); d.AddLast(3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, d.ToList());
    }
}

[TestClass]
public class EventCollectionTests
{
    [TestMethod]
    public void Add_RaisesItemAddedEvent()
    {
        var c = new EventCollection<int>();
        int raised = -1;
        c.ItemAdded += (_, item) => raised = item;

        c.Add(42);

        Assert.AreEqual(42, raised);
        Assert.AreEqual(1, c.Count);
    }

    [TestMethod]
    public void Remove_ExistingItem_RaisesItemRemovedEvent()
    {
        var c = new EventCollection<int>();
        c.Add(1);
        int raised = -1;
        c.ItemRemoved += (_, item) => raised = item;

        Assert.IsTrue(c.Remove(1));
        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void Remove_NonexistentItem_ReturnsFalse()
    {
        var c = new EventCollection<int>();
        bool eventFired = false;
        c.ItemRemoved += (_, _) => eventFired = true;

        Assert.IsFalse(c.Remove(99));
        Assert.IsFalse(eventFired);
    }

    [TestMethod]
    public void Clear_RaisesClearedEvent()
    {
        var c = new EventCollection<int>();
        c.Add(1);
        bool cleared = false;
        c.Cleared += (_, _) => cleared = true;

        c.Clear();

        Assert.IsTrue(cleared);
        Assert.AreEqual(0, c.Count);
    }

    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        var c = new EventCollection<string>();
        c.Add("test");
        Assert.IsTrue(c.Contains("test"));
        Assert.IsFalse(c.Contains("other"));
    }
}

[TestClass]
public class HistoryStackTests
{
    [TestMethod]
    public void Push_AddsItemToStack()
    {
        var s = new HistoryStack<int>();
        s.Push(1);
        Assert.AreEqual(1, s.Count);
        Assert.AreEqual(1, s.Peek());
    }

    [TestMethod]
    public void Peek_EmptyStack_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => new HistoryStack<int>().Peek());
    }

    [TestMethod]
    public void Undo_MovesItemToRedoStack()
    {
        var s = new HistoryStack<int>();
        s.Push(1); s.Push(2);

        int undone = s.Undo();

        Assert.AreEqual(2, undone);
        Assert.AreEqual(1, s.Count);
        Assert.IsTrue(s.CanRedo);
    }

    [TestMethod]
    public void Undo_EmptyStack_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => new HistoryStack<int>().Undo());
    }

    [TestMethod]
    public void Redo_RestoresUndoneItem()
    {
        var s = new HistoryStack<int>();
        s.Push(1); s.Push(2);
        s.Undo();

        int redone = s.Redo();

        Assert.AreEqual(2, redone);
        Assert.AreEqual(2, s.Count);
        Assert.IsFalse(s.CanRedo);
    }

    [TestMethod]
    public void Redo_NothingToRedo_Throws()
    {
        var s = new HistoryStack<int>();
        s.Push(1);
        Assert.ThrowsExactly<InvalidOperationException>(() => s.Redo());
    }

    [TestMethod]
    public void Push_ClearsRedoHistory()
    {
        var s = new HistoryStack<int>();
        s.Push(1); s.Push(2);
        s.Undo();
        Assert.IsTrue(s.CanRedo);

        s.Push(3);

        Assert.IsFalse(s.CanRedo);
    }

    [TestMethod]
    public void Clear_ResetsAll()
    {
        var s = new HistoryStack<int>();
        s.Push(1); s.Push(2);
        s.Undo();
        s.Clear();
        Assert.AreEqual(0, s.Count);
        Assert.IsFalse(s.CanUndo);
        Assert.IsFalse(s.CanRedo);
    }
}

[TestClass]
public class TrackingCollectionTests
{
    [TestMethod]
    public void Add_InvokesCallback()
    {
        int tracked = -1;
        var c = new TrackingCollection<int>(x => tracked = x);

        c.Add(42);

        Assert.AreEqual(42, tracked);
        Assert.AreEqual(1, c.Count);
    }

    [TestMethod]
    public void Remove_RemovesItem()
    {
        var c = new TrackingCollection<int>(_ => { });
        c.Add(1); c.Add(2);

        Assert.IsTrue(c.Remove(1));
        Assert.AreEqual(1, c.Count);
    }

    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        var c = new TrackingCollection<string>(_ => { });
        c.Add("hello");
        Assert.IsTrue(c.Contains("hello"));
        Assert.IsFalse(c.Contains("world"));
    }

    [TestMethod]
    public void Clear_RemovesAllItems()
    {
        var c = new TrackingCollection<int>(_ => { });
        c.Add(1); c.Add(2);
        c.Clear();
        Assert.AreEqual(0, c.Count);
    }
}
