using Catharsis.Text.RegularExpressions;

namespace Catharsis.UnitTests.Text.RegularExpressions;

///<summary>
///Unit tests for the <see cref="MatchResult"/> struct.
///</summary>
[TestClass]
public class MatchResultTests
{
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        MatchResult result = new("hello", 5, 5);
        Assert.AreEqual("hello", result.Value);
        Assert.AreEqual(5, result.Index);
        Assert.AreEqual(5, result.Length);
    }

    [TestMethod]
    public void ToString_ReturnsFormattedString()
    {
        MatchResult result = new("test", 10, 4);
        Assert.AreEqual("'test' at 10 (length 4)", result.ToString());
    }

    [TestMethod]
    public void Equality_SameValues_AreEqual()
    {
        MatchResult a = new("abc", 0, 3);
        MatchResult b = new("abc", 0, 3);
        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void Equality_DifferentValues_AreNotEqual()
    {
        MatchResult a = new("abc", 0, 3);
        MatchResult b = new("xyz", 0, 3);
        Assert.AreNotEqual(a, b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Equality_DifferentIndex_AreNotEqual()
    {
        MatchResult a = new("abc", 0, 3);
        MatchResult b = new("abc", 5, 3);
        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Equality_DifferentLength_AreNotEqual()
    {
        MatchResult a = new("abc", 0, 3);
        MatchResult b = new("abc", 0, 4);
        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void GetHashCode_SameValues_SameHash()
    {
        MatchResult a = new("abc", 0, 3);
        MatchResult b = new("abc", 0, 3);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Default_HasDefaultValues()
    {
        MatchResult result = default;
        Assert.IsNull(result.Value);
        Assert.AreEqual(0, result.Index);
        Assert.AreEqual(0, result.Length);
    }
}
