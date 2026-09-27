using Catharsis.Security;

namespace Catharsis.UnitTests.Security;

///<summary>
///Unit tests for the <see cref="ConstantTimeComparer"/> class.
///</summary>
[TestClass]
public class ConstantTimeComparerTests
{
    #region Equals (bytes)

    [TestMethod]
    public void Equals_Bytes_IdenticalSequences_ReturnsTrue()
    {
        byte[] left = [1, 2, 3];
        byte[] right = [1, 2, 3];
        Assert.IsTrue(ConstantTimeComparer.Equals(left, right));
    }

    [TestMethod]
    public void Equals_Bytes_DifferentContent_ReturnsFalse()
    {
        byte[] left = [1, 2, 3];
        byte[] right = [1, 2, 4];
        Assert.IsFalse(ConstantTimeComparer.Equals(left, right));
    }

    [TestMethod]
    public void Equals_Bytes_DifferentLength_ReturnsFalse()
    {
        byte[] left = [1, 2, 3];
        byte[] right = [1, 2];
        Assert.IsFalse(ConstantTimeComparer.Equals(left, right));
    }

    [TestMethod]
    public void Equals_Bytes_BothEmpty_ReturnsTrue()
    {
        Assert.IsTrue(ConstantTimeComparer.Equals(ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty));
    }

    #endregion

    #region Equals (strings)

    [TestMethod]
    public void Equals_Strings_NullLeft_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => ConstantTimeComparer.Equals(null!, "a")); }

    [TestMethod]
    public void Equals_Strings_NullRight_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => ConstantTimeComparer.Equals("a", null!)); }

    [TestMethod]
    public void Equals_Strings_IdenticalStrings_ReturnsTrue() { Assert.IsTrue(ConstantTimeComparer.Equals("secret-token", "secret-token")); }

    [TestMethod]
    public void Equals_Strings_DifferentStrings_ReturnsFalse() { Assert.IsFalse(ConstantTimeComparer.Equals("secret-token", "wrong-token")); }

    [TestMethod]
    public void Equals_Strings_CaseSensitive() { Assert.IsFalse(ConstantTimeComparer.Equals("Token", "token")); }

    #endregion
}
