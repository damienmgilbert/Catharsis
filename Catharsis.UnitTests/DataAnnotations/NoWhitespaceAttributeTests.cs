using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="NoWhitespaceAttribute"/> class.
///</summary>
[TestClass]
public class NoWhitespaceAttributeTests
{
    #region Private methods
    private static ValidationContext CreateContext(string memberName) => new(new object()) { MemberName = memberName, DisplayName = memberName };
    #endregion

    #region Public methods

    [TestMethod]
    public void IsValid_Null_ReturnsSuccess()
    {
        NoWhitespaceAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(null, CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_EmptyString_ReturnsSuccess()
    {
        NoWhitespaceAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult("", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_NoWhitespace_ReturnsSuccess()
    {
        NoWhitespaceAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult("username", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_InteriorWhitespace_ReturnsFailure()
    {
        NoWhitespaceAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult("user name", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_LeadingWhitespace_ReturnsFailure()
    {
        NoWhitespaceAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult(" username", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_NonStringValue_ReturnsFailure()
    {
        NoWhitespaceAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult(42, CreateContext("Value")));
    }

    #endregion
}
