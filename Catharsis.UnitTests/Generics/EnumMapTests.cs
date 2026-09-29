using Catharsis.Generics;

namespace Catharsis.UnitTests.Generics;

///<summary>
///Unit tests for the <see cref="EnumMap{TEnum, TValue}"/> class.
///</summary>
[TestClass]
public class EnumMapTests
{
    enum Color { Red = 10, Green = 20, Blue = 30 }

    enum WithAlias { A = 1, B = 2, Alias = 1 }

    enum Negative { Low = -5, Zero = 0, High = 5 }

    enum Contiguous { A = -2, B, C, D }

    enum Small : byte { Lo = 0, Hi = 255 }

    enum Huge : ulong { Small = 1, Big = ulong.MaxValue }

    enum Signed64 : long { Min = long.MinValue, Max = long.MaxValue }

    [TestMethod]
    public void Count_EqualsDistinctMembers()
    {
        Assert.AreEqual(3, new EnumMap<Color, int>().Count);
        Assert.AreEqual(2, new EnumMap<WithAlias, int>().Count);
    }

    [TestMethod]
    public void Indexer_DefaultsThenStoresPerMember()
    {
        EnumMap<Color, string?> map = new();
        Assert.IsNull(map[Color.Green]);

        map[Color.Green] = "g";

        Assert.AreEqual("g", map[Color.Green]);
        Assert.IsNull(map[Color.Red]);
    }

    [TestMethod]
    public void Indexer_UndefinedValue_Throws()
    {
        EnumMap<Color, int> map = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => map[(Color)99]);
    }

    [TestMethod]
    public void Initializer_FillsEveryMember()
    {
        EnumMap<Color, string> map = new(static c => c.ToString());
        Assert.AreEqual("Blue", map[Color.Blue]);
    }

    [TestMethod]
    public void Constructor_NullInitializer_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new EnumMap<Color, int>(null!)); }

    [TestMethod]
    public void Alias_SharesSlotWithCanonicalMember()
    {
        EnumMap<WithAlias, int> map = new();
        map[WithAlias.A] = 5;

        Assert.AreEqual(5, map[WithAlias.Alias]);
    }

    [TestMethod]
    public void NonContiguousSignedValues_RoundTrip()
    {
        EnumMap<Negative, int> map = new(static v => (int)v);

        Assert.AreEqual(-5, map[Negative.Low]);
        Assert.AreEqual(0, map[Negative.Zero]);
        Assert.AreEqual(5, map[Negative.High]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => map[(Negative)1]);
    }

    [TestMethod]
    public void ContiguousRangeStartingBelowZero_UsesDirectIndex()
    {
        EnumMap<Contiguous, string> map = new(static v => v.ToString());

        Assert.AreEqual("A", map[Contiguous.A]);
        Assert.AreEqual("D", map[Contiguous.D]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => map[(Contiguous)(-3)]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => map[(Contiguous)2]);
    }

    [TestMethod]
    public void ByteBackedEnum_DoesNotSignExtend()
    {
        EnumMap<Small, int> map = new(static v => (int)v);

        Assert.AreEqual(255, map[Small.Hi]);
        Assert.AreEqual(0, map[Small.Lo]);
    }

    [TestMethod]
    public void UInt64BackedEnum_HandlesValuesAboveInt64Max()
    {
        EnumMap<Huge, string> map = new(static v => v.ToString());

        Assert.AreEqual("Big", map[Huge.Big]);
        Assert.AreEqual("Small", map[Huge.Small]);
        Assert.AreEqual(2, map.Count);
    }

    [TestMethod]
    public void Int64Extremes_DoNotOverflowIndexing()
    {
        EnumMap<Signed64, int> map = new(static v => v == Signed64.Min ? 1 : 2);

        Assert.AreEqual(1, map[Signed64.Min]);
        Assert.AreEqual(2, map[Signed64.Max]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => map[(Signed64)0]);
    }

    [TestMethod]
    public void Enumeration_YieldsMembersInValueOrder()
    {
        EnumMap<Color, int> map = new(static c => (int)c);
        CollectionAssert.AreEqual(new[] { Color.Red, Color.Green, Color.Blue }, map.Select(static p => p.Key).ToArray());
    }
}
