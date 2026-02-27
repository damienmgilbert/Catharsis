using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class UniqueElementsAttributeTests
{
    [TestMethod]
    public void NullValue_ReturnsSuccess()
    {
        var attribute = new UniqueElementsAttribute();
        var context = CreateContext(nameof(TestModel.Tags));

        var result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UniqueElements_ReturnsSuccess()
    {
        var attribute = new UniqueElementsAttribute();
        var context = CreateContext(nameof(TestModel.Tags));

        var result = attribute.GetValidationResult(new[] { "a", "b", "c" }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void DuplicateElements_ReturnsFailure()
    {
        var attribute = new UniqueElementsAttribute();
        var context = CreateContext(nameof(TestModel.Tags));

        var result = attribute.GetValidationResult(new[] { "a", "b", "a" }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("duplicate"));
    }

    [TestMethod]
    public void EmptyCollection_ReturnsSuccess()
    {
        var attribute = new UniqueElementsAttribute();
        var context = CreateContext(nameof(TestModel.Tags));

        var result = attribute.GetValidationResult(Array.Empty<string>(), context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void SingleElement_ReturnsSuccess()
    {
        var attribute = new UniqueElementsAttribute();
        var context = CreateContext(nameof(TestModel.Tags));

        var result = attribute.GetValidationResult(new[] { "only" }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void DuplicateNulls_ReturnsFailure()
    {
        var attribute = new UniqueElementsAttribute();
        var context = CreateContext(nameof(TestModel.Tags));

        var result = attribute.GetValidationResult(new string?[] { null, "a", null }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void DuplicateIntegers_ReturnsFailure()
    {
        var attribute = new UniqueElementsAttribute();
        var context = CreateContext(nameof(TestModel.Tags));

        var result = attribute.GetValidationResult(new[] { 1, 2, 3, 2 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("index 3"));
    }

    [TestMethod]
    public void NonEnumerableValue_ReturnsFailure()
    {
        var attribute = new UniqueElementsAttribute();
        var context = CreateContext(nameof(TestModel.Tags));

        var result = attribute.GetValidationResult(42, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("collection"));
    }

    private static ValidationContext CreateContext(string memberName)
    {
        var model = new TestModel();
        return new ValidationContext(model) { MemberName = memberName, DisplayName = memberName };
    }

    private sealed class TestModel
    {
        public List<string>? Tags { get; set; }
    }
}
