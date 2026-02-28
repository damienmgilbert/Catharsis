using Catharsis.Buffers;

namespace Catharsis.UnitTests.Buffers;

///<summary>
///Unit tests for the <see cref="PooledBuffer{T}"/> class.
///</summary>
[TestClass]
public class PooledBufferTests
{
    #region Public methods
    ///<summary>
    ///Tests that Advance throws after disposal.
    ///</summary>
    [TestMethod]
    public void Advance_AfterDispose_ThrowsObjectDisposedException()
    {
        PooledBuffer<byte> buffer = new PooledBuffer<byte>();
        buffer.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => buffer.Advance(1));
    }

    ///<summary>
    ///Tests that Advance increases WrittenCount.
    ///</summary>
    [TestMethod]
    public void Advance_IncreasesWrittenCount()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>();
        buffer.GetSpan(5);

        buffer.Advance(5);

        Assert.AreEqual(5, buffer.WrittenCount);
    }

    ///<summary>
    ///Tests that Advance throws for negative count.
    ///</summary>
    [TestMethod]
    public void Advance_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => buffer.Advance(-1));
    }

    ///<summary>
    ///Tests that Advance past end throws ArgumentOutOfRangeException.
    ///</summary>
    [TestMethod]
    public void Advance_PastEnd_ThrowsArgumentOutOfRangeException()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>(16);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => buffer.Advance(buffer.Capacity + 1));
    }

    ///<summary>
    ///Tests that Advance with zero does not change WrittenCount.
    ///</summary>
    [TestMethod]
    public void Advance_Zero_DoesNotChangeWrittenCount()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>();
        buffer.GetSpan(1);

        buffer.Advance(0);

        Assert.AreEqual(0, buffer.WrittenCount);
    }

    ///<summary>
    ///Tests that the default constructor creates a buffer with expected defaults.
    ///</summary>
    [TestMethod]
    public void Constructor_Default_CreatesBufferWithDefaults()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>();

        Assert.IsGreaterThanOrEqualTo(256, buffer.Capacity);
        Assert.AreEqual(0, buffer.WrittenCount);
        Assert.AreEqual(BufferGrowthStrategy.Doubling, buffer.GrowthStrategy);
    }

    ///<summary>
    ///Tests that the constructor throws when pool is null.
    ///</summary>
    [TestMethod]
    public void Constructor_NullPool_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new PooledBuffer<byte>(null!, 256)); }
    ///<summary>
    ///Tests that the constructor sets the growth strategy.
    ///</summary>
    [TestMethod]
    [DataRow(BufferGrowthStrategy.Doubling)]
    [DataRow(BufferGrowthStrategy.Linear)]
    [DataRow(BufferGrowthStrategy.Exact)]
    [DataRow(BufferGrowthStrategy.OnePointFive)]
    public void Constructor_WithGrowthStrategy_SetsStrategy(BufferGrowthStrategy strategy)
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>(256, strategy);

        Assert.AreEqual(strategy, buffer.GrowthStrategy);
    }

    ///<summary>
    ///Tests that the constructor throws when initial capacity is zero.
    ///</summary>
    [TestMethod]
    public void Constructor_ZeroCapacity_ThrowsArgumentOutOfRangeException() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PooledBuffer<byte>(0)); }
    ///<summary>
    ///Tests that Dispose is idempotent.
    ///</summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        PooledBuffer<byte> buffer = new PooledBuffer<byte>();

        buffer.Dispose();
        buffer.Dispose();
    }

    ///<summary>
    ///Tests that EnsureCapacity grows the buffer when needed.
    ///</summary>
    [TestMethod]
    public void EnsureCapacity_GrowsBufferWhenNeeded()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>(16);
        int originalCapacity = buffer.Capacity;

        buffer.EnsureCapacity(originalCapacity + 100);

        Assert.IsGreaterThanOrEqualTo(originalCapacity + 100, buffer.Capacity);
    }

    ///<summary>
    ///Tests that EnsureCapacity preserves written data when growing.
    ///</summary>
    [TestMethod]
    public void EnsureCapacity_PreservesWrittenData()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>(16);
        Span<byte> span = buffer.GetSpan(3);
        span[0] = 0xAA;
        span[1] = 0xBB;
        span[2] = 0xCC;
        buffer.Advance(3);

        buffer.EnsureCapacity(1024);

        Assert.AreEqual(3, buffer.WrittenCount);
        Assert.AreEqual(0xAA, buffer.WrittenSpan[0]);
        Assert.AreEqual(0xBB, buffer.WrittenSpan[1]);
        Assert.AreEqual(0xCC, buffer.WrittenSpan[2]);
    }

    ///<summary>
    ///Tests that EnsureCapacity does nothing when capacity is sufficient.
    ///</summary>
    [TestMethod]
    public void EnsureCapacity_SufficientCapacity_DoesNotChange()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>(256);
        int originalCapacity = buffer.Capacity;

        buffer.EnsureCapacity(10);

        Assert.AreEqual(originalCapacity, buffer.Capacity);
    }

    ///<summary>
    ///Tests that GetMemory throws after disposal.
    ///</summary>
    [TestMethod]
    public void GetMemory_AfterDispose_ThrowsObjectDisposedException()
    {
        PooledBuffer<byte> buffer = new PooledBuffer<byte>();
        buffer.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => buffer.GetMemory(1));
    }

    ///<summary>
    ///Tests that GetMemory returns writable memory of at least the requested size.
    ///</summary>
    [TestMethod]
    public void GetMemory_ReturnsMemoryOfAtLeastRequestedSize()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>();

        Memory<byte> mem = buffer.GetMemory(10);

        Assert.IsGreaterThanOrEqualTo(10, mem.Length);
    }

    ///<summary>
    ///Tests that GetSpan throws after disposal.
    ///</summary>
    [TestMethod]
    public void GetSpan_AfterDispose_ThrowsObjectDisposedException()
    {
        PooledBuffer<byte> buffer = new PooledBuffer<byte>();
        buffer.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => buffer.GetSpan(1));
    }

    ///<summary>
    ///Tests that GetSpan returns a span of at least the requested size.
    ///</summary>
    [TestMethod]
    public void GetSpan_ReturnsSpanOfAtLeastRequestedSize()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>();

        Span<byte> span = buffer.GetSpan(10);

        Assert.IsGreaterThanOrEqualTo(10, span.Length);
    }

    ///<summary>
    ///Tests Doubling growth strategy behavior.
    ///</summary>
    [TestMethod]
    public void GrowthStrategy_Doubling_DoublesCapacity()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>(16, BufferGrowthStrategy.Doubling);
        int initial = buffer.Capacity;

        buffer.GetSpan(initial + 1);

        Assert.IsGreaterThanOrEqualTo(initial * 2, buffer.Capacity);
    }

    ///<summary>
    ///Tests Exact growth strategy behavior.
    ///</summary>
    [TestMethod]
    public void GrowthStrategy_Exact_GrowsToExactRequired()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>(16, BufferGrowthStrategy.Exact);

        buffer.GetSpan(100);

        Assert.IsGreaterThanOrEqualTo(100, buffer.Capacity);
    }

    ///<summary>
    ///Tests Linear growth strategy behavior.
    ///</summary>
    [TestMethod]
    public void GrowthStrategy_Linear_IncreasesBy256()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>(16, BufferGrowthStrategy.Linear);
        int initial = buffer.Capacity;

        buffer.GetSpan(initial + 1);

        Assert.IsGreaterThanOrEqualTo(initial + 256, buffer.Capacity);
    }

    ///<summary>
    ///Tests that buffer works correctly as IBufferWriter with multiple write cycles.
    ///</summary>
    [TestMethod]
    public void IBufferWriter_WriteMultipleTimes_AccumulatesData()
    {
        using PooledBuffer<int> buffer = new PooledBuffer<int>();

        Span<int> s1 = buffer.GetSpan(2);
        s1[0] = 10;
        s1[1] = 20;
        buffer.Advance(2);

        Span<int> s2 = buffer.GetSpan(1);
        s2[0] = 30;
        buffer.Advance(1);

        Assert.AreEqual(3, buffer.WrittenCount);
        Assert.AreEqual(10, buffer.WrittenSpan[0]);
        Assert.AreEqual(20, buffer.WrittenSpan[1]);
        Assert.AreEqual(30, buffer.WrittenSpan[2]);
    }

    ///<summary>
    ///Tests that Reset clears WrittenCount without affecting capacity.
    ///</summary>
    [TestMethod]
    public void Reset_ClearsWrittenCount()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>();
        buffer.GetSpan(5);
        buffer.Advance(5);

        buffer.Reset();

        Assert.AreEqual(0, buffer.WrittenCount);
        Assert.IsGreaterThan(0, buffer.Capacity);
    }

    ///<summary>
    ///Tests that WrittenMemory returns the written data.
    ///</summary>
    [TestMethod]
    public void WrittenMemory_ReturnsWrittenData()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>();
        Memory<byte> mem = buffer.GetMemory(2);
        mem.Span[0] = 1;
        mem.Span[1] = 2;
        buffer.Advance(2);

        ReadOnlyMemory<byte> written = buffer.WrittenMemory;

        Assert.AreEqual(2, written.Length);
        Assert.AreEqual(1, written.Span[0]);
        Assert.AreEqual(2, written.Span[1]);
    }

    ///<summary>
    ///Tests that WrittenSpan returns the written data.
    ///</summary>
    [TestMethod]
    public void WrittenSpan_ReturnsWrittenData()
    {
        using PooledBuffer<byte> buffer = new PooledBuffer<byte>();
        Span<byte> span = buffer.GetSpan(3);
        span[0] = 10;
        span[1] = 20;
        span[2] = 30;
        buffer.Advance(3);

        ReadOnlySpan<byte> written = buffer.WrittenSpan;

        Assert.AreEqual(3, written.Length);
        Assert.AreEqual(10, written[0]);
        Assert.AreEqual(20, written[1]);
        Assert.AreEqual(30, written[2]);
    }
    #endregion
}
