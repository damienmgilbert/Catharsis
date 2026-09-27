using Catharsis.Common;

namespace Catharsis.UnitTests.Common;

///<summary>
///Unit tests for the <see cref="EnumCache{TEnum}"/> class.
///</summary>
[TestClass]
public class EnumCacheTests
{
    #region Values / Names

    [TestMethod]
    public void Values_ReturnsAllDefinedMembers()
    {
        IReadOnlyList<Color> values = EnumCache<Color>.Values;
        CollectionAssert.AreEquivalent(new[] { Color.Red, Color.Green, Color.Blue }, values.ToList());
    }

    [TestMethod]
    public void Names_ReturnsAllDefinedMemberNames()
    {
        IReadOnlyList<string> names = EnumCache<Color>.Names;
        CollectionAssert.AreEquivalent(new[] { "Red", "Green", "Blue" }, names.ToList());
    }

    #endregion

    #region TryParse

    [TestMethod]
    public void TryParse_NullName_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => EnumCache<Color>.TryParse(null!, out _)); }

    [TestMethod]
    public void TryParse_DefinedName_ReturnsTrueWithValue()
    {
        bool found = EnumCache<Color>.TryParse("Green", out Color value);

        Assert.IsTrue(found);
        Assert.AreEqual(Color.Green, value);
    }

    [TestMethod]
    public void TryParse_UndefinedName_ReturnsFalse()
    {
        bool found = EnumCache<Color>.TryParse("Purple", out _);
        Assert.IsFalse(found);
    }

    [TestMethod]
    public void TryParse_CaseMismatch_ReturnsFalse()
    {
        bool found = EnumCache<Color>.TryParse("green", out _);
        Assert.IsFalse(found);
    }

    #endregion

    private enum Color
    {
        Red,
        Green,
        Blue
    }
}
