using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="StringExtensions"/> class.
///</summary>
[TestClass]
public class StringExtensionsTests
{
    #region Capitalize

    [TestMethod]
    public void Capitalize_NullValue_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((string)null!).Capitalize());
    }

    [TestMethod]
    public void Capitalize_LowercaseFirstLetter_UppercasesIt()
    {
        Assert.AreEqual("Hello", "hello".Capitalize());
    }

    [TestMethod]
    public void Capitalize_EmptyString_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, "".Capitalize());
    }

    [TestMethod]
    public void Capitalize_AlreadyCapitalized_Unchanged()
    {
        Assert.AreEqual("Hello", "Hello".Capitalize());
    }

    #endregion

    #region OrDefault

    [TestMethod]
    public void OrDefault_NullFallback_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => "value".OrDefault(null!));
    }

    [TestMethod]
    public void OrDefault_NonWhitespaceValue_ReturnsValue()
    {
        Assert.AreEqual("value", "value".OrDefault("fallback"));
    }

    [TestMethod]
    public void OrDefault_NullValue_ReturnsFallback()
    {
        Assert.AreEqual("fallback", ((string?)null).OrDefault("fallback"));
    }

    [TestMethod]
    public void OrDefault_WhitespaceValue_ReturnsFallback()
    {
        Assert.AreEqual("fallback", "   ".OrDefault("fallback"));
    }

    #endregion

    #region Slugify

    [TestMethod]
    public void Slugify_NullValue_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((string)null!).Slugify());
    }

    [TestMethod]
    public void Slugify_MixedCaseWithPunctuation_ProducesLowercaseHyphenatedSlug()
    {
        Assert.AreEqual("hello-world", "Hello, World!".Slugify());
    }

    [TestMethod]
    public void Slugify_LeadingAndTrailingPunctuation_IsTrimmed()
    {
        Assert.AreEqual("middle", "  --Middle--  ".Slugify());
    }

    [TestMethod]
    public void Slugify_EmptyString_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, "".Slugify());
    }

    [TestMethod]
    public void Slugify_OnlyPunctuation_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, "!!!".Slugify());
    }

    #endregion

    #region Truncate

    [TestMethod]
    public void Truncate_NullValue_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((string)null!).Truncate(5));
    }

    [TestMethod]
    public void Truncate_NegativeMaxLength_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => "hello".Truncate(-1));
    }

    [TestMethod]
    public void Truncate_ShorterThanMax_ReturnsUnchanged()
    {
        Assert.AreEqual("hi", "hi".Truncate(10));
    }

    [TestMethod]
    public void Truncate_LongerThanMax_AppendsEllipsis()
    {
        Assert.AreEqual("he...", "hello world".Truncate(5));
    }

    [TestMethod]
    public void Truncate_CustomEllipsis_IsUsed()
    {
        Assert.AreEqual("hell~", "hello world".Truncate(5, "~"));
    }

    [TestMethod]
    public void Truncate_MaxLengthShorterThanEllipsis_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => "hello world".Truncate(2, "..."));
    }

    #endregion
}
