using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class MutuallyExclusiveAttributeTests
{
    [TestMethod]
    public void OnlyOnePopulated_ReturnsSuccess()
    {
        var attribute = new MutuallyExclusiveAttribute(
            nameof(TestModel.FilePath), nameof(TestModel.Url));
        var model = new TestModel { FilePath = "C:\\file.txt", Url = null };
        var context = new ValidationContext(model);

        var result = attribute.GetValidationResult(model, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void BothPopulated_ReturnsFailure()
    {
        var attribute = new MutuallyExclusiveAttribute(
            nameof(TestModel.FilePath), nameof(TestModel.Url));
        var model = new TestModel { FilePath = "C:\\file.txt", Url = "https://example.com" };
        var context = new ValidationContext(model);

        var result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Only one"));
    }

    [TestMethod]
    public void NonePopulated_ReturnsSuccess()
    {
        var attribute = new MutuallyExclusiveAttribute(
            nameof(TestModel.FilePath), nameof(TestModel.Url));
        var model = new TestModel { FilePath = null, Url = null };
        var context = new ValidationContext(model);

        var result = attribute.GetValidationResult(model, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void EmptyStringsNotTreatedAsValues_ReturnsSuccess()
    {
        var attribute = new MutuallyExclusiveAttribute(
            nameof(TestModel.FilePath), nameof(TestModel.Url));
        var model = new TestModel { FilePath = "C:\\file.txt", Url = "" };
        var context = new ValidationContext(model);

        var result = attribute.GetValidationResult(model, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void WhitespaceNotTreatedAsValues_ReturnsSuccess()
    {
        var attribute = new MutuallyExclusiveAttribute(
            nameof(TestModel.FilePath), nameof(TestModel.Url));
        var model = new TestModel { FilePath = "C:\\file.txt", Url = "   " };
        var context = new ValidationContext(model);

        var result = attribute.GetValidationResult(model, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ThreeProperties_TwoPopulated_ReturnsFailure()
    {
        var attribute = new MutuallyExclusiveAttribute(
            nameof(TestModel.FilePath), nameof(TestModel.Url), nameof(TestModel.Count));
        var model = new TestModel { FilePath = "C:\\file.txt", Url = null, Count = 5 };
        var context = new ValidationContext(model);

        var result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void GroupNameAppearsInErrorMessage()
    {
        var attribute = new MutuallyExclusiveAttribute(
            nameof(TestModel.FilePath), nameof(TestModel.Url))
        {
            GroupName = "Data Source"
        };
        var model = new TestModel { FilePath = "C:\\file.txt", Url = "https://example.com" };
        var context = new ValidationContext(model);

        var result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Data Source"));
    }

    [TestMethod]
    public void NullObject_ReturnsSuccess()
    {
        var attribute = new MutuallyExclusiveAttribute(
            nameof(TestModel.FilePath), nameof(TestModel.Url));
        var context = new ValidationContext(new object());

        var result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UnknownProperty_ReturnsFailure()
    {
        var attribute = new MutuallyExclusiveAttribute("NonExistent", nameof(TestModel.Url));
        var model = new TestModel { Url = "https://example.com" };
        var context = new ValidationContext(model);

        var result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Unknown property"));
    }

    [TestMethod]
    public void Constructor_FewerThanTwoProperties_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new MutuallyExclusiveAttribute("OnlyOne"));
    }

    [TestMethod]
    public void FailureResult_ContainsMemberNames()
    {
        var attribute = new MutuallyExclusiveAttribute(
            nameof(TestModel.FilePath), nameof(TestModel.Url));
        var model = new TestModel { FilePath = "C:\\file.txt", Url = "https://example.com" };
        var context = new ValidationContext(model);

        var result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        CollectionAssert.Contains(
            result!.MemberNames.ToList(),
            nameof(TestModel.FilePath));
        CollectionAssert.Contains(
            result.MemberNames.ToList(),
            nameof(TestModel.Url));
    }

    private sealed class TestModel
    {
        public string? FilePath { get; set; }
        public string? Url { get; set; }
        public int? Count { get; set; }
    }
}
