using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="PastDateAttribute"/> class.
///</summary>
[TestClass]
public class PastDateAttributeTests
{
    #region Private methods
    private static ValidationContext CreateContext(string memberName) => new(new object()) { MemberName = memberName, DisplayName = memberName };
    #endregion

    #region Public methods

    [TestMethod]
    public void IsValid_Null_ReturnsSuccess()
    {
        PastDateAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(null, CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_NonDateValue_ReturnsFailure()
    {
        PastDateAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult(42, CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DateTimeInPast_ReturnsSuccess()
    {
        PastDateAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(DateTime.Now.AddDays(-1), CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DateTimeInFuture_ReturnsFailure()
    {
        PastDateAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult(DateTime.Now.AddDays(1), CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DateOnlyInPast_ReturnsSuccess()
    {
        PastDateAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(DateOnly.FromDateTime(DateTime.Now.AddDays(-2)), CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DateTimeOffsetInPast_ReturnsSuccess()
    {
        PastDateAttribute attribute = new();
        Assert.AreEqual(ValidationResult.Success, attribute.GetValidationResult(DateTimeOffset.Now.AddDays(-1), CreateContext("Value")));
    }

    [TestMethod]
    public void IsValid_DateTimeOffsetInFuture_ReturnsFailure()
    {
        PastDateAttribute attribute = new();
        Assert.AreNotEqual(ValidationResult.Success, attribute.GetValidationResult(DateTimeOffset.Now.AddDays(1), CreateContext("Value")));
    }

    #endregion
}
