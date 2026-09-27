using Catharsis.Security;

namespace Catharsis.UnitTests.Security;

///<summary>
///Unit tests for the <see cref="SecureRandomToken"/> class.
///</summary>
[TestClass]
public class SecureRandomTokenTests
{
    #region Generate

    [TestMethod]
    public void Generate_NonPositiveByteLength_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => SecureRandomToken.Generate(0)); }

    [TestMethod]
    public void Generate_NegativeByteLength_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => SecureRandomToken.Generate(-1)); }

    [TestMethod]
    public void Generate_DefaultLength_ReturnsNonEmptyToken() { Assert.IsFalse(string.IsNullOrEmpty(SecureRandomToken.Generate())); }

    [TestMethod]
    public void Generate_ContainsOnlyUrlSafeCharacters()
    {
        string token = SecureRandomToken.Generate();
        Assert.IsTrue(token.All(static c => char.IsAsciiLetterOrDigit(c) || (c == '-') || (c == '_')));
    }

    [TestMethod]
    public void Generate_SuccessiveCalls_ProduceDifferentTokens()
    {
        string first = SecureRandomToken.Generate();
        string second = SecureRandomToken.Generate();
        Assert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void Generate_LargerByteLength_ProducesLongerToken()
    {
        string small = SecureRandomToken.Generate(8);
        string large = SecureRandomToken.Generate(64);
        Assert.IsTrue(large.Length > small.Length);
    }

    #endregion
}
