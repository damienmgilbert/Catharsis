using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="SpanExtensions"/> class.
///</summary>
[TestClass]
public class SpanExtensionsTests
{
    #region ContainsAnyOf

    [TestMethod]
    public void ContainsAnyOf_NullValues_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => "hello".AsSpan().ContainsAnyOf(null!));
    }

    [TestMethod]
    public void ContainsAnyOf_MatchingValue_ReturnsTrue()
    {
        Assert.IsTrue("hello world".AsSpan().ContainsAnyOf("xyz", "world"));
    }

    [TestMethod]
    public void ContainsAnyOf_NoMatch_ReturnsFalse()
    {
        Assert.IsFalse("hello world".AsSpan().ContainsAnyOf("xyz", "abc"));
    }

    [TestMethod]
    public void ContainsAnyOf_NoValues_ReturnsFalse()
    {
        Assert.IsFalse("hello".AsSpan().ContainsAnyOf());
    }

    #endregion

    #region CountOccurrences

    [TestMethod]
    public void CountOccurrences_MultipleMatches_ReturnsCount()
    {
        Assert.AreEqual(3, "banana".AsSpan().CountOccurrences('a'));
    }

    [TestMethod]
    public void CountOccurrences_NoMatches_ReturnsZero()
    {
        Assert.AreEqual(0, "banana".AsSpan().CountOccurrences('z'));
    }

    [TestMethod]
    public void CountOccurrences_EmptySpan_ReturnsZero()
    {
        Assert.AreEqual(0, "".AsSpan().CountOccurrences('a'));
    }

    #endregion

    #region Truncate

    [TestMethod]
    public void Truncate_NegativeMaxLength_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => "hello".AsSpan().Truncate(-1));
    }

    [TestMethod]
    public void Truncate_ShorterThanMax_ReturnsUnchanged()
    {
        ReadOnlySpan<char> result = "hi".AsSpan().Truncate(10);
        Assert.AreEqual("hi", result.ToString());
    }

    [TestMethod]
    public void Truncate_LongerThanMax_ReturnsSlice()
    {
        ReadOnlySpan<char> result = "hello world".AsSpan().Truncate(5);
        Assert.AreEqual("hello", result.ToString());
    }

    #endregion
}
