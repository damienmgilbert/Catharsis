using Catharsis.Security;
using System.Text;

namespace Catharsis.UnitTests.Security;

///<summary>
///Unit tests for the <see cref="Base32Codec"/> class.
///</summary>
[TestClass]
public class Base32CodecTests
{
    #region Encode

    [TestMethod]
    public void Encode_EmptyInput_ReturnsEmptyString() { Assert.AreEqual(string.Empty, Base32Codec.Encode(ReadOnlySpan<byte>.Empty)); }

    [TestMethod]
    public void Encode_KnownVector_MatchesRfc4648Example()
    {
        Assert.AreEqual("MZXW6===", Base32Codec.Encode(Encoding.ASCII.GetBytes("foo")));
    }

    [TestMethod]
    public void Encode_ResultLength_IsMultipleOfEight()
    {
        string encoded = Base32Codec.Encode([1, 2, 3, 4, 5, 6, 7]);
        Assert.AreEqual(0, encoded.Length % 8);
    }

    #endregion

    #region Decode

    [TestMethod]
    public void Decode_NullInput_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => Base32Codec.Decode(null!)); }

    [TestMethod]
    public void Decode_KnownVector_MatchesRfc4648Example()
    {
        byte[] decoded = Base32Codec.Decode("MZXW6===");
        CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("foo"), decoded);
    }

    [TestMethod]
    public void Decode_LowercaseInput_IsCaseInsensitive()
    {
        byte[] decoded = Base32Codec.Decode("mzxw6===");
        CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("foo"), decoded);
    }

    [TestMethod]
    public void Decode_WithoutPadding_StillDecodesCorrectly()
    {
        byte[] decoded = Base32Codec.Decode("MZXW6");
        CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("foo"), decoded);
    }

    [TestMethod]
    public void Decode_InvalidCharacter_ThrowsFormatException() { Assert.ThrowsExactly<FormatException>(static () => Base32Codec.Decode("1nvalid!")); }

    #endregion

    #region Round-trip

    [TestMethod]
    public void RoundTrip_VariousLengths_ReturnsOriginalBytes()
    {
        for(int length = 0; length <= 20; length++)
        {
            byte[] original = new byte[length];
            for(int i = 0; i < length; i++)
            {
                original[i] = (byte)(i * 7);
            }

            string encoded = Base32Codec.Encode(original);
            byte[] decoded = Base32Codec.Decode(encoded);

            CollectionAssert.AreEqual(original, decoded);
        }
    }

    #endregion
}
