namespace Catharsis.Common.UnitTests;

[TestClass]
public class ValueStopwatchTests
{
    [TestMethod]
    public void Default_IsNotActive()
    {
        ValueStopwatch sw = default;
        Assert.IsFalse(sw.IsActive);
    }

    [TestMethod]
    public void StartNew_IsActive()
    {
        var sw = ValueStopwatch.StartNew();
        Assert.IsTrue(sw.IsActive);
    }

    [TestMethod]
    public void GetElapsedTime_NotStarted_Throws()
    {
        ValueStopwatch sw = default;
        Assert.ThrowsExactly<InvalidOperationException>(() => sw.GetElapsedTime());
    }

    [TestMethod]
    public void GetElapsedTime_Started_ReturnsNonNegative()
    {
        var sw = ValueStopwatch.StartNew();
        Thread.Sleep(10);
        var elapsed = sw.GetElapsedTime();
        Assert.IsTrue(elapsed.TotalMilliseconds >= 0);
    }

    [TestMethod]
    public void GetElapsedMilliseconds_ReturnsNonNegative()
    {
        var sw = ValueStopwatch.StartNew();
        Assert.IsTrue(sw.GetElapsedMilliseconds() >= 0);
    }

    [TestMethod]
    public void GetElapsedMicroseconds_ReturnsNonNegative()
    {
        var sw = ValueStopwatch.StartNew();
        Assert.IsTrue(sw.GetElapsedMicroseconds() >= 0);
    }
}

[TestClass]
public class BufferComparerTests
{
    [TestMethod]
    public void Equals_SameArrays_ReturnsTrue()
    {
        int[] a = [1, 2, 3];
        int[] b = [1, 2, 3];
        Assert.IsTrue(BufferComparer<int>.Default.Equals(a, b));
    }

    [TestMethod]
    public void Equals_DifferentArrays_ReturnsFalse()
    {
        int[] a = [1, 2, 3];
        int[] b = [1, 2, 4];
        Assert.IsFalse(BufferComparer<int>.Default.Equals(a, b));
    }

    [TestMethod]
    public void Equals_NullArrays_HandlesCorrectly()
    {
        Assert.IsTrue(BufferComparer<int>.Default.Equals(null, null));
        Assert.IsFalse(BufferComparer<int>.Default.Equals([1], null));
        Assert.IsFalse(BufferComparer<int>.Default.Equals(null, [1]));
    }

    [TestMethod]
    public void Equals_SameReference_ReturnsTrue()
    {
        int[] a = [1, 2, 3];
        Assert.IsTrue(BufferComparer<int>.Default.Equals(a, a));
    }

    [TestMethod]
    public void Compare_Equal_ReturnsZero()
    {
        int[] a = [1, 2, 3];
        int[] b = [1, 2, 3];
        Assert.AreEqual(0, BufferComparer<int>.Default.Compare(a, b));
    }

    [TestMethod]
    public void Compare_LessThan_ReturnsNegative()
    {
        int[] a = [1, 2];
        int[] b = [1, 3];
        Assert.IsTrue(BufferComparer<int>.Default.Compare(a, b) < 0);
    }

    [TestMethod]
    public void Compare_ShorterArray_ReturnsNegative()
    {
        int[] a = [1, 2];
        int[] b = [1, 2, 3];
        Assert.IsTrue(BufferComparer<int>.Default.Compare(a, b) < 0);
    }

    [TestMethod]
    public void Compare_NullHandling()
    {
        Assert.AreEqual(0, BufferComparer<int>.Default.Compare(null, null));
        Assert.IsTrue(BufferComparer<int>.Default.Compare(null, [1]) < 0);
        Assert.IsTrue(BufferComparer<int>.Default.Compare([1], null) > 0);
    }

    [TestMethod]
    public void GetHashCode_EqualArrays_SameHash()
    {
        int[] a = [1, 2, 3];
        int[] b = [1, 2, 3];
        Assert.AreEqual(
            BufferComparer<int>.Default.GetHashCode(a),
            BufferComparer<int>.Default.GetHashCode(b));
    }

    [TestMethod]
    public void Static_Equals_Span_ReturnsCorrectResult()
    {
        ReadOnlySpan<int> a = [1, 2, 3];
        ReadOnlySpan<int> b = [1, 2, 3];
        Assert.IsTrue(BufferComparer<int>.Equals(a, b));
    }
}

[TestClass]
public class AsyncBufferLockTests
{
    [TestMethod]
    public async Task LockAsync_AcquiresAndReleases()
    {
        using var abl = new AsyncBufferLock();
        Assert.IsFalse(abl.IsLocked);

        using (await abl.LockAsync())
        {
            Assert.IsTrue(abl.IsLocked);
        }

        Assert.IsFalse(abl.IsLocked);
    }

    [TestMethod]
    public async Task LockValueAsync_AcquiresAndReleases()
    {
        using var abl = new AsyncBufferLock();

        using (await abl.LockValueAsync())
        {
            Assert.IsTrue(abl.IsLocked);
        }

        Assert.IsFalse(abl.IsLocked);
    }

    [TestMethod]
    public void Lock_Synchronous_AcquiresAndReleases()
    {
        using var abl = new AsyncBufferLock();

        using (abl.Lock())
        {
            Assert.IsTrue(abl.IsLocked);
        }

        Assert.IsFalse(abl.IsLocked);
    }

    [TestMethod]
    public void TryLock_NotLocked_ReturnsTrue()
    {
        using var abl = new AsyncBufferLock();
        Assert.IsTrue(abl.TryLock(out var handle));
        Assert.IsNotNull(handle);
        Assert.IsTrue(abl.IsLocked);
        handle.Dispose();
        Assert.IsFalse(abl.IsLocked);
    }

    [TestMethod]
    public void TryLock_AlreadyLocked_ReturnsFalse()
    {
        using var abl = new AsyncBufferLock();
        using var handle = abl.Lock();
        Assert.IsFalse(abl.TryLock(out var h2));
        Assert.IsNull(h2);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var abl = new AsyncBufferLock();
        abl.Dispose();
        abl.Dispose();
    }

    [TestMethod]
    public void LockAsync_AfterDispose_Throws()
    {
        var abl = new AsyncBufferLock();
        abl.Dispose();
        Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => abl.LockAsync());
    }
}

[TestClass]
public class BufferWeakReferenceCacheTests
{
    [TestMethod]
    public void Set_And_TryGet_ReturnsValue()
    {
        var cache = new BufferWeakReferenceCache<string>();
        cache.Set("key", "value");
        Assert.IsTrue(cache.TryGet("key", out string? val));
        Assert.AreEqual("value", val);
    }

    [TestMethod]
    public void TryGet_MissingKey_ReturnsFalse()
    {
        var cache = new BufferWeakReferenceCache<string>();
        Assert.IsFalse(cache.TryGet("missing", out _));
    }

    [TestMethod]
    public void GetOrCreate_CreatesOnMiss()
    {
        var cache = new BufferWeakReferenceCache<string>();
        string val = cache.GetOrCreate("k", () => "created");
        Assert.AreEqual("created", val);
    }

    [TestMethod]
    public void GetOrCreate_ReturnsCachedOnHit()
    {
        var cache = new BufferWeakReferenceCache<string>();
        cache.Set("k", "original");
        string val = cache.GetOrCreate("k", () => "new");
        Assert.AreEqual("original", val);
    }

    [TestMethod]
    public void Remove_ReturnsCorrectResult()
    {
        var cache = new BufferWeakReferenceCache<string>();
        cache.Set("k", "v");
        Assert.IsTrue(cache.Remove("k"));
        Assert.IsFalse(cache.Remove("k"));
    }

    [TestMethod]
    public void Clear_RemovesAll()
    {
        var cache = new BufferWeakReferenceCache<string>();
        cache.Set("a", "1"); cache.Set("b", "2");
        cache.Clear();
        Assert.AreEqual(0, cache.Count);
    }

    [TestMethod]
    public void Count_ReflectsEntries()
    {
        var cache = new BufferWeakReferenceCache<string>();
        cache.Set("a", "1"); cache.Set("b", "2");
        Assert.AreEqual(2, cache.Count);
    }
}
