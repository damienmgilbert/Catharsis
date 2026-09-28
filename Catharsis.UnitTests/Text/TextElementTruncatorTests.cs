using Catharsis.Text;

namespace Catharsis.UnitTests.Text;

///<summary>
///Unit tests for the <see cref="TextElementTruncator"/> class.
///</summary>
[TestClass]
public class TextElementTruncatorTests
{
    #region Truncate

    [TestMethod]
    public void Truncate_NullValue_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => TextElementTruncator.Truncate(null!, 3)); }

    [TestMethod]
    public void Truncate_NegativeMaxTextElements_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => TextElementTruncator.Truncate("hello", -1)); }

    [TestMethod]
    public void Truncate_ShorterThanMax_ReturnsUnchanged()
    {
        Assert.AreEqual("hi", TextElementTruncator.Truncate("hi", 10));
    }

    [TestMethod]
    public void Truncate_LongerThanMax_ReturnsTruncated()
    {
        Assert.AreEqual("hello", TextElementTruncator.Truncate("hello world", 5));
    }

    [TestMethod]
    public void Truncate_ExactLength_ReturnsUnchanged()
    {
        Assert.AreEqual("hello", TextElementTruncator.Truncate("hello", 5));
    }

    [TestMethod]
    public void Truncate_ZeroMaxTextElements_ReturnsEmptyString()
    {
        Assert.AreEqual("", TextElementTruncator.Truncate("hello", 0));
    }

    [TestMethod]
    public void Truncate_DoesNotSplitCombiningSequence()
    {
        string combining = "éclair";
        string result = TextElementTruncator.Truncate(combining, 1);

        Assert.AreEqual("é", result);
    }

    [TestMethod]
    public void Truncate_DoesNotSplitSurrogatePair()
    {
        string withEmoji = "\U0001F600text";
        string result = TextElementTruncator.Truncate(withEmoji, 1);

        Assert.AreEqual("\U0001F600", result);
    }

    #endregion
}
