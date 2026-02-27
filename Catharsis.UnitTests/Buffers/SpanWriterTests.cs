using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Catharsis.Buffers;

namespace Catharsis.UnitTests.Buffers;

/// <summary>
/// Unit tests for the <see cref="SpanWriter"/> ref struct.
/// </summary>
[TestClass]
public class SpanWriterTests
{
    /// <summary>
    /// Tests that the constructor initializes position and length correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_InitializesCorrectly()
    {
        byte[] data = new byte[10];
        var writer = new SpanWriter(data);

        Assert.AreEqual(0, writer.Position);
        Assert.AreEqual(10, writer.Length);
        Assert.AreEqual(10, writer.Remaining);
    }

    /// <summary>
    /// Tests that constructor with empty span produces zero length.
    /// </summary>
    [TestMethod]
    public void Constructor_EmptySpan_ZeroLength()
    {
        var writer = new SpanWriter(Span<byte>.Empty);

        Assert.AreEqual(0, writer.Length);
        Assert.AreEqual(0, writer.Remaining);
    }

    /// <summary>
    /// Tests that WriteByte writes a byte and advances.
    /// </summary>
    [TestMethod]
    public void WriteByte_WritesByteAndAdvances()
    {
        byte[] data = new byte[5];
        var writer = new SpanWriter(data);

        writer.WriteByte(0xAB);

        Assert.AreEqual(1, writer.Position);
        Assert.AreEqual(0xAB, data[0]);
    }

    /// <summary>
    /// Tests that WriteByte throws when at end of span.
    /// </summary>
    [TestMethod]
    public void WriteByte_AtEnd_ThrowsArgumentOutOfRangeException()
    {
        byte[] data = new byte[1];
        var writer = new SpanWriter(data);
        writer.WriteByte(0x01);

        try
        {
            writer.WriteByte(0x02);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    /// <summary>
    /// Tests that WriteInt16LittleEndian writes correctly.
    /// </summary>
    [TestMethod]
    public void WriteInt16LittleEndian_WritesCorrectValue()
    {
        byte[] data = new byte[2];
        var writer = new SpanWriter(data);

        writer.WriteInt16LittleEndian(12345);

        Assert.AreEqual(2, writer.Position);
        Assert.AreEqual((short)12345, BinaryPrimitives.ReadInt16LittleEndian(data));
    }

    /// <summary>
    /// Tests that WriteInt32LittleEndian writes correctly.
    /// </summary>
    [TestMethod]
    public void WriteInt32LittleEndian_WritesCorrectValue()
    {
        byte[] data = new byte[4];
        var writer = new SpanWriter(data);

        writer.WriteInt32LittleEndian(123456789);

        Assert.AreEqual(4, writer.Position);
        Assert.AreEqual(123456789, BinaryPrimitives.ReadInt32LittleEndian(data));
    }

    /// <summary>
    /// Tests that WriteInt64LittleEndian writes correctly.
    /// </summary>
    [TestMethod]
    public void WriteInt64LittleEndian_WritesCorrectValue()
    {
        byte[] data = new byte[8];
        var writer = new SpanWriter(data);

        writer.WriteInt64LittleEndian(9876543210L);

        Assert.AreEqual(8, writer.Position);
        Assert.AreEqual(9876543210L, BinaryPrimitives.ReadInt64LittleEndian(data));
    }

    /// <summary>
    /// Tests that WriteSingleLittleEndian writes correctly.
    /// </summary>
    [TestMethod]
    public void WriteSingleLittleEndian_WritesCorrectValue()
    {
        byte[] data = new byte[4];
        var writer = new SpanWriter(data);

        writer.WriteSingleLittleEndian(3.14f);

        Assert.AreEqual(3.14f, BinaryPrimitives.ReadSingleLittleEndian(data));
    }

    /// <summary>
    /// Tests that WriteDoubleLittleEndian writes correctly.
    /// </summary>
    [TestMethod]
    public void WriteDoubleLittleEndian_WritesCorrectValue()
    {
        byte[] data = new byte[8];
        var writer = new SpanWriter(data);

        writer.WriteDoubleLittleEndian(2.71828);

        Assert.AreEqual(2.71828, BinaryPrimitives.ReadDoubleLittleEndian(data));
    }

    /// <summary>
    /// Tests that WriteBytes writes data correctly.
    /// </summary>
    [TestMethod]
    public void WriteBytes_WritesDataCorrectly()
    {
        byte[] data = new byte[10];
        var writer = new SpanWriter(data);
        byte[] source = [0xAA, 0xBB, 0xCC];

        writer.WriteBytes(source);

        Assert.AreEqual(3, writer.Position);
        Assert.AreEqual(0xAA, data[0]);
        Assert.AreEqual(0xBB, data[1]);
        Assert.AreEqual(0xCC, data[2]);
    }

    /// <summary>
    /// Tests that WriteBytes throws when past end.
    /// </summary>
    [TestMethod]
    public void WriteBytes_PastEnd_ThrowsArgumentOutOfRangeException()
    {
        byte[] data = new byte[2];
        var writer = new SpanWriter(data);

        try
        {
            writer.WriteBytes(new byte[] { 1, 2, 3, 4 });
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    /// <summary>
    /// Tests that WriteUtf8String writes a length-prefixed UTF-8 string.
    /// </summary>
    [TestMethod]
    public void WriteUtf8String_WritesLengthPrefixedString()
    {
        string text = "Hello";
        int byteCount = Encoding.UTF8.GetByteCount(text);
        byte[] data = new byte[4 + byteCount];
        var writer = new SpanWriter(data);

        writer.WriteUtf8String(text);

        Assert.AreEqual(data.Length, writer.Position);
        int prefixedLength = BinaryPrimitives.ReadInt32LittleEndian(data);
        Assert.AreEqual(byteCount, prefixedLength);
        Assert.AreEqual(text, Encoding.UTF8.GetString(data, 4, byteCount));
    }

    /// <summary>
    /// Tests that WriteUtf8String writes empty string correctly.
    /// </summary>
    [TestMethod]
    public void WriteUtf8String_EmptyString_WritesZeroLength()
    {
        byte[] data = new byte[4];
        var writer = new SpanWriter(data);

        writer.WriteUtf8String(ReadOnlySpan<char>.Empty);

        Assert.AreEqual(4, writer.Position);
        Assert.AreEqual(0, BinaryPrimitives.ReadInt32LittleEndian(data));
    }

    /// <summary>
    /// Tests that Write&lt;T&gt; writes an unmanaged value.
    /// </summary>
    [TestMethod]
    public void Write_UnmanagedType_WritesCorrectValue()
    {
        byte[] data = new byte[Unsafe.SizeOf<int>()];
        var writer = new SpanWriter(data);

        writer.Write(42);

        Assert.AreEqual(42, MemoryMarshal.Read<int>(data));
    }

    /// <summary>
    /// Tests that Skip advances position and clears bytes.
    /// </summary>
    [TestMethod]
    public void Skip_AdvancesAndClearsBytes()
    {
        byte[] data = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF];
        var writer = new SpanWriter(data);

        writer.Skip(3);

        Assert.AreEqual(3, writer.Position);
        Assert.AreEqual(0, data[0]);
        Assert.AreEqual(0, data[1]);
        Assert.AreEqual(0, data[2]);
        Assert.AreEqual(0xFF, data[3]); // untouched
    }

    /// <summary>
    /// Tests that Skip throws for negative count.
    /// </summary>
    [TestMethod]
    public void Skip_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        var writer = new SpanWriter(new byte[10]);

        try
        {
            writer.Skip(-1);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    /// <summary>
    /// Tests that Skip throws when past end.
    /// </summary>
    [TestMethod]
    public void Skip_PastEnd_ThrowsArgumentOutOfRangeException()
    {
        var writer = new SpanWriter(new byte[5]);

        try
        {
            writer.Skip(10);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    /// <summary>
    /// Tests that Reset resets position to zero.
    /// </summary>
    [TestMethod]
    public void Reset_ResetsPositionToZero()
    {
        var writer = new SpanWriter(new byte[10]);
        writer.WriteByte(1);
        writer.WriteByte(2);

        writer.Reset();

        Assert.AreEqual(0, writer.Position);
        Assert.AreEqual(10, writer.Remaining);
    }

    /// <summary>
    /// Tests that FreeSpan returns the remaining writable area.
    /// </summary>
    [TestMethod]
    public void FreeSpan_ReturnsRemainingArea()
    {
        byte[] data = new byte[10];
        var writer = new SpanWriter(data);
        writer.Skip(3);

        Span<byte> free = writer.FreeSpan;

        Assert.AreEqual(7, free.Length);
    }

    /// <summary>
    /// Tests that sequential writes of different types work together.
    /// </summary>
    [TestMethod]
    public void SequentialWrites_MixedTypes_WritesCorrectly()
    {
        byte[] data = new byte[1 + 4 + 8];
        var writer = new SpanWriter(data);

        writer.WriteByte(0xFF);
        writer.WriteInt32LittleEndian(42);
        writer.WriteInt64LittleEndian(100L);

        Assert.AreEqual(data.Length, writer.Position);
        Assert.AreEqual(0, writer.Remaining);
        Assert.AreEqual(0xFF, data[0]);
        Assert.AreEqual(42, BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(1)));
        Assert.AreEqual(100L, BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan(5)));
    }

    /// <summary>
    /// Tests round-trip with SpanReader.
    /// </summary>
    [TestMethod]
    public void RoundTrip_WriteAndRead_ProducesOriginalValues()
    {
        byte[] buffer = new byte[4 + 8 + 4 + Encoding.UTF8.GetByteCount("Test")];
        var writer = new SpanWriter(buffer);

        writer.WriteInt32LittleEndian(42);
        writer.WriteDoubleLittleEndian(3.14);
        writer.WriteUtf8String("Test");

        var reader = new SpanReader(buffer);

        Assert.AreEqual(42, reader.ReadInt32LittleEndian());
        Assert.AreEqual(3.14, reader.ReadDoubleLittleEndian());
        Assert.AreEqual("Test", reader.ReadUtf8String());
    }
}
