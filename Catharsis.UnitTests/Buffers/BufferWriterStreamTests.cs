using System.Buffers;
using Catharsis.Buffers;

namespace Catharsis.Buffers.UnitTests;

/// <summary>
/// Unit tests for the <see cref="BufferWriterStream"/> class.
/// </summary>
[TestClass]
public class BufferWriterStreamTests
{
    /// <summary>
    /// Tests that the constructor throws when writer is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullWriter_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new BufferWriterStream(null!));
    }

    /// <summary>
    /// Tests that CanRead always returns false.
    /// </summary>
    [TestMethod]
    public void CanRead_ReturnsFalse()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());

        Assert.IsFalse(stream.CanRead);
    }

    /// <summary>
    /// Tests that CanSeek always returns false.
    /// </summary>
    [TestMethod]
    public void CanSeek_ReturnsFalse()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());

        Assert.IsFalse(stream.CanSeek);
    }

    /// <summary>
    /// Tests that CanWrite returns true when not disposed.
    /// </summary>
    [TestMethod]
    public void CanWrite_NotDisposed_ReturnsTrue()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());

        Assert.IsTrue(stream.CanWrite);
    }

    /// <summary>
    /// Tests that CanWrite returns false after disposal.
    /// </summary>
    [TestMethod]
    public void CanWrite_AfterDispose_ReturnsFalse()
    {
        var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());
        stream.Dispose();

        Assert.IsFalse(stream.CanWrite);
    }

    /// <summary>
    /// Tests that Length returns zero initially.
    /// </summary>
    [TestMethod]
    public void Length_InitiallyZero()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());

        Assert.AreEqual(0L, stream.Length);
    }

    /// <summary>
    /// Tests that Length increases after writing.
    /// </summary>
    [TestMethod]
    public void Length_AfterWrite_ReflectsBytesWritten()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());
        byte[] data = [1, 2, 3, 4, 5];

        stream.Write(data, 0, data.Length);

        Assert.AreEqual(5L, stream.Length);
    }

    /// <summary>
    /// Tests that Position returns the number of bytes written.
    /// </summary>
    [TestMethod]
    public void Position_Get_ReturnsBytesWritten()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());
        stream.Write([10, 20, 30]);

        Assert.AreEqual(3L, stream.Position);
    }

    /// <summary>
    /// Tests that setting Position throws NotSupportedException.
    /// </summary>
    [TestMethod]
    public void Position_Set_ThrowsNotSupportedException()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Position = 0);
    }

    /// <summary>
    /// Tests that Write(byte[], int, int) writes data correctly.
    /// </summary>
    [TestMethod]
    public void Write_ByteArrayOffsetCount_WritesCorrectData()
    {
        var writer = new ArrayBufferWriter<byte>();
        using var stream = new BufferWriterStream(writer);
        byte[] data = [10, 20, 30, 40, 50];

        stream.Write(data, 1, 3);

        Assert.AreEqual(3L, stream.Length);
        CollectionAssert.AreEqual(new byte[] { 20, 30, 40 }, writer.WrittenSpan.ToArray());
    }

    /// <summary>
    /// Tests that Write(byte[], int, int) throws when disposed.
    /// </summary>
    [TestMethod]
    public void Write_ByteArray_AfterDispose_ThrowsObjectDisposedException()
    {
        var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());
        stream.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => stream.Write([1], 0, 1));
    }

    /// <summary>
    /// Tests that Write(ReadOnlySpan) writes data correctly.
    /// </summary>
    [TestMethod]
    public void Write_Span_WritesCorrectData()
    {
        var writer = new ArrayBufferWriter<byte>();
        using var stream = new BufferWriterStream(writer);
        ReadOnlySpan<byte> data = [1, 2, 3];

        stream.Write(data);

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, writer.WrittenSpan.ToArray());
    }

    /// <summary>
    /// Tests that Write with empty span does not advance.
    /// </summary>
    [TestMethod]
    public void Write_EmptySpan_DoesNotAdvance()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());

        stream.Write(ReadOnlySpan<byte>.Empty);

        Assert.AreEqual(0L, stream.Length);
    }

    /// <summary>
    /// Tests that WriteByte writes a single byte.
    /// </summary>
    [TestMethod]
    public void WriteByte_WritesSingleByte()
    {
        var writer = new ArrayBufferWriter<byte>();
        using var stream = new BufferWriterStream(writer);

        stream.WriteByte(0xAB);

        Assert.AreEqual(1L, stream.Length);
        Assert.AreEqual(0xAB, writer.WrittenSpan[0]);
    }

    /// <summary>
    /// Tests that WriteByte throws when disposed.
    /// </summary>
    [TestMethod]
    public void WriteByte_AfterDispose_ThrowsObjectDisposedException()
    {
        var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());
        stream.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => stream.WriteByte(0x01));
    }

    /// <summary>
    /// Tests that WriteAsync(byte[], int, int, CancellationToken) writes correctly.
    /// </summary>
    [TestMethod]
    public async Task WriteAsync_ByteArray_WritesCorrectData()
    {
        var writer = new ArrayBufferWriter<byte>();
        using var stream = new BufferWriterStream(writer);
        byte[] data = [5, 6, 7];

        await stream.WriteAsync(data, 0, data.Length, CancellationToken.None);

        CollectionAssert.AreEqual(new byte[] { 5, 6, 7 }, writer.WrittenSpan.ToArray());
    }

    /// <summary>
    /// Tests that WriteAsync(ReadOnlyMemory, CancellationToken) writes correctly.
    /// </summary>
    [TestMethod]
    public async Task WriteAsync_ReadOnlyMemory_WritesCorrectData()
    {
        var writer = new ArrayBufferWriter<byte>();
        using var stream = new BufferWriterStream(writer);
        ReadOnlyMemory<byte> data = new byte[] { 8, 9 };

        await stream.WriteAsync(data);

        CollectionAssert.AreEqual(new byte[] { 8, 9 }, writer.WrittenSpan.ToArray());
    }

    /// <summary>
    /// Tests that WriteAsync throws when cancelled.
    /// </summary>
    [TestMethod]
    public async Task WriteAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            () => stream.WriteAsync(new byte[] { 1 }, 0, 1, cts.Token));
    }

    /// <summary>
    /// Tests that Read throws NotSupportedException.
    /// </summary>
    [TestMethod]
    public void Read_ThrowsNotSupportedException()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Read(new byte[1], 0, 1));
    }

    /// <summary>
    /// Tests that Seek throws NotSupportedException.
    /// </summary>
    [TestMethod]
    public void Seek_ThrowsNotSupportedException()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
    }

    /// <summary>
    /// Tests that SetLength throws NotSupportedException.
    /// </summary>
    [TestMethod]
    public void SetLength_ThrowsNotSupportedException()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());

        Assert.ThrowsExactly<NotSupportedException>(() => stream.SetLength(0));
    }

    /// <summary>
    /// Tests that Flush does not throw.
    /// </summary>
    [TestMethod]
    public void Flush_DoesNotThrow()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());

        stream.Flush();
    }

    /// <summary>
    /// Tests that FlushAsync completes successfully.
    /// </summary>
    [TestMethod]
    public async Task FlushAsync_CompletesSuccessfully()
    {
        using var stream = new BufferWriterStream(new ArrayBufferWriter<byte>());

        await stream.FlushAsync();
    }

    /// <summary>
    /// Tests that multiple writes accumulate correctly.
    /// </summary>
    [TestMethod]
    public void Write_MultipleWrites_AccumulatesLength()
    {
        var writer = new ArrayBufferWriter<byte>();
        using var stream = new BufferWriterStream(writer);

        stream.Write([1, 2]);
        stream.WriteByte(3);
        stream.Write([4, 5, 6]);

        Assert.AreEqual(6L, stream.Length);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6 }, writer.WrittenSpan.ToArray());
    }
}
