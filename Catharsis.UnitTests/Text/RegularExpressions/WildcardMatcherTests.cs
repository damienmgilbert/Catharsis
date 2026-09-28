using Catharsis.Text.RegularExpressions;

namespace Catharsis.UnitTests.Text.RegularExpressions;

///<summary>
///Unit tests for the <see cref="WildcardMatcher"/> class.
///</summary>
[TestClass]
public class WildcardMatcherTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullPattern_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new WildcardMatcher(null!)); }

    #endregion

    #region IsMatch

    [TestMethod]
    public void IsMatch_NullInput_Throws()
    {
        WildcardMatcher matcher = new("*.txt");
        Assert.ThrowsExactly<ArgumentNullException>(() => matcher.IsMatch(null!));
    }

    [TestMethod]
    public void IsMatch_StarMatchesAnyRun()
    {
        WildcardMatcher matcher = new("*.txt");
        Assert.IsTrue(matcher.IsMatch("report.txt"));
        Assert.IsTrue(matcher.IsMatch(".txt"));
        Assert.IsFalse(matcher.IsMatch("report.csv"));
    }

    [TestMethod]
    public void IsMatch_QuestionMarkMatchesExactlyOneCharacter()
    {
        WildcardMatcher matcher = new("file?.txt");
        Assert.IsTrue(matcher.IsMatch("file1.txt"));
        Assert.IsFalse(matcher.IsMatch("file12.txt"));
        Assert.IsFalse(matcher.IsMatch("file.txt"));
    }

    [TestMethod]
    public void IsMatch_LiteralCharactersAreEscaped()
    {
        WildcardMatcher matcher = new("a.b");
        Assert.IsFalse(matcher.IsMatch("axb"));
        Assert.IsTrue(matcher.IsMatch("a.b"));
    }

    [TestMethod]
    public void IsMatch_CaseSensitiveByDefault()
    {
        WildcardMatcher matcher = new("*.TXT");
        Assert.IsFalse(matcher.IsMatch("report.txt"));
    }

    [TestMethod]
    public void IsMatch_IgnoreCase_MatchesRegardlessOfCase()
    {
        WildcardMatcher matcher = new("*.TXT", ignoreCase: true);
        Assert.IsTrue(matcher.IsMatch("report.txt"));
    }

    [TestMethod]
    public void IsMatch_MustMatchEntireString()
    {
        WildcardMatcher matcher = new("abc");
        Assert.IsFalse(matcher.IsMatch("xabcx"));
    }

    #endregion
}
