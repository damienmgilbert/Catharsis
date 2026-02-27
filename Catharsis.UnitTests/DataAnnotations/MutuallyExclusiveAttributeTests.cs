using System.ComponentModel.DataAnnotations;
using Catharsis.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class MutuallyExclusiveAttributeTests
{
    #region Public methods
    [TestMethod]
    public void BothPopulated_ReturnsFailure()
    {
        MutuallyExclusiveAttribute attribute = new MutuallyExclusiveAttribute(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new TestModel { FilePath = "C:\\file.txt", Url = "https://example.com" };
        ValidationContext context = new ValidationContext(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Only one"));
    }

    [TestMethod]
    public void Constructor_FewerThanTwoProperties_ThrowsArgumentException() { Assert.ThrowsExactly<ArgumentException>(() => new MutuallyExclusiveAttribute("OnlyOne")); }
    [TestMethod]
    public void EmptyStringsNotTreatedAsValues_ReturnsSuccess()
    {
        MutuallyExclusiveAttribute attribute = new MutuallyExclusiveAttribute(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new TestModel { FilePath = "C:\\file.txt", Url = string.Empty };
        ValidationContext context = new ValidationContext(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void FailureResult_ContainsMemberNames()
    {
        MutuallyExclusiveAttribute attribute = new MutuallyExclusiveAttribute(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new TestModel { FilePath = "C:\\file.txt", Url = "https://example.com" };
        ValidationContext context = new ValidationContext(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        CollectionAssert.Contains(result!.MemberNames.ToList(), nameof(TestModel.FilePath));
        CollectionAssert.Contains(result.MemberNames.ToList(), nameof(TestModel.Url));
    }

    [TestMethod]
    public void GroupNameAppearsInErrorMessage()
    {
        MutuallyExclusiveAttribute attribute = new MutuallyExclusiveAttribute(nameof(TestModel.FilePath), nameof(TestModel.Url)) { GroupName = "Data Source" };
        TestModel model = new TestModel { FilePath = "C:\\file.txt", Url = "https://example.com" };
        ValidationContext context = new ValidationContext(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Data Source"));
    }

    [TestMethod]
    public void NonePopulated_ReturnsSuccess()
    {
        MutuallyExclusiveAttribute attribute = new MutuallyExclusiveAttribute(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new TestModel { FilePath = null, Url = null };
        ValidationContext context = new ValidationContext(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void NullObject_ReturnsSuccess()
    {
        MutuallyExclusiveAttribute attribute = new MutuallyExclusiveAttribute(nameof(TestModel.FilePath), nameof(TestModel.Url));
        ValidationContext context = new ValidationContext(new object());

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void OnlyOnePopulated_ReturnsSuccess()
    {
        MutuallyExclusiveAttribute attribute = new MutuallyExclusiveAttribute(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new TestModel { FilePath = "C:\\file.txt", Url = null };
        ValidationContext context = new ValidationContext(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ThreeProperties_TwoPopulated_ReturnsFailure()
    {
        MutuallyExclusiveAttribute attribute = new MutuallyExclusiveAttribute(nameof(TestModel.FilePath), nameof(TestModel.Url), nameof(TestModel.Count));
        TestModel model = new TestModel { FilePath = "C:\\file.txt", Url = null, Count = 5 };
        ValidationContext context = new ValidationContext(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UnknownProperty_ReturnsFailure()
    {
        MutuallyExclusiveAttribute attribute = new MutuallyExclusiveAttribute("NonExistent", nameof(TestModel.Url));
        TestModel model = new TestModel { Url = "https://example.com" };
        ValidationContext context = new ValidationContext(model);

        ValidationResult? result = attribute.GetValidationResult(model, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Unknown property"));
    }

    [TestMethod]
    public void WhitespaceNotTreatedAsValues_ReturnsSuccess()
    {
        MutuallyExclusiveAttribute attribute = new MutuallyExclusiveAttribute(nameof(TestModel.FilePath), nameof(TestModel.Url));
        TestModel model = new TestModel { FilePath = "C:\\file.txt", Url = "   " };
        ValidationContext context = new ValidationContext(model);

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
