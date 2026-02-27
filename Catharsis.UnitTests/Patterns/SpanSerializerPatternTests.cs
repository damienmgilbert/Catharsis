using Catharsis.Buffers;
using Catharsis.Patterns;

namespace Catharsis.UnitTests.Patterns;

[TestClass]
public class SpanSerializerPatternTests
{
    #region Public methods
    [TestMethod]
    public void GetSerializedSize_ReturnsExpected()
    {
        SpanSerializerPattern.SensorReading reading = new SpanSerializerPattern.SensorReading(0, 0, 0, 0);
        Assert.AreEqual(SpanSerializerPattern.SensorReading.SerializedSizeValue, reading.GetSerializedSize());
    }

    [TestMethod]
    public void SensorReading_RoundTrip()
    {
        SpanSerializerPattern.SensorReading reading = new SpanSerializerPattern.SensorReading(1, 25.5f, 60.0f, 1000L);

        using PooledBuffer<byte> buffer = new PooledBuffer<byte>(SpanSerializerPattern.SensorReading.SerializedSizeValue);
        reading.Serialize(buffer);

        SpanSerializerPattern.SensorReading deserialized = SpanSerializerPattern.SensorReading.Deserialize(buffer.WrittenSpan);

        Assert.AreEqual(1, deserialized.SensorId);
        Assert.AreEqual(25.5f, deserialized.Temperature);
        Assert.AreEqual(60.0f, deserialized.Humidity);
        Assert.AreEqual(1000L, deserialized.TimestampTicks);
    }

    [TestMethod]
    public void SerializeBatch_And_DeserializeBatch_RoundTrip()
    {
        SpanSerializerPattern.SensorReading[] readings =[ new(1, 20.0f, 50.0f, 100L), new(2, 30.0f, 70.0f, 200L), ];

        byte[] data = SpanSerializerPattern.SerializeBatch(readings);

        SpanSerializerPattern.SensorReading[] dest = new SpanSerializerPattern.SensorReading[2];
        int count = SpanSerializerPattern.DeserializeBatch(data, dest);

        Assert.AreEqual(2, count);
        Assert.AreEqual(1, dest[0].SensorId);
        Assert.AreEqual(2, dest[1].SensorId);
    }
    #endregion
}
