using Catharsis.Text;

namespace Catharsis.UnitTests.Text;

///<summary>
///Unit tests for the <see cref="SafeJsEncoder"/> class.
///</summary>
[TestClass]
public class SafeJsEncoderTests
{
    #region Encode

    [TestMethod]
    public void Encode_NullValue_Throws()
    {
        SafeJsEncoder encoder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => encoder.Encode(null!));
    }

    [TestMethod]
    public void Encode_PlainText_ReturnsUnchanged()
    {
        SafeJsEncoder encoder = new();
        Assert.AreEqual("hello", encoder.Encode("hello"));
    }

    [TestMethod]
    public void Encode_QuoteCharacter_IsEscaped()
    {
        SafeJsEncoder encoder = new();
        string result = encoder.Encode("say \"hi\"");

        Assert.IsFalse(result.Contains('"'));
        StringAssert.Contains(result, "hi");
    }

    [TestMethod]
    public void Encode_IsUsableAsITextEncoder()
    {
        ITextEncoder encoder = new SafeJsEncoder();
        Assert.IsFalse(encoder.Encode("</script>").Contains('<'));
    }

    #endregion
}
