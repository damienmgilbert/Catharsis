using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Collections.Immutable;

namespace Catharsis.Immutable.UnitTests;

[TestClass]
public class ImmutableBufferTests
{
    [TestMethod]
    public void Constructor_FromSpan_CopiesData()
    {
        ReadOnlySpan<int> data = [1, 2, 3];
        var buf = new ImmutableBuffer<int>(data);
        Assert.AreEqual(3, buf.Count);
        Assert.AreEqual(1, buf[0]);
        Assert.AreEqual(3, buf[2]);
    }

    [TestMethod]
    public void Constructor_FromImmutableArray()
    {
        var arr = ImmutableArray.Create(10, 20);
        var buf = new ImmutableBuffer<int>(arr);
        Assert.AreEqual(2, buf.Count);
    }

    [TestMethod]
    public void Empty_ReturnsEmptyBuffer()
    {
        var buf = ImmutableBuffer<int>.Empty;
        Assert.AreEqual(0, buf.Count);
        Assert.IsTrue(buf.IsEmpty);
    }

    [TestMethod]
    public void Slice_ReturnsSubset()
    {
        var buf = ImmutableBuffer<int>.Create([10, 20, 30, 40, 50]);
        var sliced = buf.Slice(1, 3);
        Assert.AreEqual(3, sliced.Count);
        Assert.AreEqual(20, sliced[0]);
        Assert.AreEqual(40, sliced[2]);
    }

    [TestMethod]
    public void Equals_SameData_ReturnsTrue()
    {
        var a = ImmutableBuffer<int>.Create([1, 2, 3]);
        var b = ImmutableBuffer<int>.Create([1, 2, 3]);
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void Equals_DifferentData_ReturnsFalse()
    {
        var a = ImmutableBuffer<int>.Create([1, 2]);
        var b = ImmutableBuffer<int>.Create([1, 3]);
        Assert.IsFalse(a.Equals(b));
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Span_ReturnsReadOnlySpan()
    {
        var buf = ImmutableBuffer<int>.Create([5, 10]);
        var span = buf.Span;
        Assert.AreEqual(2, span.Length);
        Assert.AreEqual(5, span[0]);
    }

    [TestMethod]
    public void GetHashCode_EqualBuffers_SameHash()
    {
        var a = ImmutableBuffer<int>.Create([1, 2]);
        var b = ImmutableBuffer<int>.Create([1, 2]);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Enumeration_ReturnsAllElements()
    {
        var buf = ImmutableBuffer<int>.Create([1, 2, 3]);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, buf.ToList());
    }
}

[TestClass]
public class ImmutableSequenceTests
{
    [TestMethod]
    public void Create_FromSpan_StoresData()
    {
        var seq = ImmutableSequence<int>.Create([1, 2, 3]);
        Assert.AreEqual(3, seq.Count);
        Assert.AreEqual(2, seq[1]);
    }

    [TestMethod]
    public void Empty_ReturnsEmptySequence()
    {
        var seq = ImmutableSequence<int>.Empty;
        Assert.AreEqual(0, seq.Count);
        Assert.IsTrue(seq.IsEmpty);
    }

    [TestMethod]
    public void CreateFrom_ReadOnlySequence()
    {
        var data = new ReadOnlySequence<byte>(new byte[] { 10, 20, 30 });
        var seq = ImmutableSequence<byte>.CreateFrom(in data);
        Assert.AreEqual(3, seq.Count);
        Assert.AreEqual(10, seq[0]);
    }

    [TestMethod]
    public void Add_ReturnsNewSequenceWithItem()
    {
        var seq = ImmutableSequence<int>.Create([1, 2]);
        var seq2 = seq.Add(3);
        Assert.AreEqual(3, seq2.Count);
        Assert.AreEqual(2, seq.Count); // original unchanged
    }

    [TestMethod]
    public void Slice_ReturnsSubset()
    {
        var seq = ImmutableSequence<int>.Create([10, 20, 30, 40]);
        var sliced = seq.Slice(1, 2);
        Assert.AreEqual(2, sliced.Count);
        Assert.AreEqual(20, sliced[0]);
    }

    [TestMethod]
    public void Equals_SameData_ReturnsTrue()
    {
        var a = ImmutableSequence<int>.Create([1, 2]);
        var b = ImmutableSequence<int>.Create([1, 2]);
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void Equals_DifferentData_ReturnsFalse()
    {
        var a = ImmutableSequence<int>.Create([1, 2]);
        var b = ImmutableSequence<int>.Create([1, 3]);
        Assert.IsFalse(a.Equals(b));
    }
}

[TestClass]
public class ChangeTokenBufferWatcherTests
{
    private sealed class TestNotifier : IBufferChangeNotifier
    {
        private CancellationTokenSource _cts = new();

        public bool HasChanged => _cts.IsCancellationRequested;

        public IChangeToken GetChangeToken() => new CancellationChangeToken(_cts.Token);

        public void TriggerChange()
        {
            var old = _cts;
            _cts = new CancellationTokenSource();
            old.Cancel();
        }
    }

    [TestMethod]
    public void Start_SetsIsWatchingTrue()
    {
        var notifier = new TestNotifier();
        using var watcher = new ChangeTokenBufferWatcher(notifier, () => { });
        watcher.Start();
        Assert.IsTrue(watcher.IsWatching);
    }

    [TestMethod]
    public void Stop_SetsIsWatchingFalse()
    {
        var notifier = new TestNotifier();
        using var watcher = new ChangeTokenBufferWatcher(notifier, () => { });
        watcher.Start();
        watcher.Stop();
        Assert.IsFalse(watcher.IsWatching);
    }

    [TestMethod]
    public void Change_IncrementsChangeCount()
    {
        var notifier = new TestNotifier();
        int callbackCount = 0;
        using var watcher = new ChangeTokenBufferWatcher(notifier, () => callbackCount++);
        watcher.Start();

        notifier.TriggerChange();

        // Give the change token a moment to propagate
        Thread.Sleep(50);
        Assert.IsTrue(watcher.ChangeCount >= 1);
        Assert.IsTrue(callbackCount >= 1);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var notifier = new TestNotifier();
        var watcher = new ChangeTokenBufferWatcher(notifier, () => { });
        watcher.Dispose();
        watcher.Dispose();
    }
}
