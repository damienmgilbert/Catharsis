using System.ComponentModel.DataAnnotations;
using Catharsis.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="DataTypePatternAttribute"/> class.
///</summary>
[TestClass]
public class DataTypePatternAttributeTests
{
    #region Private methods
    static ValidationContext CreateContext(string memberName)
    {
        TestModel model = new();
        return new ValidationContext(model) { MemberName = memberName, DisplayName = memberName };
    }
    #endregion

    #region Public methods
    [TestMethod]
    public void InvalidEmail_ReturnsFailure()
    {
        DataTypePatternAttribute attribute = new(DataType.EmailAddress);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("not-an-email", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("EmailAddress", result!.ErrorMessage!);
    }

    [TestMethod]
    public void InvalidPhoneNumber_ReturnsFailure()
    {
        DataTypePatternAttribute attribute = new(DataType.PhoneNumber);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("abc", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void NonStringValue_ReturnsFailure()
    {
        DataTypePatternAttribute attribute = new(DataType.EmailAddress);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult(42, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("string", result!.ErrorMessage!);
    }

    [TestMethod]
    public void NullValue_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.EmailAddress);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void PatternProperty_MatchesExpectedDataType()
    {
        DataTypePatternAttribute attribute = new(DataType.EmailAddress);

        Assert.AreEqual(DataType.EmailAddress, attribute.DataType);
        Assert.IsNotNull(attribute.Pattern);
    }

    [TestMethod]
    public void UnsupportedDataType_ThrowsArgumentException() { Assert.ThrowsExactly<ArgumentException>(static () => new DataTypePatternAttribute(DataType.Password)); }
    [TestMethod]
    public void ValidCurrency_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.Currency);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("$1,234.56", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidDate_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.Date);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("2024-01-15", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidDateTime_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.DateTime);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("2024-01-15T14:30:00", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidDuration_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.Duration);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("P1DT2H30M", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidEmail_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.EmailAddress);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("user@example.com", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidImageUrl_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.ImageUrl);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("https://example.com/photo.jpg", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidPhoneNumber_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.PhoneNumber);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("+1 (555) 123-4567", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidPostalCode_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.PostalCode);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("12345", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidPostalCodeWithExtension_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.PostalCode);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("12345-6789", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidTime_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.Time);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("14:30:00", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidUrl_ReturnsSuccess()
    {
        DataTypePatternAttribute attribute = new(DataType.Url);
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("https://example.com/path?q=1", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }
    #endregion

    sealed class TestModel
    {
        #region Public properties
        public string? Value { get; set; }
        #endregion
    }
}
