using Catharsis.Operators;

namespace Catharsis.UnitTests.Operators;

///<summary>
///Unit tests for the <see cref="SemanticVersion"/> struct.
///</summary>
[TestClass]
public class SemanticVersionTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NegativePart_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new SemanticVersion(-1, 0, 0)); }

    [TestMethod]
    public void Default_HasEmptyPrereleaseAndFormatsAsZero()
    {
        SemanticVersion version = default;
        Assert.AreEqual(string.Empty, version.Prerelease);
        Assert.AreEqual("0.0.0", version.ToString());
    }

    #endregion

    #region Parse

    [TestMethod]
    public void Parse_Release_ReadsParts()
    {
        SemanticVersion version = SemanticVersion.Parse("1.2.3");
        Assert.AreEqual(1, version[0]);
        Assert.AreEqual(2, version[1]);
        Assert.AreEqual(3, version[2]);
        Assert.IsFalse(version.IsPrerelease);
    }

    [TestMethod]
    public void Parse_Prerelease_KeepsLabelAndRoundTrips()
    {
        SemanticVersion version = SemanticVersion.Parse("1.2.3-beta");
        Assert.AreEqual("beta", version.Prerelease);
        Assert.AreEqual("1.2.3-beta", version.ToString());
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("1.2")]
    [DataRow("1.2.3.4")]
    [DataRow("a.b.c")]
    [DataRow("1.2.3-")]
    [DataRow("-1.2.3")]
    public void TryParse_Invalid_ReturnsFalse(string text) { Assert.IsFalse(SemanticVersion.TryParse(text, out _)); }

    [TestMethod]
    public void Parse_Invalid_Throws() { Assert.ThrowsExactly<FormatException>(static () => SemanticVersion.Parse("nope")); }

    #endregion

    #region Ordering

    [TestMethod]
    public void Compare_NumericPartsOrderedNumericallyNotLexically() { Assert.IsTrue(SemanticVersion.Parse("1.10.0") > SemanticVersion.Parse("1.9.0")); }

    [TestMethod]
    public void Compare_PrereleaseSortsBeforeRelease() { Assert.IsTrue(SemanticVersion.Parse("1.0.0-rc") < SemanticVersion.Parse("1.0.0")); }

    [TestMethod]
    public void Equality_SameVersion_Equal() { Assert.IsTrue(SemanticVersion.Parse("1.2.3") == new SemanticVersion(1, 2, 3)); }

    #endregion

    #region Bump

    [TestMethod]
    public void Bump_ResetsLowerPartsAndClearsPrerelease()
    {
        SemanticVersion version = SemanticVersion.Parse("1.2.3-beta");
        Assert.AreEqual("2.0.0", version.BumpMajor().ToString());
        Assert.AreEqual("1.3.0", version.BumpMinor().ToString());
        Assert.AreEqual("1.2.4", version.BumpPatch().ToString());
    }

    #endregion

    [TestMethod]
    public void Indexer_OutOfRange_Throws()
    {
        SemanticVersion version = new(1, 2, 3);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => version[3]);
    }
}
