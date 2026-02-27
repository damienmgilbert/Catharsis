using Catharsis.Buffers;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Catharsis.Advanced.UnitTests;

[TestClass]
public class SequenceSliceTests
{
    [TestMethod]
    public void Constructor_SetsLength()
    {
        var data = new ReadOnlySequence<byte>(new byte[] { 1, 2, 3, 4, 5 });
        var slice = new SequenceSlice<byte>(in data);
        Assert.AreEqual(5, slice.Length);
        Assert.IsFalse(slice.IsEmpty);
    }

    [TestMethod]
    public void Empty_HasZeroLength()
    {
        var data = ReadOnlySequence<byte>.Empty;
        var slice = new SequenceSlice<byte>(in data);
        Assert.AreEqual(0, slice.Length);
        Assert.IsTrue(slice.IsEmpty);
    }

    [TestMethod]
    public void Slice_ReturnsSubRange()
    {
        var data = new ReadOnlySequence<byte>(new byte[] { 10, 20, 30, 40, 50 });
        var slice = new SequenceSlice<byte>(in data);
        var sub = slice.Slice(1, 3);
        Assert.AreEqual(3, sub.Length);
    }

    [TestMethod]
    public void ToArray_ReturnsAllData()
    {
        byte[] source = [1, 2, 3];
        var data = new ReadOnlySequence<byte>(source);
        var slice = new SequenceSlice<byte>(in data);
        CollectionAssert.AreEqual(source, slice.ToArray());
    }

    [TestMethod]
    public void FirstSpan_ReturnsData()
    {
        var data = new ReadOnlySequence<byte>(new byte[] { 42 });
        var slice = new SequenceSlice<byte>(in data);
        Assert.AreEqual(42, slice.FirstSpan[0]);
    }
}

[TestClass]
public class PooledUtf8StringTests
{
    [TestMethod]
    public void Create_FromString_RoundTrips()
    {
        using var utf8 = PooledUtf8String.Create("Hello");
        Assert.AreEqual("Hello", utf8.ToString());
        Assert.AreEqual(5, utf8.ByteLength);
    }

    [TestMethod]
    public void Create_FromCharSpan_RoundTrips()
    {
        using var utf8 = PooledUtf8String.Create("World".AsSpan());
        Assert.AreEqual("World", utf8.ToString());
    }

    [TestMethod]
    public void FromUtf8_Bytes_RoundTrips()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("Test");
        using var utf8 = PooledUtf8String.FromUtf8(bytes);
        Assert.AreEqual("Test", utf8.ToString());
        Assert.AreEqual(4, utf8.ByteLength);
    }

    [TestMethod]
    public void Span_ReturnsUtf8Bytes()
    {
        using var utf8 = PooledUtf8String.Create("A");
        Assert.AreEqual(1, utf8.Span.Length);
        Assert.AreEqual((byte)'A', utf8.Span[0]);
    }

    [TestMethod]
    public void Memory_ReturnsReadOnlyMemory()
    {
        using var utf8 = PooledUtf8String.Create("AB");
        Assert.AreEqual(2, utf8.Memory.Length);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var utf8 = PooledUtf8String.Create("X");
        utf8.Dispose();
        utf8.Dispose();
    }

    [TestMethod]
    public void Equals_SameContent_ReturnsTrue()
    {
        using var a = PooledUtf8String.Create("abc");
        using var b = PooledUtf8String.Create("abc");
        Assert.IsTrue(a.Equals(b));
    }

    [TestMethod]
    public void Equals_DifferentContent_ReturnsFalse()
    {
        using var a = PooledUtf8String.Create("abc");
        using var b = PooledUtf8String.Create("xyz");
        Assert.IsFalse(a.Equals(b));
    }
}

[TestClass]
public class PooledJsonDocumentTests
{
    [TestMethod]
    public void Parse_String_ReturnsDocument()
    {
        using var doc = PooledJsonDocument.Parse("""{"key":"value"}""");
        Assert.AreEqual(JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.AreEqual("value", doc.RootElement.GetProperty("key").GetString());
    }

    [TestMethod]
    public void Parse_Bytes_ReturnsDocument()
    {
        byte[] json = Encoding.UTF8.GetBytes("""{"n":42}""");
        using var doc = PooledJsonDocument.Parse(json.AsSpan());
        Assert.AreEqual(42, doc.RootElement.GetProperty("n").GetInt32());
    }

    [TestMethod]
    public async Task ParseAsync_Stream_ReturnsDocument()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""{"a":1}"""));
        using var doc = await PooledJsonDocument.ParseAsync(stream);
        Assert.AreEqual(1, doc.RootElement.GetProperty("a").GetInt32());
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var doc = PooledJsonDocument.Parse("""{"x":1}""");
        doc.Dispose();
        doc.Dispose();
    }
}

[TestClass]
public class StreamingSequenceReaderTests
{
    [TestMethod]
    public async Task ReadAllAsync_ReadsEntireStream()
    {
        using var reader = new StreamingSequenceReader();
        byte[] data = [1, 2, 3, 4, 5];
        using var stream = new MemoryStream(data);

        var sequence = await reader.ReadAllAsync(stream);

        Assert.AreEqual(5, sequence.Length);
        Assert.AreEqual(5, reader.TotalBytesRead);
    }

    [TestMethod]
    public async Task ReadAllAsync_EmptyStream()
    {
        using var reader = new StreamingSequenceReader();
        using var stream = new MemoryStream([]);

        var sequence = await reader.ReadAllAsync(stream);

        Assert.AreEqual(0, sequence.Length);
    }

    [TestMethod]
    public void Reset_ClearsTotalBytesRead()
    {
        using var reader = new StreamingSequenceReader();
        reader.Reset();
        Assert.AreEqual(0, reader.TotalBytesRead);
    }
}

[TestClass]
public class HighPerformanceSerializerTests
{
    private sealed class TestSerializable : ISequenceSerializable
    {
        private readonly byte[] _data;
        public TestSerializable(byte[] data) => _data = data;
        public int GetSerializedSize() => _data.Length;
        public void Serialize(IBufferWriter<byte> writer)
        {
            Span<byte> span = writer.GetSpan(_data.Length);
            _data.CopyTo(span);
            writer.Advance(_data.Length);
        }
    }

    [TestMethod]
    public void Serialize_ReturnsPooledArray()
    {
        using var serializer = new HighPerformanceSerializer();
        var obj = new TestSerializable([1, 2, 3]);

        byte[] result = serializer.Serialize(obj, out int bytesWritten);

        Assert.AreEqual(3, bytesWritten);
        Assert.AreEqual(1, result[0]);
        Assert.AreEqual(3, result[2]);
        serializer.ReturnArray(result);
    }

    [TestMethod]
    public void SerializeToMemoryOwner_ReturnsOwner()
    {
        using var serializer = new HighPerformanceSerializer();
        var obj = new TestSerializable([10, 20]);

        using var owner = serializer.SerializeToMemoryOwner(obj);

        Assert.AreEqual(2, owner.Length);
        Assert.AreEqual(10, owner.Span[0]);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var serializer = new HighPerformanceSerializer();
        serializer.Dispose();
        serializer.Dispose();
    }
}

[TestClass]
public class MemoryBackedChannelTests
{
    [TestMethod]
    public async Task WriteAsync_And_ReadAsync_RoundTrips()
    {
        using var channel = new MemoryBackedChannel<byte>();
        byte[] data = [1, 2, 3];

        await channel.WriteAsync(data);
        using var segment = await channel.ReadAsync();

        Assert.AreEqual(3, segment.Length);
        Assert.AreEqual(1, segment.Memory.Span[0]);
    }

    [TestMethod]
    public async Task Complete_And_ReadAll()
    {
        using var channel = new MemoryBackedChannel<byte>();
        await channel.WriteAsync(new byte[] { 10 });
        await channel.WriteAsync(new byte[] { 20 });
        channel.Complete();

        var segments = new List<byte>();
        while (channel.Reader.TryRead(out var seg))
        {
            segments.Add(seg.Memory.Span[0]);
            seg.Dispose();
        }

        CollectionAssert.AreEqual(new byte[] { 10, 20 }, segments);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var channel = new MemoryBackedChannel<byte>();
        channel.Dispose();
        channel.Dispose();
    }
}
