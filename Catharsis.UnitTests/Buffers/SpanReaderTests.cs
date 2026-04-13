using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using Catharsis.Buffers;

namespace Catharsis.UnitTests.Buffers;

///<summary>
///Unit tests for the <see cref="SpanReader"/> ref struct.
///</summary>
[TestClass]
public class SpanReaderTests
{
    #region Public methods
    ///<summary>
    ///Tests that constructor with empty span produces zero length.
    ///</summary>
    [TestMethod]
    public void Constructor_EmptySpan_ZeroLength()
    {
        SpanReader reader = new([]);

        Assert.AreEqual(0, reader.Length);
        Assert.AreEqual(0, reader.Remaining);
        Assert.IsFalse(reader.HasRemaining);
    }

    ///<summary>
    ///Tests that the constructor initializes position and length correctly.
    ///</summary>
    [TestMethod]
    public void Constructor_InitializesCorrectly()
    {
        byte[] data = new byte[10];
        SpanReader reader = new(data);

        Assert.AreEqual(0, reader.Position);
        Assert.AreEqual(10, reader.Length);
        Assert.AreEqual(10, reader.Remaining);
        Assert.IsTrue(reader.HasRemaining);
    }

    ///<summary>
    ///Tests that Read&lt;T&gt; reads an unmanaged value.
    ///</summary>
    [TestMethod]
    public void Read_UnmanagedType_ReadsCorrectValue()
    {
        int expected = 42;
        byte[] data = new byte[Unsafe.SizeOf<int>()];
        BitConverter.TryWriteBytes(data, expected);
        SpanReader reader = new(data);

        int result = reader.Read<int>();

        Assert.AreEqual(expected, result);
    }

    ///<summary>
    ///Tests that ReadByte throws when at end of span.
    ///</summary>
    [TestMethod]
    public void ReadByte_AtEnd_ThrowsArgumentOutOfRangeException()
    {
        byte[] data = [ 0x01 ];
        SpanReader reader = new(data);
        reader.ReadByte();

        try
        {
            reader.ReadByte();
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        } catch(ArgumentOutOfRangeException)
        {
        }
    }

    ///<summary>
    ///Tests that ReadByte reads a byte and advances position.
    ///</summary>
    [TestMethod]
    public void ReadByte_ReadsByteAndAdvances()
    {
        byte[] data = [ 0xAB, 0xCD ];
        SpanReader reader = new(data);

        byte result = reader.ReadByte();

        Assert.AreEqual(0xAB, result);
        Assert.AreEqual(1, reader.Position);
    }

    ///<summary>
    ///Tests that ReadBytes throws for negative count.
    ///</summary>
    [TestMethod]
    public void ReadBytes_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        SpanReader reader = new(new byte[10]);

        try
        {
            reader.ReadBytes(-1);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        } catch(ArgumentOutOfRangeException)
        {
        }
    }

    ///<summary>
    ///Tests that ReadBytes throws when reading past end.
    ///</summary>
    [TestMethod]
    public void ReadBytes_PastEnd_ThrowsArgumentOutOfRangeException()
    {
        SpanReader reader = new(new byte[5]);

        try
        {
            reader.ReadBytes(10);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        } catch(ArgumentOutOfRangeException)
        {
        }
    }

    ///<summary>
    ///Tests that ReadBytes returns the correct slice and advances.
    ///</summary>
    [TestMethod]
    public void ReadBytes_ReturnsCorrectSlice()
    {
        byte[] data = [ 1, 2, 3, 4, 5 ];
        SpanReader reader = new(data);
        reader.ReadByte(); // skip first

        ReadOnlySpan<byte> result = reader.ReadBytes(3);

        Assert.AreEqual(3, result.Length);
        Assert.AreEqual(2, result[0]);
        Assert.AreEqual(3, result[1]);
        Assert.AreEqual(4, result[2]);
        Assert.AreEqual(4, reader.Position);
    }

    ///<summary>
    ///Tests that ReadDoubleLittleEndian reads a double correctly.
    ///</summary>
    [TestMethod]
    public void ReadDoubleLittleEndian_ReadsCorrectValue()
    {
        byte[] data = new byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(data, 2.71828);
        SpanReader reader = new(data);

        double result = reader.ReadDoubleLittleEndian();

        Assert.AreEqual(2.71828, result);
    }

    ///<summary>
    ///Tests that ReadInt16LittleEndian reads a 16-bit integer correctly.
    ///</summary>
    [TestMethod]
    public void ReadInt16LittleEndian_ReadsCorrectValue()
    {
        byte[] data = new byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(data, 12345);
        SpanReader reader = new(data);

        short result = reader.ReadInt16LittleEndian();

        Assert.AreEqual((short)12345, result);
        Assert.AreEqual(2, reader.Position);
    }

    ///<summary>
    ///Tests that ReadInt32LittleEndian reads a 32-bit integer correctly.
    ///</summary>
    [TestMethod]
    public void ReadInt32LittleEndian_ReadsCorrectValue()
    {
        byte[] data = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(data, 123456789);
        SpanReader reader = new(data);

        int result = reader.ReadInt32LittleEndian();

        Assert.AreEqual(123456789, result);
        Assert.AreEqual(4, reader.Position);
    }

    ///<summary>
    ///Tests that ReadInt64LittleEndian reads a 64-bit integer correctly.
    ///</summary>
    [TestMethod]
    public void ReadInt64LittleEndian_ReadsCorrectValue()
    {
        byte[] data = new byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(data, 9876543210L);
        SpanReader reader = new(data);

        long result = reader.ReadInt64LittleEndian();

        Assert.AreEqual(9876543210L, result);
        Assert.AreEqual(8, reader.Position);
    }

    ///<summary>
    ///Tests that ReadSingleLittleEndian reads a float correctly.
    ///</summary>
    [TestMethod]
    public void ReadSingleLittleEndian_ReadsCorrectValue()
    {
        byte[] data = new byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(data, 3.14f);
        SpanReader reader = new(data);

        float result = reader.ReadSingleLittleEndian();

        Assert.AreEqual(3.14f, result);
    }

    ///<summary>
    ///Tests that ReadUtf8String reads an empty string.
    ///</summary>
    [TestMethod]
    public void ReadUtf8String_EmptyString_ReadsCorrectly()
    {
        byte[] data = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(data, 0);
        SpanReader reader = new(data);

        string result = reader.ReadUtf8String();

        Assert.AreEqual(string.Empty, result);
    }

    ///<summary>
    ///Tests that ReadUtf8String reads a length-prefixed UTF-8 string.
    ///</summary>
    [TestMethod]
    public void ReadUtf8String_ReadsCorrectString()
    {
        string expected = "Hello";
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(expected);
        byte[] data = new byte[4 + utf8Bytes.Length];
        BinaryPrimitives.WriteInt32LittleEndian(data, utf8Bytes.Length);
        utf8Bytes.CopyTo(data.AsSpan(4));
        SpanReader reader = new(data);

        string result = reader.ReadUtf8String();

        Assert.AreEqual(expected, result);
        Assert.AreEqual(data.Length, reader.Position);
    }

    ///<summary>
    ///Tests that Reset resets position to zero.
    ///</summary>
    [TestMethod]
    public void Reset_ResetsPositionToZero()
    {
        SpanReader reader = new(new byte[10]);
        reader.Skip(5);

        reader.Reset();

        Assert.AreEqual(0, reader.Position);
        Assert.AreEqual(10, reader.Remaining);
    }

    ///<summary>
    ///Tests that sequential reads of different types work together.
    ///</summary>
    [TestMethod]
    public void SequentialReads_MixedTypes_ReadsCorrectly()
    {
        byte[] data = new byte[1 + 4 + 8];
        data[0] = 0xFF;
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(1), 42);
        BinaryPrimitives.WriteInt64LittleEndian(data.AsSpan(5), 100L);
        SpanReader reader = new(data);

        byte b = reader.ReadByte();
        int i = reader.ReadInt32LittleEndian();
        long l = reader.ReadInt64LittleEndian();

        Assert.AreEqual(0xFF, b);
        Assert.AreEqual(42, i);
        Assert.AreEqual(100L, l);
        Assert.IsFalse(reader.HasRemaining);
    }

    ///<summary>
    ///Tests that Skip advances the position by the specified count.
    ///</summary>
    [TestMethod]
    public void Skip_AdvancesPosition()
    {
        SpanReader reader = new(new byte[10]);

        reader.Skip(5);

        Assert.AreEqual(5, reader.Position);
        Assert.AreEqual(5, reader.Remaining);
    }

    ///<summary>
    ///Tests that Skip throws for negative count.
    ///</summary>
    [TestMethod]
    public void Skip_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        SpanReader reader = new(new byte[10]);

        try
        {
            reader.Skip(-1);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        } catch(ArgumentOutOfRangeException)
        {
        }
    }

    ///<summary>
    ///Tests that Skip throws when past end.
    ///</summary>
    [TestMethod]
    public void Skip_PastEnd_ThrowsArgumentOutOfRangeException()
    {
        SpanReader reader = new(new byte[5]);

        try
        {
            reader.Skip(10);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        } catch(ArgumentOutOfRangeException)
        {
        }
    }

    ///<summary>
    ///Tests that UnreadSpan returns the remaining bytes.
    ///</summary>
    [TestMethod]
    public void UnreadSpan_ReturnsRemainingBytes()
    {
        byte[] data = [ 1, 2, 3, 4, 5 ];
        SpanReader reader = new(data);
        reader.Skip(2);

        ReadOnlySpan<byte> unread = reader.UnreadSpan;

        Assert.AreEqual(3, unread.Length);
        Assert.AreEqual(3, unread[0]);
        Assert.AreEqual(4, unread[1]);
        Assert.AreEqual(5, unread[2]);
    }
    #endregion
}
