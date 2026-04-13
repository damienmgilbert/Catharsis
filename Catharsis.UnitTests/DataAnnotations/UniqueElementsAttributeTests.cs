using System.ComponentModel.DataAnnotations;
using Catharsis.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="UniqueElementsAttribute"/> class.
///</summary>
[TestClass]
public class UniqueElementsAttributeTests
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
    public void DuplicateElements_ReturnsFailure()
    {
        UniqueElementsAttribute attribute = new();
        ValidationContext context = CreateContext(nameof(TestModel.Tags));

        ValidationResult? result = attribute.GetValidationResult(new[] { "a", "b", "a" }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("duplicate", result!.ErrorMessage!);
    }

    [TestMethod]
    public void DuplicateIntegers_ReturnsFailure()
    {
        UniqueElementsAttribute attribute = new();
        ValidationContext context = CreateContext(nameof(TestModel.Tags));

        ValidationResult? result = attribute.GetValidationResult(new[] { 1, 2, 3, 2 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("index 3", result!.ErrorMessage!);
    }

    [TestMethod]
    public void DuplicateNulls_ReturnsFailure()
    {
        UniqueElementsAttribute attribute = new();
        ValidationContext context = CreateContext(nameof(TestModel.Tags));

        ValidationResult? result = attribute.GetValidationResult(new string?[] { null, "a", null }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void EmptyCollection_ReturnsSuccess()
    {
        UniqueElementsAttribute attribute = new();
        ValidationContext context = CreateContext(nameof(TestModel.Tags));

        ValidationResult? result = attribute.GetValidationResult(Array.Empty<string>(), context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void NonEnumerableValue_ReturnsFailure()
    {
        UniqueElementsAttribute attribute = new();
        ValidationContext context = CreateContext(nameof(TestModel.Tags));

        ValidationResult? result = attribute.GetValidationResult(42, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("collection", result!.ErrorMessage!);
    }

    [TestMethod]
    public void NullValue_ReturnsSuccess()
    {
        UniqueElementsAttribute attribute = new();
        ValidationContext context = CreateContext(nameof(TestModel.Tags));

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void SingleElement_ReturnsSuccess()
    {
        UniqueElementsAttribute attribute = new();
        ValidationContext context = CreateContext(nameof(TestModel.Tags));

        ValidationResult? result = attribute.GetValidationResult(new[] { "only" }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UniqueElements_ReturnsSuccess()
    {
        UniqueElementsAttribute attribute = new();
        ValidationContext context = CreateContext(nameof(TestModel.Tags));

        ValidationResult? result = attribute.GetValidationResult(new[] { "a", "b", "c" }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }
    #endregion

    sealed class TestModel
    {
        #region Public properties
        public List<string>? Tags { get; set; }
        #endregion
    }
}
