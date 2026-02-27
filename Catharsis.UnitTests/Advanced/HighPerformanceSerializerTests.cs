using System.Buffers;
using Catharsis;
using Catharsis.Advanced;
using Catharsis.Buffers;
using CommunityToolkit.HighPerformance.Buffers;

namespace Catharsis.UnitTests.Advanced;

[TestClass]
public class HighPerformanceSerializerTests
{
    sealed class TestSerializable : ISequenceSerializable
    {
        readonly byte[] _data;
        public TestSerializable(byte[] data)
        {
            _data = data;
        }

        public int GetSerializedSize()
        {
            return _data.Length;
        }
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
        using HighPerformanceSerializer serializer = new HighPerformanceSerializer();
        TestSerializable obj = new TestSerializable([1, 2, 3]);

        byte[] result = serializer.Serialize(obj, out int bytesWritten);

        Assert.AreEqual(3, bytesWritten);
        Assert.AreEqual(1, result[0]);
        Assert.AreEqual(3, result[2]);
        serializer.ReturnArray(result);
    }

    [TestMethod]
    public void SerializeToMemoryOwner_ReturnsOwner()
    {
        using HighPerformanceSerializer serializer = new HighPerformanceSerializer();
        TestSerializable obj = new TestSerializable([10, 20]);

        using MemoryOwner<byte> owner = serializer.SerializeToMemoryOwner(obj);

        Assert.AreEqual(2, owner.Length);
        Assert.AreEqual(10, owner.Span[0]);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        HighPerformanceSerializer serializer = new HighPerformanceSerializer();
        serializer.Dispose();
        serializer.Dispose();
    }
}
