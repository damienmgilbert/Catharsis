using Catharsis.Text.RegularExpressions;

namespace Catharsis.UnitTests.Text.RegularExpressions;

///<summary>
///Unit tests for the <see cref="CommonPatterns"/> class.
///</summary>
[TestClass]
public class CommonPatternsTests
{
    #region Email
    [TestMethod]
    [DataRow("user@example.com", true)]
    [DataRow("USER@EXAMPLE.COM", true)]
    [DataRow("user.name+tag@sub.domain.org", true)]
    [DataRow("missing-at-sign.com", false)]
    [DataRow("@no-local.com", false)]
    [DataRow("user@ .com", false)]
    public void Email_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.Email().IsMatch(input));
    }
    #endregion

    #region Url
    [TestMethod]
    [DataRow("https://example.com", true)]
    [DataRow("http://sub.domain.org/path?q=1", true)]
    [DataRow("ftp://not-http.com", false)]
    [DataRow("example.com", false)]
    public void Url_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.Url().IsMatch(input));
    }
    #endregion

    #region IPv4
    [TestMethod]
    [DataRow("192.168.1.1", true)]
    [DataRow("0.0.0.0", true)]
    [DataRow("255.255.255.255", true)]
    [DataRow("256.1.1.1", false)]
    [DataRow("1.2.3", false)]
    [DataRow("abc.def.ghi.jkl", false)]
    public void IPv4_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.IPv4().IsMatch(input));
    }
    #endregion

    #region PhoneNumber
    [TestMethod]
    [DataRow("+1 (555) 123-4567", true)]
    [DataRow("555-1234", true)]
    [DataRow("12345", false)]
    public void PhoneNumber_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.PhoneNumber().IsMatch(input));
    }
    #endregion

    #region ZipCode
    [TestMethod]
    [DataRow("12345", true)]
    [DataRow("12345-6789", true)]
    [DataRow("1234", false)]
    [DataRow("123456", false)]
    public void ZipCode_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.ZipCode().IsMatch(input));
    }
    #endregion

    #region HexColor
    [TestMethod]
    [DataRow("#FF00AA", true)]
    [DataRow("FF00AA", true)]
    [DataRow("#FFF", true)]
    [DataRow("#FF00AA88", true)]
    [DataRow("#GGHHII", false)]
    [DataRow("12345", false)]
    public void HexColor_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.HexColor().IsMatch(input));
    }
    #endregion

    #region IsoDate
    [TestMethod]
    [DataRow("2024-01-15", true)]
    [DataRow("2024-1-15", false)]
    [DataRow("01-15-2024", false)]
    public void IsoDate_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.IsoDate().IsMatch(input));
    }
    #endregion

    #region IsoTime
    [TestMethod]
    [DataRow("14:30", true)]
    [DataRow("14:30:59", true)]
    [DataRow("2:30", false)]
    [DataRow("14:30:59.123", false)]
    public void IsoTime_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.IsoTime().IsMatch(input));
    }
    #endregion

    #region Guid
    [TestMethod]
    [DataRow("550e8400-e29b-41d4-a716-446655440000", true)]
    [DataRow("550e8400e29b41d4a716446655440000", false)]
    [DataRow("not-a-guid", false)]
    public void Guid_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.Guid().IsMatch(input));
    }
    #endregion

    #region Alphanumeric
    [TestMethod]
    [DataRow("Hello123", true)]
    [DataRow("ABC", true)]
    [DataRow("123", true)]
    [DataRow("Hello 123", false)]
    [DataRow("Hello!", false)]
    [DataRow("", false)]
    public void Alphanumeric_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.Alphanumeric().IsMatch(input));
    }
    #endregion

    #region SemanticVersion
    [TestMethod]
    [DataRow("1.0.0", true)]
    [DataRow("1.2.3-beta.1", true)]
    [DataRow("0.0.1+build.42", true)]
    [DataRow("1.2.3-alpha+meta", true)]
    [DataRow("1.2", false)]
    [DataRow("01.2.3", false)]
    public void SemanticVersion_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.SemanticVersion().IsMatch(input));
    }
    #endregion

    #region StrongPassword
    [TestMethod]
    [DataRow("P@ssw0rd!", true)]
    [DataRow("Str0ng!Pw", true)]
    [DataRow("weakpass", false)]
    [DataRow("NoDigit!", false)]
    [DataRow("n0upper!", false)]
    [DataRow("N0LOWER!", false)]
    [DataRow("Short1!", false)]
    public void StrongPassword_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.StrongPassword().IsMatch(input));
    }
    #endregion

    #region MacAddress
    [TestMethod]
    [DataRow("00:1A:2B:3C:4D:5E", true)]
    [DataRow("00-1A-2B-3C-4D-5E", true)]
    [DataRow("001A2B3C4D5E", false)]
    [DataRow("GG:HH:II:JJ:KK:LL", false)]
    public void MacAddress_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.MacAddress().IsMatch(input));
    }
    #endregion

    #region SocialSecurityNumber
    [TestMethod]
    [DataRow("123-45-6789", true)]
    [DataRow("123456789", false)]
    [DataRow("12-345-6789", false)]
    public void SocialSecurityNumber_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.SocialSecurityNumber().IsMatch(input));
    }
    #endregion

    #region CreditCard
    [TestMethod]
    [DataRow("4111111111111111", true)]
    [DataRow("4111-1111-1111-1111", true)]
    [DataRow("123", false)]
    public void CreditCard_MatchesExpected(string input, bool expected)
    {
        Assert.AreEqual(expected, CommonPatterns.CreditCard().IsMatch(input));
    }
    #endregion

    #region HtmlTag
    [TestMethod]
    public void HtmlTag_MatchesTags()
    {
        Assert.IsTrue(CommonPatterns.HtmlTag().IsMatch("<p>"));
        Assert.IsTrue(CommonPatterns.HtmlTag().IsMatch("</div>"));
        Assert.IsTrue(CommonPatterns.HtmlTag().IsMatch("<br/>"));
        Assert.IsTrue(CommonPatterns.HtmlTag().IsMatch("<img src=\"test.png\"/>"));
        Assert.IsFalse(CommonPatterns.HtmlTag().IsMatch("no tags here"));
    }
    #endregion

    #region Whitespace
    [TestMethod]
    public void Whitespace_MatchesConsecutiveSpaces()
    {
        Assert.IsTrue(CommonPatterns.Whitespace().IsMatch("hello world"));
        Assert.IsTrue(CommonPatterns.Whitespace().IsMatch("tab\there"));
        Assert.IsFalse(CommonPatterns.Whitespace().IsMatch("nospaces"));
    }
    #endregion
}
