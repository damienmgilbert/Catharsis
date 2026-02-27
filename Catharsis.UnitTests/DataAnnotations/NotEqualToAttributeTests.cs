using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class NotEqualToAttributeTests
{
    [TestMethod]
    public void DifferentValues_ReturnsSuccess()
    {
        var attribute = new NotEqualToAttribute(nameof(TestModel.Other));
        var model = new TestModel { Value = "A", Other = "B" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        var result = attribute.GetValidationResult("A", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void EqualValues_ReturnsFailure()
    {
        var attribute = new NotEqualToAttribute(nameof(TestModel.Other));
        var model = new TestModel { Value = "same", Other = "same" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        var result = attribute.GetValidationResult("same", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("must not equal"));
    }

    [TestMethod]
    public void BothNull_ReturnsFailure()
    {
        var attribute = new NotEqualToAttribute(nameof(TestModel.Other));
        var model = new TestModel { Value = null, Other = null };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        var result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void OneNullOneDifferent_ReturnsSuccess()
    {
        var attribute = new NotEqualToAttribute(nameof(TestModel.Other));
        var model = new TestModel { Value = null, Other = "not null" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        var result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UnknownProperty_ReturnsFailure()
    {
        var attribute = new NotEqualToAttribute("NonExistent");
        var model = new TestModel { Value = "A" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        var result = attribute.GetValidationResult("A", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Unknown property"));
    }

    [TestMethod]
    public void CustomDisplayName_AppearsInErrorMessage()
    {
        var attribute = new NotEqualToAttribute(nameof(TestModel.Other))
        {
            OtherPropertyDisplayName = "Comparison Field"
        };
        var model = new TestModel { Value = "X", Other = "X" };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Value) };

        var result = attribute.GetValidationResult("X", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("Comparison Field"));
    }

    [TestMethod]
    public void Constructor_NullPropertyName_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new NotEqualToAttribute(null!));
    }

    private sealed class TestModel
    {
        public string? Value { get; set; }
        public string? Other { get; set; }
    }
}
