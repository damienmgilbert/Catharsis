using System.ComponentModel.DataAnnotations;
using Catharsis.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class NotEqualToAttributeTests
{
    #region Public methods
    [TestMethod]
    public void BothNull_ReturnsFailure()
    {
        NotEqualToAttribute attribute = new NotEqualToAttribute(nameof(TestModel.Other));
        TestModel model = new TestModel { Value = null, Other = null };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void Constructor_NullPropertyName_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new NotEqualToAttribute(null!)); }
    [TestMethod]
    public void CustomDisplayName_AppearsInErrorMessage()
    {
        NotEqualToAttribute attribute = new NotEqualToAttribute(nameof(TestModel.Other)) { OtherPropertyDisplayName = "Comparison Field" };
        TestModel model = new TestModel { Value = "X", Other = "X" };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        ValidationResult? result = attribute.GetValidationResult("X", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Comparison Field"));
    }

    [TestMethod]
    public void DifferentValues_ReturnsSuccess()
    {
        NotEqualToAttribute attribute = new NotEqualToAttribute(nameof(TestModel.Other));
        TestModel model = new TestModel { Value = "A", Other = "B" };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        ValidationResult? result = attribute.GetValidationResult("A", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void EqualValues_ReturnsFailure()
    {
        NotEqualToAttribute attribute = new NotEqualToAttribute(nameof(TestModel.Other));
        TestModel model = new TestModel { Value = "same", Other = "same" };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        ValidationResult? result = attribute.GetValidationResult("same", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("must not equal"));
    }

    [TestMethod]
    public void OneNullOneDifferent_ReturnsSuccess()
    {
        NotEqualToAttribute attribute = new NotEqualToAttribute(nameof(TestModel.Other));
        TestModel model = new TestModel { Value = null, Other = "not null" };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UnknownProperty_ReturnsFailure()
    {
        NotEqualToAttribute attribute = new NotEqualToAttribute("NonExistent");
        TestModel model = new TestModel { Value = "A" };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        ValidationResult? result = attribute.GetValidationResult("A", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Unknown property"));
    }
    #endregion

    sealed class TestModel
    {
        #region Public properties
        public string? Other { get; set; }

        public string? Value { get; set; }
        #endregion
    }
}
