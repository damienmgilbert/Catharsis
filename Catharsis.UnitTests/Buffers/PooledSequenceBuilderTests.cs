using System.Buffers;
using Catharsis.Buffers;

namespace Catharsis.UnitTests.Buffers;

///<summary>
///Unit tests for the <see cref="PooledSequenceBuilder{T}"/> class.
///</summary>
[TestClass]
public class PooledSequenceBuilderTests
{
    #region Public methods
    ///<summary>
    ///Tests that Advance throws after disposal.
    ///</summary>
    [TestMethod]
    public void Advance_AfterDispose_ThrowsObjectDisposedException()
    {
        PooledSequenceBuilder<byte> builder = new();
        builder.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => builder.Advance(1));
    }

    ///<summary>
    ///Tests that Advance throws for negative count.
    ///</summary>
    [TestMethod]
    public void Advance_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        using PooledSequenceBuilder<byte> builder = new();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => builder.Advance(-1));
    }

    ///<summary>
    ///Tests that Advance throws when past the current buffer.
    ///</summary>
    [TestMethod]
    public void Advance_PastBuffer_ThrowsArgumentOutOfRangeException()
    {
        using PooledSequenceBuilder<byte> builder = new(16);
        builder.GetSpan(10);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => builder.Advance(100000));
    }

    ///<summary>
    ///Tests that Build throws after disposal.
    ///</summary>
    [TestMethod]
    public void Build_AfterDispose_ThrowsObjectDisposedException()
    {
        PooledSequenceBuilder<byte> builder = new();
        builder.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => builder.Build());
    }

    ///<summary>
    ///Tests that Build preserves data order across segments.
    ///</summary>
    [TestMethod]
    public void Build_MultipleSegments_PreservesDataOrder()
    {
        using PooledSequenceBuilder<byte> builder = new(16);

        for(int i = 0; i < 5; i++)
        {
            Span<byte> span = builder.GetSpan(16);
            for(int j = 0; j < 16; j++)
            {
                span[j] = (byte)(i * 16 + j);
            }

            builder.Advance(16);
        }

        ReadOnlySequence<byte> sequence = builder.Build();
        byte[] result = sequence.ToArray();

        for(int i = 0; i < 80; i++)
        {
            Assert.AreEqual((byte)i, result[i], $"Mismatch at index {i}.");
        }
    }

    ///<summary>
    ///Tests that Build returns a multi-segment sequence for large writes.
    ///</summary>
    [TestMethod]
    public void Build_MultipleSegments_ReturnsCorrectTotalLength()
    {
        using PooledSequenceBuilder<byte> builder = new(16);

        // Write enough data to span multiple segments
        for(int i = 0; i < 10; i++)
        {
            Span<byte> span = builder.GetSpan(16);
            for(int j = 0; j < 16; j++)
            {
                span[j] = (byte)(i * 16 + j);
            }

            builder.Advance(16);
        }

        ReadOnlySequence<byte> sequence = builder.Build();

        Assert.AreEqual(160L, sequence.Length);
    }

    ///<summary>
    ///Tests that Build returns Empty for no data.
    ///</summary>
    [TestMethod]
    public void Build_NoData_ReturnsEmptySequence()
    {
        using PooledSequenceBuilder<byte> builder = new();

        ReadOnlySequence<byte> sequence = builder.Build();

        Assert.AreEqual(0L, sequence.Length);
    }

    ///<summary>
    ///Tests that Build returns a single-segment sequence for small writes.
    ///</summary>
    [TestMethod]
    public void Build_SingleSegment_ReturnsSingleSegmentSequence()
    {
        using PooledSequenceBuilder<byte> builder = new();
        Span<byte> span = builder.GetSpan(3);
        span[0] = 0xAA;
        span[1] = 0xBB;
        span[2] = 0xCC;
        builder.Advance(3);

        ReadOnlySequence<byte> sequence = builder.Build();

        Assert.AreEqual(3L, sequence.Length);
        Assert.IsTrue(sequence.IsSingleSegment);
        Assert.AreEqual(0xAA, sequence.FirstSpan[0]);
        Assert.AreEqual(0xBB, sequence.FirstSpan[1]);
        Assert.AreEqual(0xCC, sequence.FirstSpan[2]);
    }

    ///<summary>
    ///Tests that the default constructor creates a valid instance.
    ///</summary>
    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        using PooledSequenceBuilder<byte> builder = new();

        Assert.AreEqual(0L, builder.WrittenCount);
    }

    ///<summary>
    ///Tests that the constructor throws when pool is null.
    ///</summary>
    [TestMethod]
    public void Constructor_NullPool_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new PooledSequenceBuilder<byte>(null!, 4096)); }
    ///<summary>
    ///Tests that the constructor throws when segment size is zero.
    ///</summary>
    [TestMethod]
    public void Constructor_ZeroSegmentSize_ThrowsArgumentOutOfRangeException() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new PooledSequenceBuilder<byte>(0)); }
    ///<summary>
    ///Tests that Dispose is idempotent.
    ///</summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        PooledSequenceBuilder<byte> builder = new();

        builder.Dispose();
        builder.Dispose();
    }

    ///<summary>
    ///Tests that GetMemory throws after disposal.
    ///</summary>
    [TestMethod]
    public void GetMemory_AfterDispose_ThrowsObjectDisposedException()
    {
        PooledSequenceBuilder<byte> builder = new();
        builder.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => builder.GetMemory(1));
    }

    ///<summary>
    ///Tests that GetMemory returns writable memory.
    ///</summary>
    [TestMethod]
    public void GetMemory_ReturnsWritableMemory()
    {
        using PooledSequenceBuilder<byte> builder = new();

        Memory<byte> mem = builder.GetMemory(10);

        Assert.IsGreaterThanOrEqualTo(10, mem.Length);
    }

    ///<summary>
    ///Tests that GetSpan throws after disposal.
    ///</summary>
    [TestMethod]
    public void GetSpan_AfterDispose_ThrowsObjectDisposedException()
    {
        PooledSequenceBuilder<byte> builder = new();
        builder.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => builder.GetSpan(1));
    }

    ///<summary>
    ///Tests that GetSpan returns writable span.
    ///</summary>
    [TestMethod]
    public void GetSpan_ReturnsWritableSpan()
    {
        using PooledSequenceBuilder<byte> builder = new();

        Span<byte> span = builder.GetSpan(10);

        Assert.IsGreaterThanOrEqualTo(10, span.Length);
    }

    ///<summary>
    ///Tests that Reset allows reuse.
    ///</summary>
    [TestMethod]
    public void Reset_AllowsReuse()
    {
        using PooledSequenceBuilder<byte> builder = new();
        Span<byte> span = builder.GetSpan(5);
        builder.Advance(5);

        builder.Reset();

        Span<byte> newSpan = builder.GetSpan(3);
        newSpan[0] = 42;
        builder.Advance(1);

        ReadOnlySequence<byte> sequence = builder.Build();
        Assert.AreEqual(1L, sequence.Length);
        Assert.AreEqual(42, sequence.FirstSpan[0]);
    }

    ///<summary>
    ///Tests that Reset clears all data.
    ///</summary>
    [TestMethod]
    public void Reset_ClearsAllData()
    {
        using PooledSequenceBuilder<byte> builder = new();
        Span<byte> span = builder.GetSpan(5);
        builder.Advance(5);

        builder.Reset();

        Assert.AreEqual(0L, builder.WrittenCount);
    }

    ///<summary>
    ///Tests that WrittenCount tracks data correctly.
    ///</summary>
    [TestMethod]
    public void WrittenCount_AfterWrites_ReflectsTotalWritten()
    {
        using PooledSequenceBuilder<byte> builder = new();
        Span<byte> span = builder.GetSpan(5);
        span[0] = 1;
        span[1] = 2;
        span[2] = 3;
        builder.Advance(3);

        Assert.AreEqual(3L, builder.WrittenCount);
    }
    #endregion
}
