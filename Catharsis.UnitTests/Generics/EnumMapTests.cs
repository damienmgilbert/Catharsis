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
    public void Enumeration_YieldsMembersInValueOrder()
    {
        EnumMap<Color, int> map = new(static c => (int)c);
        CollectionAssert.AreEqual(new[] { Color.Red, Color.Green, Color.Blue }, map.Select(static p => p.Key).ToArray());
    }
}
