using Catharsis.Text;

namespace Catharsis.UnitTests.Text;

///<summary>
///Unit tests for the <see cref="SafeHtmlEncoder"/> class.
///</summary>
[TestClass]
public class SafeHtmlEncoderTests
{
    #region Encode

    [TestMethod]
    public void Encode_NullValue_Throws()
    {
        SafeHtmlEncoder encoder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => encoder.Encode(null!));
    }

    [TestMethod]
    public void Encode_PlainText_ReturnsUnchanged()
    {
        SafeHtmlEncoder encoder = new();
        Assert.AreEqual("hello", encoder.Encode("hello"));
    }

    [TestMethod]
    public void Encode_HtmlSpecialCharacters_AreEscaped()
    {
        SafeHtmlEncoder encoder = new();
        string result = encoder.Encode("<script>");

        StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex("[<>]"));
        StringAssert.Contains(result, "script");
    }

    [TestMethod]
    public void Encode_IsUsableAsITextEncoder()
    {
        ITextEncoder encoder = new SafeHtmlEncoder();
        Assert.IsFalse(encoder.Encode("<b>").Contains('<'));
    }

    #endregion
}
