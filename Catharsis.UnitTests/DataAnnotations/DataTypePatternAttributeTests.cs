using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class DataTypePatternAttributeTests
{
    [TestMethod]
    public void NullValue_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.EmailAddress);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidEmail_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.EmailAddress);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("user@example.com", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void InvalidEmail_ReturnsFailure()
    {
        var attribute = new DataTypePatternAttribute(DataType.EmailAddress);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("not-an-email", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("EmailAddress"));
    }

    [TestMethod]
    public void ValidPhoneNumber_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.PhoneNumber);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("+1 (555) 123-4567", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void InvalidPhoneNumber_ReturnsFailure()
    {
        var attribute = new DataTypePatternAttribute(DataType.PhoneNumber);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("abc", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidPostalCode_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.PostalCode);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("12345", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidPostalCodeWithExtension_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.PostalCode);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("12345-6789", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidUrl_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.Url);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("https://example.com/path?q=1", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidDate_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.Date);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("2024-01-15", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidTime_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.Time);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("14:30:00", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidDateTime_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.DateTime);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("2024-01-15T14:30:00", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidDuration_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.Duration);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("P1DT2H30M", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidImageUrl_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.ImageUrl);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("https://example.com/photo.jpg", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidCurrency_ReturnsSuccess()
    {
        var attribute = new DataTypePatternAttribute(DataType.Currency);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("$1,234.56", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UnsupportedDataType_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new DataTypePatternAttribute(DataType.Password));
    }

    [TestMethod]
    public void NonStringValue_ReturnsFailure()
    {
        var attribute = new DataTypePatternAttribute(DataType.EmailAddress);
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult(42, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("string"));
    }

    [TestMethod]
    public void PatternProperty_MatchesExpectedDataType()
    {
        var attribute = new DataTypePatternAttribute(DataType.EmailAddress);

        Assert.AreEqual(DataType.EmailAddress, attribute.DataType);
        Assert.IsNotNull(attribute.Pattern);
    }

    private static ValidationContext CreateContext(string memberName)
    {
        var model = new TestModel();
        return new ValidationContext(model) { MemberName = memberName, DisplayName = memberName };
    }

    private sealed class TestModel
    {
        public string? Value { get; set; }
    }
}
