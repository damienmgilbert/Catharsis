using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="FutureDateAttribute"/> class.
///</summary>
[TestClass]
public class FutureDateAttributeTests
{
    #region Private methods
    private static ValidationContext CreateContext(string memberName) => new(new object()) { MemberName = memberName, DisplayName = memberName };
    #endregion

    #region Public methods

    [TestMethod]
    public void IsValid_Null_ReturnsSuccess()
    {
        FutureDateAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(null, CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_NonDateValue_ReturnsFailure()
    {
        FutureDateAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult("not a date", CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DateTimeInFuture_ReturnsSuccess()
    {
        FutureDateAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(DateTime.Now.AddDays(1), CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DateTimeInPast_ReturnsFailure()
    {
        FutureDateAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult(DateTime.Now.AddDays(-1), CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DateOnlyInFuture_ReturnsSuccess()
    {
        FutureDateAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(DateOnly.FromDateTime(DateTime.Now.AddDays(2)), CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DateTimeOffsetInFuture_ReturnsSuccess()
    {
        FutureDateAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(DateTimeOffset.Now.AddDays(1), CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DateTimeOffsetInPast_ReturnsFailure()
    {
        FutureDateAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult(DateTimeOffset.Now.AddDays(-1), CreateContext("Value")));
    }

    #endregion
}
