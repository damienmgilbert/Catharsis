using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Catharsis.Buffers;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Patterns;

/// <summary>
/// Demonstrates a span-based serializer/deserializer pattern for unmanaged types,
/// using <see cref="SpanReader"/>, <see cref="SpanWriter"/>, and <see cref="PooledBuffer{T}"/>.
/// </summary>
public static class SpanSerializerPattern
{
    /// <summary>
    /// A sample data structure demonstrating span-based round-trip serialization.
    /// </summary>
    public readonly record struct SensorReading(int SensorId, float Temperature, float Humidity, long TimestampTicks)
        : ISequenceSerializable
    {
        /// <summary>The fixed serialized size of a sensor reading.</summary>
        public const int SerializedSizeValue = sizeof(int) + sizeof(float) + sizeof(float) + sizeof(long);

        /// <inheritdoc />
        public void Serialize(IBufferWriter<byte> writer)
        {
            Guard.IsNotNull(writer);

            Span<byte> span = writer.GetSpan(SerializedSizeValue);
            var w = new SpanWriter(span);
            w.WriteInt32LittleEndian(SensorId);
            w.WriteSingleLittleEndian(Temperature);
            w.WriteSingleLittleEndian(Humidity);
            w.WriteInt64LittleEndian(TimestampTicks);
            writer.Advance(SerializedSizeValue);
        }

        /// <inheritdoc />
        public int GetSerializedSize() => SerializedSizeValue;

        /// <summary>
        /// Deserializes a <see cref="SensorReading"/> from a byte span.
        /// </summary>
        /// <param name="data">The raw bytes.</param>
        /// <returns>The deserialized reading.</returns>
        public static SensorReading Deserialize(ReadOnlySpan<byte> data)
        {
            Guard.IsGreaterThanOrEqualTo(data.Length, SerializedSizeValue);

            var reader = new SpanReader(data);
            int sensorId = reader.ReadInt32LittleEndian();
            float temperature = reader.ReadSingleLittleEndian();
            float humidity = reader.ReadSingleLittleEndian();
            long timestamp = reader.ReadInt64LittleEndian();

            return new SensorReading(sensorId, temperature, humidity, timestamp);
        }
    }

    /// <summary>
    /// Serializes a batch of sensor readings into a pooled buffer.
    /// </summary>
    /// <param name="readings">The readings to serialize.</param>
    /// <returns>A byte array containing the serialized readings.</returns>
    public static byte[] SerializeBatch(ReadOnlySpan<SensorReading> readings)
    {
        using var buffer = new PooledBuffer<byte>(readings.Length * SensorReading.SerializedSizeValue);

        foreach (SensorReading reading in readings)
            reading.Serialize(buffer);

        return buffer.WrittenSpan.ToArray();
    }

    /// <summary>
    /// Deserializes a batch of sensor readings from raw bytes.
    /// </summary>
    /// <param name="data">The serialized data.</param>
    /// <param name="destination">The destination span for deserialized readings.</param>
    /// <returns>The number of readings deserialized.</returns>
    public static int DeserializeBatch(ReadOnlySpan<byte> data, Span<SensorReading> destination)
    {
        int count = 0;
        int offset = 0;

        while (offset + SensorReading.SerializedSizeValue <= data.Length && count < destination.Length)
        {
            destination[count++] = SensorReading.Deserialize(data[offset..]);
            offset += SensorReading.SerializedSizeValue;
        }

        return count;
    }
}
