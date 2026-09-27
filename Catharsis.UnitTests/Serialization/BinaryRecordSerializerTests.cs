using Catharsis.Serialization;

namespace Catharsis.UnitTests.Serialization;

///<summary>
///Unit tests for the <see cref="BinaryRecordSerializer{T}"/> class.
///</summary>
[TestClass]
public class BinaryRecordSerializerTests
{
    #region Constructor / field discovery

    [TestMethod]
    public void Constructor_NoDecoratedProperties_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => new BinaryRecordSerializer<Undecorated>()); }

    #endregion

    #region Serialize

    [TestMethod]
    public void Serialize_NullRecord_Throws()
    {
        BinaryRecordSerializer<Point> serializer = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => serializer.Serialize(null!));
    }

    [TestMethod]
    public void Serialize_ProducesFixedLength()
    {
        BinaryRecordSerializer<Point> serializer = new();
        byte[] bytes = serializer.Serialize(new Point { X = 1, Y = 2 });

        Assert.AreEqual(8, bytes.Length);
    }

    #endregion

    #region Round-trip

    [TestMethod]
    public void RoundTrip_SerializeThenDeserialize_ProducesEquivalentRecord()
    {
        BinaryRecordSerializer<Point> serializer = new();
        Point original = new() { X = 42, Y = -7 };

        byte[] bytes = serializer.Serialize(original);
        Point restored = serializer.Deserialize(bytes);

        Assert.AreEqual(original.X, restored.X);
        Assert.AreEqual(original.Y, restored.Y);
    }

    [TestMethod]
    public void RoundTrip_HonorsFieldOrderRegardlessOfPropertyDeclarationOrder()
    {
        BinaryRecordSerializer<ReversedOrder> serializer = new();
        ReversedOrder original = new() { Second = 99, First = 5 };

        byte[] bytes = serializer.Serialize(original);
        ReversedOrder restored = serializer.Deserialize(bytes);

        Assert.AreEqual(5, restored.First);
        Assert.AreEqual(99, restored.Second);
    }

    [TestMethod]
    public void RoundTrip_MixedPrimitiveTypes_PreservesAllValues()
    {
        BinaryRecordSerializer<MixedRecord> serializer = new();
        MixedRecord original = new()
        {
            Flag = true,
            Small = 5,
            Medium = -123,
            Large = 42,
            Big = 123456789L,
            Ratio = 1.5f,
            Precise = 3.14159
        };

        byte[] bytes = serializer.Serialize(original);
        MixedRecord restored = serializer.Deserialize(bytes);

        Assert.AreEqual(original.Flag, restored.Flag);
        Assert.AreEqual(original.Small, restored.Small);
        Assert.AreEqual(original.Medium, restored.Medium);
        Assert.AreEqual(original.Large, restored.Large);
        Assert.AreEqual(original.Big, restored.Big);
        Assert.AreEqual(original.Ratio, restored.Ratio);
        Assert.AreEqual(original.Precise, restored.Precise);
    }

    #endregion

    private sealed class Undecorated
    {
        #region Public properties
        public int Value { get; set; }
        #endregion
    }

    private sealed class Point
    {
        #region Public properties
        [BinaryField(0)]
        public int X { get; set; }

        [BinaryField(1)]
        public int Y { get; set; }
        #endregion
    }

    private sealed class ReversedOrder
    {
        #region Public properties
        [BinaryField(1)]
        public int Second { get; set; }

        [BinaryField(0)]
        public int First { get; set; }
        #endregion
    }

    private sealed class MixedRecord
    {
        #region Public properties
        [BinaryField(4)]
        public long Big { get; set; }

        [BinaryField(0)]
        public bool Flag { get; set; }

        [BinaryField(2)]
        public int Medium { get; set; }

        [BinaryField(6)]
        public double Precise { get; set; }

        [BinaryField(5)]
        public float Ratio { get; set; }

        [BinaryField(3)]
        public byte Large { get; set; }

        [BinaryField(1)]
        public short Small { get; set; }
        #endregion
    }
}
