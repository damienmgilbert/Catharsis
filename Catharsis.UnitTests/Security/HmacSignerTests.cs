using Catharsis.Security;
using System.Text;

namespace Catharsis.UnitTests.Security;

///<summary>
///Unit tests for the <see cref="HmacSigner"/> class.
///</summary>
[TestClass]
public class HmacSignerTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullKey_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new HmacSigner(null!)); }

    [TestMethod]
    public void Constructor_EmptyKey_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new HmacSigner([])); }

    #endregion

    #region Sign

    [TestMethod]
    public void Sign_ReturnsThirtyTwoByteSignature()
    {
        HmacSigner signer = new("secret-key"u8.ToArray());
        Assert.HasCount(32, signer.Sign("data"u8));
    }

    [TestMethod]
    public void Sign_SameDataAndKey_ProducesSameSignature()
    {
        HmacSigner signer = new("secret-key"u8.ToArray());
        CollectionAssert.AreEqual(signer.Sign("data"u8), signer.Sign("data"u8));
    }

    [TestMethod]
    public void Sign_DifferentData_ProducesDifferentSignatures()
    {
        HmacSigner signer = new("secret-key"u8.ToArray());
        CollectionAssert.AreNotEqual(signer.Sign("data-a"u8), signer.Sign("data-b"u8));
    }

    [TestMethod]
    public void Sign_DifferentKeys_ProduceDifferentSignatures()
    {
        HmacSigner signerA = new(Encoding.UTF8.GetBytes("key-a"));
        HmacSigner signerB = new(Encoding.UTF8.GetBytes("key-b"));

        CollectionAssert.AreNotEqual(signerA.Sign("data"u8), signerB.Sign("data"u8));
    }

    #endregion

    #region Verify

    [TestMethod]
    public void Verify_CorrectSignature_ReturnsTrue()
    {
        HmacSigner signer = new("secret-key"u8.ToArray());
        byte[] signature = signer.Sign("data"u8);

        Assert.IsTrue(signer.Verify("data"u8, signature));
    }

    [TestMethod]
    public void Verify_TamperedData_ReturnsFalse()
    {
        HmacSigner signer = new("secret-key"u8.ToArray());
        byte[] signature = signer.Sign("data"u8);

        Assert.IsFalse(signer.Verify("tampered"u8, signature));
    }

    [TestMethod]
    public void Verify_WrongSignature_ReturnsFalse()
    {
        HmacSigner signer = new("secret-key"u8.ToArray());
        Assert.IsFalse(signer.Verify("data"u8, new byte[32]));
    }

    [TestMethod]
    public void Verify_SignatureFromDifferentKey_ReturnsFalse()
    {
        HmacSigner signerA = new("key-a"u8.ToArray());
        HmacSigner signerB = new("key-b"u8.ToArray());

        byte[] signature = signerA.Sign("data"u8);

        Assert.IsFalse(signerB.Verify("data"u8, signature));
    }

    #endregion
}
