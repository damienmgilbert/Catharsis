using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="CreditCardLuhnAttribute"/> class.
///</summary>
[TestClass]
public class CreditCardLuhnAttributeTests
{
    #region Private methods
    private static ValidationContext CreateContext(string memberName) => new(new object()) { MemberName = memberName, DisplayName = memberName };
    #endregion

    #region Public methods

    [TestMethod]
    public void IsValid_Null_ReturnsSuccess()
    {
        CreditCardLuhnAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(null, CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_EmptyString_ReturnsSuccess()
    {
        CreditCardLuhnAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult("", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_ValidNumber_ReturnsSuccess()
    {
        CreditCardLuhnAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult("4111111111111111", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_ValidNumberWithSpaces_ReturnsSuccess()
    {
        CreditCardLuhnAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult("4111 1111 1111 1111", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_ValidNumberWithHyphens_ReturnsSuccess()
    {
        CreditCardLuhnAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult("4111-1111-1111-1111", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_InvalidChecksum_ReturnsFailure()
    {
        CreditCardLuhnAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult("4111111111111112", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_ContainsLetters_ReturnsFailure()
    {
        CreditCardLuhnAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult("4111A111111111", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_TooFewDigits_ReturnsFailure()
    {
        CreditCardLuhnAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult("5", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_NonStringValue_ReturnsFailure()
    {
        CreditCardLuhnAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult(4111111111111111L, CreateContext("Value")));
    }

    #endregion
}
