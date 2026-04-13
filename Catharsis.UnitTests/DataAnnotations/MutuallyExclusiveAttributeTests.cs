using System.ComponentModel.DataAnnotations;
using Catharsis.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="MutuallyExclusiveAttribute"/> class.
///</summary>
[TestClass]
public class MutuallyExclusiveAttributeTests
{
    #region Public methods
    [TestMethod]
    public void BothPopulated_ReturnsFailure()
    {
        MutuallyExclusiveAttribute attribute = new(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new() { FilePath = "C:\\file.txt", Url = "https://example.com" };
        ValidationContext context = new(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("Only one", result!.ErrorMessage!);
    }

    [TestMethod]
    public void Constructor_FewerThanTwoProperties_ThrowsArgumentException() { Assert.ThrowsExactly<ArgumentException>(static () => new MutuallyExclusiveAttribute("OnlyOne")); }
    [TestMethod]
    public void EmptyStringsNotTreatedAsValues_ReturnsSuccess()
    {
        MutuallyExclusiveAttribute attribute = new(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new() { FilePath = "C:\\file.txt", Url = string.Empty };
        ValidationContext context = new(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void FailureResult_ContainsMemberNames()
    {
        MutuallyExclusiveAttribute attribute = new(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new() { FilePath = "C:\\file.txt", Url = "https://example.com" };
        ValidationContext context = new(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        CollectionAssert.Contains(result!.MemberNames.ToList(), nameof(TestModel.FilePath));
        CollectionAssert.Contains(result.MemberNames.ToList(), nameof(TestModel.Url));
    }

    [TestMethod]
    public void GroupNameAppearsInErrorMessage()
    {
        MutuallyExclusiveAttribute attribute = new(nameof(TestModel.FilePath), nameof(TestModel.Url)) { GroupName = "Data Source" };
        TestModel model = new() { FilePath = "C:\\file.txt", Url = "https://example.com" };
        ValidationContext context = new(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("Data Source", result!.ErrorMessage!);
    }

    [TestMethod]
    public void NonePopulated_ReturnsSuccess()
    {
        MutuallyExclusiveAttribute attribute = new(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new() { FilePath = null, Url = null };
        ValidationContext context = new(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void NullObject_ReturnsSuccess()
    {
        MutuallyExclusiveAttribute attribute = new(nameof(TestModel.FilePath), nameof(TestModel.Url));
        ValidationContext context = new(new object());

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void OnlyOnePopulated_ReturnsSuccess()
    {
        MutuallyExclusiveAttribute attribute = new(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new() { FilePath = "C:\\file.txt", Url = null };
        ValidationContext context = new(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ThreeProperties_TwoPopulated_ReturnsFailure()
    {
        MutuallyExclusiveAttribute attribute = new(nameof(TestModel.FilePath), nameof(TestModel.Url), nameof(TestModel.Count));
        TestModel model = new() { FilePath = "C:\\file.txt", Url = null, Count = 5 };
        ValidationContext context = new(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UnknownProperty_ReturnsFailure()
    {
        MutuallyExclusiveAttribute attribute = new("NonExistent", nameof(TestModel.Url));
        TestModel model = new() { Url = "https://example.com" };
        ValidationContext context = new(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("Unknown property", result!.ErrorMessage!);
    }

    [TestMethod]
    public void WhitespaceNotTreatedAsValues_ReturnsSuccess()
    {
        MutuallyExclusiveAttribute attribute = new(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new() { FilePath = "C:\\file.txt", Url = "   " };
        ValidationContext context = new(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }
    #endregion

    sealed class TestModel
    {
        #region Public properties
        public int? Count { get; set; }

        public string? FilePath { get; set; }

        public string? Url { get; set; }
        #endregion
    }
}
