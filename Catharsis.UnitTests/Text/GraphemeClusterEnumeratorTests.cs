using Catharsis.Text;

namespace Catharsis.UnitTests.Text;

///<summary>
///Unit tests for the <see cref="GraphemeClusterEnumerator"/> class.
///</summary>
[TestClass]
public class GraphemeClusterEnumeratorTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullValue_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new GraphemeClusterEnumerator(null!)); }

    #endregion

    #region GetEnumerator

    [TestMethod]
    public void GetEnumerator_SimpleAsciiString_YieldsOneCharacterPerCluster()
    {
        GraphemeClusterEnumerator enumerator = new("abc");
        List<string> clusters = [.. enumerator];

        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, clusters);
    }

    [TestMethod]
    public void GetEnumerator_EmptyString_YieldsNoClusters()
    {
        GraphemeClusterEnumerator enumerator = new("");
        List<string> clusters = [.. enumerator];

        Assert.IsEmpty(clusters);
    }

    [TestMethod]
    public void GetEnumerator_CombiningMark_KeepsBaseAndMarkTogether()
    {
        string combining = "é";
        GraphemeClusterEnumerator enumerator = new(combining);
        List<string> clusters = [.. enumerator];

        CollectionAssert.AreEqual(new[] { combining }, clusters);
    }

    [TestMethod]
    public void GetEnumerator_SurrogatePairEmoji_YieldsSingleCluster()
    {
        string emoji = "\U0001F600";
        GraphemeClusterEnumerator enumerator = new(emoji);
        List<string> clusters = [.. enumerator];

        CollectionAssert.AreEqual(new[] { emoji }, clusters);
    }

    #endregion

    #region Count

    [TestMethod]
    public void Count_AsciiString_MatchesCharacterCount()
    {
        GraphemeClusterEnumerator enumerator = new("hello");
        Assert.AreEqual(5, enumerator.Count);
    }

    [TestMethod]
    public void Count_SurrogatePairEmoji_IsOneDespiteTwoUtf16CodeUnits()
    {
        string emoji = "\U0001F600";
        GraphemeClusterEnumerator enumerator = new(emoji);

        Assert.AreEqual(2, emoji.Length);
        Assert.AreEqual(1, enumerator.Count);
    }

    #endregion
}
