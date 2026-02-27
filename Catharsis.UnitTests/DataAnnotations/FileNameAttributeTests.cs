using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class FileNameAttributeTests
{
    [TestMethod]
    public void NullValue_ReturnsSuccess()
    {
        var attribute = new FileNameAttribute();
        var context = CreateContext(null, nameof(TestModel.Name));

        var result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidFileName_ReturnsSuccess()
    {
        var attribute = new FileNameAttribute();
        var context = CreateContext("report.pdf", nameof(TestModel.Name));

        var result = attribute.GetValidationResult("report.pdf", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void FileNameWithInvalidChars_ReturnsFailure()
    {
        var attribute = new FileNameAttribute();
        var context = CreateContext("file<name>.txt", nameof(TestModel.Name));

        var result = attribute.GetValidationResult("file<name>.txt", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("invalid character"));
    }

    [TestMethod]
    public void EmptyString_ReturnsFailure()
    {
        var attribute = new FileNameAttribute();
        var context = CreateContext("", nameof(TestModel.Name));

        var result = attribute.GetValidationResult("", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void WhitespaceOnly_ReturnsFailure()
    {
        var attribute = new FileNameAttribute();
        var context = CreateContext("   ", nameof(TestModel.Name));

        var result = attribute.GetValidationResult("   ", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ExceedsMaxLength_ReturnsFailure()
    {
        var attribute = new FileNameAttribute { MaxLength = 10 };
        var context = CreateContext("verylongfilename.txt", nameof(TestModel.Name));

        var result = attribute.GetValidationResult("verylongfilename.txt", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("at most 10"));
    }

    [TestMethod]
    public void WithinMaxLength_ReturnsSuccess()
    {
        var attribute = new FileNameAttribute { MaxLength = 20 };
        var context = CreateContext("short.txt", nameof(TestModel.Name));

        var result = attribute.GetValidationResult("short.txt", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void AllowedExtension_ReturnsSuccess()
    {
        var attribute = new FileNameAttribute { AllowedExtensions = [".txt", ".csv"] };
        var context = CreateContext("data.csv", nameof(TestModel.Name));

        var result = attribute.GetValidationResult("data.csv", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void DisallowedExtension_ReturnsFailure()
    {
        var attribute = new FileNameAttribute { AllowedExtensions = [".txt", ".csv"] };
        var context = CreateContext("image.png", nameof(TestModel.Name));

        var result = attribute.GetValidationResult("image.png", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains(".txt"));
    }

    [TestMethod]
    public void NonStringValue_ReturnsFailure()
    {
        var attribute = new FileNameAttribute();
        var context = CreateContext(42, nameof(TestModel.Name));

        var result = attribute.GetValidationResult(42, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("string"));
    }

    [TestMethod]
    public void AllowedExtensionCaseInsensitive_ReturnsSuccess()
    {
        var attribute = new FileNameAttribute { AllowedExtensions = [".TXT"] };
        var context = CreateContext("readme.txt", nameof(TestModel.Name));

        var result = attribute.GetValidationResult("readme.txt", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    private static ValidationContext CreateContext(object? value, string memberName)
    {
        var model = new TestModel { Name = value as string };
        return new ValidationContext(model) { MemberName = memberName, DisplayName = memberName };
    }

    private sealed class TestModel
    {
        public string? Name { get; set; }
    }
}
