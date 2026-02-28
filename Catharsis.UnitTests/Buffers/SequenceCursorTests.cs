using System.Buffers;
using Catharsis.Buffers;

namespace Catharsis.UnitTests.Buffers;

///<summary>
///Unit tests for the <see cref="SequenceCursor{T}"/> ref struct.
///</summary>
[TestClass]
public class SequenceCursorTests
{
    #region Public methods
    ///<summary>
    ///Tests that Advance moves the cursor forward.
    ///</summary>
    [TestMethod]
    public void Advance_MovesForward()
    {
        byte[] data = [ 1, 2, 3, 4, 5 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        cursor.Advance(3);

        Assert.AreEqual(3L, cursor.Consumed);
        Assert.AreEqual(2L, cursor.Remaining);
        Assert.IsTrue(cursor.TryRead(out byte value));
        Assert.AreEqual(4, value);
    }

    ///<summary>
    ///Tests that Advance throws for negative count.
    ///</summary>
    [TestMethod]
    public void Advance_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        byte[] data = [ 1, 2, 3 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        try
        {
            cursor.Advance(-1);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        } catch(ArgumentOutOfRangeException)
        {
        }
    }

    ///<summary>
    ///Tests that Advance past the end throws.
    ///</summary>
    [TestMethod]
    public void Advance_PastEnd_Throws()
    {
        byte[] data = [ 1, 2 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        bool threw = false;
        try
        {
            cursor.Advance(10);
        } catch(Exception)
        {
            threw = true;
        }

        Assert.IsTrue(threw, "Expected an exception when advancing past the end.");
    }

    ///<summary>
    ///Tests that a cursor over an empty sequence has no remaining elements.
    ///</summary>
    [TestMethod]
    public void Constructor_EmptySequence_HasNoRemaining()
    {
        ReadOnlySequence<byte> seq = ReadOnlySequence<byte>.Empty;
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        Assert.IsFalse(cursor.HasRemaining);
        Assert.AreEqual(0L, cursor.Remaining);
        Assert.AreEqual(0L, cursor.Consumed);
    }

    ///<summary>
    ///Tests that a cursor over a single-segment sequence has correct Remaining.
    ///</summary>
    [TestMethod]
    public void Constructor_SingleSegment_CorrectRemaining()
    {
        byte[] data = [ 1, 2, 3, 4, 5 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        Assert.IsTrue(cursor.HasRemaining);
        Assert.AreEqual(5L, cursor.Remaining);
        Assert.AreEqual(0L, cursor.Consumed);
    }

    ///<summary>
    ///Tests that HasRemaining is false when all elements are consumed.
    ///</summary>
    [TestMethod]
    public void HasRemaining_AllConsumed_ReturnsFalse()
    {
        byte[] data = [ 1, 2 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);
        cursor.Advance(2);

        Assert.IsFalse(cursor.HasRemaining);
    }

    ///<summary>
    ///Tests that IndexOf searches from current position.
    ///</summary>
    [TestMethod]
    public void IndexOf_AfterAdvance_SearchesFromCurrentPosition()
    {
        byte[] data = [ 10, 20, 30, 20, 50 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);
        cursor.Advance(2);

        long index = cursor.IndexOf(20);

        Assert.AreEqual(1L, index); // relative to current position
    }

    ///<summary>
    ///Tests that IndexOf finds the correct offset.
    ///</summary>
    [TestMethod]
    public void IndexOf_ExistingValue_ReturnsCorrectOffset()
    {
        byte[] data = [ 10, 20, 30, 40, 50 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        long index = cursor.IndexOf(30);

        Assert.AreEqual(2L, index);
    }

    ///<summary>
    ///Tests that IndexOf returns -1 when value is not found.
    ///</summary>
    [TestMethod]
    public void IndexOf_NotFound_ReturnsNegativeOne()
    {
        byte[] data = [ 1, 2, 3 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        long index = cursor.IndexOf(99);

        Assert.AreEqual(-1L, index);
    }

    ///<summary>
    ///Tests that Reset moves cursor back to start.
    ///</summary>
    [TestMethod]
    public void Reset_MovesToStart()
    {
        byte[] data = [ 1, 2, 3, 4, 5 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);
        cursor.Advance(3);

        cursor.Reset();

        Assert.AreEqual(0L, cursor.Consumed);
        Assert.AreEqual(5L, cursor.Remaining);
        Assert.IsTrue(cursor.TryRead(out byte value));
        Assert.AreEqual(1, value);
    }

    ///<summary>
    ///Tests that TryPeek on empty sequence returns false.
    ///</summary>
    [TestMethod]
    public void TryPeek_EmptySequence_ReturnsFalse()
    {
        ReadOnlySequence<byte> seq = ReadOnlySequence<byte>.Empty;
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        Assert.IsFalse(cursor.TryPeek(out _));
    }

    ///<summary>
    ///Tests that TryPeek returns the next element without advancing.
    ///</summary>
    [TestMethod]
    public void TryPeek_ReturnsNextWithoutAdvancing()
    {
        byte[] data = [ 42, 99 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        Assert.IsTrue(cursor.TryPeek(out byte value));
        Assert.AreEqual(42, value);
        Assert.AreEqual(0L, cursor.Consumed);

        // Peek again returns the same value
        Assert.IsTrue(cursor.TryPeek(out byte value2));
        Assert.AreEqual(42, value2);
    }

    ///<summary>
    ///Tests that TryRead on empty sequence returns false.
    ///</summary>
    [TestMethod]
    public void TryRead_EmptySequence_ReturnsFalse()
    {
        ReadOnlySequence<byte> seq = ReadOnlySequence<byte>.Empty;
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        Assert.IsFalse(cursor.TryRead(out _));
    }

    ///<summary>
    ///Tests cursor over a multi-segment sequence.
    ///</summary>
    [TestMethod]
    public void TryRead_MultiSegmentSequence_ReadsAcrossSegments()
    {
        using PooledSequenceBuilder<byte> builder = new PooledSequenceBuilder<byte>(4);

        // Write first segment
        Span<byte> s1 = builder.GetSpan(4);
        s1[0] = 1;
        s1[1] = 2;
        s1[2] = 3;
        s1[3] = 4;
        builder.Advance(4);

        // Write second segment
        Span<byte> s2 = builder.GetSpan(4);
        s2[0] = 5;
        s2[1] = 6;
        s2[2] = 7;
        s2[3] = 8;
        builder.Advance(4);

        ReadOnlySequence<byte> seq = builder.Build();
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        byte[] results = new byte[8];
        for(int i = 0; i < 8; i++)
        {
            Assert.IsTrue(cursor.TryRead(out results[i]));
        }

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, results);
        Assert.IsFalse(cursor.TryRead(out _));
    }

    ///<summary>
    ///Tests that TryRead reads elements sequentially.
    ///</summary>
    [TestMethod]
    public void TryRead_ReadsElementsSequentially()
    {
        byte[] data = [ 10, 20, 30 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        Assert.IsTrue(cursor.TryRead(out byte v1));
        Assert.AreEqual(10, v1);

        Assert.IsTrue(cursor.TryRead(out byte v2));
        Assert.AreEqual(20, v2);

        Assert.IsTrue(cursor.TryRead(out byte v3));
        Assert.AreEqual(30, v3);

        Assert.IsFalse(cursor.TryRead(out _));
    }

    ///<summary>
    ///Tests that TryRead updates Consumed.
    ///</summary>
    [TestMethod]
    public void TryRead_UpdatesConsumed()
    {
        byte[] data = [ 1, 2, 3 ];
        ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>(data);
        SequenceCursor<byte> cursor = new SequenceCursor<byte>(in seq);

        cursor.TryRead(out _);
        cursor.TryRead(out _);

        Assert.AreEqual(2L, cursor.Consumed);
        Assert.AreEqual(1L, cursor.Remaining);
    }
    #endregion
}
