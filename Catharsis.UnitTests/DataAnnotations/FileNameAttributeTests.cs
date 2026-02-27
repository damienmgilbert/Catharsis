using System.ComponentModel.DataAnnotations;
using Catharsis.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class FileNameAttributeTests
{
    #region Private methods
    static ValidationContext CreateContext(object? value, string memberName)
    {
        TestModel model = new TestModel { Name = value as string };
        return new ValidationContext(model) { MemberName = memberName, DisplayName = memberName };
    }
    #endregion

    #region Public methods
    [TestMethod]
    public void AllowedExtension_ReturnsSuccess()
    {
        FileNameAttribute attribute = new FileNameAttribute { AllowedExtensions = [ ".txt", ".csv" ] };
        ValidationContext context = CreateContext("data.csv", nameof(TestModel.Name));

        ValidationResult? result = attribute.GetValidationResult("data.csv", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void AllowedExtensionCaseInsensitive_ReturnsSuccess()
    {
        FileNameAttribute attribute = new FileNameAttribute { AllowedExtensions = [ ".TXT" ] };
        ValidationContext context = CreateContext("readme.txt", nameof(TestModel.Name));

        ValidationResult? result = attribute.GetValidationResult("readme.txt", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void DisallowedExtension_ReturnsFailure()
    {
        FileNameAttribute attribute = new FileNameAttribute { AllowedExtensions = [ ".txt", ".csv" ] };
        ValidationContext context = CreateContext("image.png", nameof(TestModel.Name));

        ValidationResult? result = attribute.GetValidationResult("image.png", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains(".txt"));
    }

    [TestMethod]
    public void EmptyString_ReturnsFailure()
    {
        FileNameAttribute attribute = new FileNameAttribute();
        ValidationContext context = CreateContext(string.Empty, nameof(TestModel.Name));

        ValidationResult? result = attribute.GetValidationResult(string.Empty, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ExceedsMaxLength_ReturnsFailure()
    {
        FileNameAttribute attribute = new FileNameAttribute { MaxLength = 10 };
        ValidationContext context = CreateContext("verylongfilename.txt", nameof(TestModel.Name));

        ValidationResult? result = attribute.GetValidationResult("verylongfilename.txt", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("at most 10"));
    }

    [TestMethod]
    public void FileNameWithInvalidChars_ReturnsFailure()
    {
        FileNameAttribute attribute = new FileNameAttribute();
        ValidationContext context = CreateContext("file<name>.txt", nameof(TestModel.Name));

        ValidationResult? result = attribute.GetValidationResult("file<name>.txt", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("invalid character"));
    }

    [TestMethod]
    public void NonStringValue_ReturnsFailure()
    {
        FileNameAttribute attribute = new FileNameAttribute();
        ValidationContext context = CreateContext(42, nameof(TestModel.Name));

        ValidationResult? result = attribute.GetValidationResult(42, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("string"));
    }

    [TestMethod]
    public void NullValue_ReturnsSuccess()
    {
        FileNameAttribute attribute = new FileNameAttribute();
        ValidationContext context = CreateContext(null, nameof(TestModel.Name));

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidFileName_ReturnsSuccess()
    {
        FileNameAttribute attribute = new FileNameAttribute();
        ValidationContext context = CreateContext("report.pdf", nameof(TestModel.Name));

        ValidationResult? result = attribute.GetValidationResult("report.pdf", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void WhitespaceOnly_ReturnsFailure()
    {
        FileNameAttribute attribute = new FileNameAttribute();
        ValidationContext context = CreateContext("   ", nameof(TestModel.Name));

        ValidationResult? result = attribute.GetValidationResult("   ", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void WithinMaxLength_ReturnsSuccess()
    {
        FileNameAttribute attribute = new FileNameAttribute { MaxLength = 20 };
        ValidationContext context = CreateContext("short.txt", nameof(TestModel.Name));

        ValidationResult? result = attribute.GetValidationResult("short.txt", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }
    #endregion

    sealed class TestModel
    {
        #region Public properties
        public string? Name { get; set; }
        #endregion
    }
}
