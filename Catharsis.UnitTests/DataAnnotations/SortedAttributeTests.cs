using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class SortedAttributeTests
{
    [TestMethod]
    public void NullValue_ReturnsSuccess()
    {
        var attribute = new SortedAttribute();
        var context = CreateContext(nameof(TestModel.Values));

        var result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void AscendingSorted_ReturnsSuccess()
    {
        var attribute = new SortedAttribute(SortDirection.Ascending);
        var context = CreateContext(nameof(TestModel.Values));

        var result = attribute.GetValidationResult(new[] { 1, 2, 3, 4, 5 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void DescendingSorted_ReturnsSuccess()
    {
        var attribute = new SortedAttribute(SortDirection.Descending);
        var context = CreateContext(nameof(TestModel.Values));

        var result = attribute.GetValidationResult(new[] { 5, 4, 3, 2, 1 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UnsortedAscending_ReturnsFailure()
    {
        var attribute = new SortedAttribute(SortDirection.Ascending);
        var context = CreateContext(nameof(TestModel.Values));

        var result = attribute.GetValidationResult(new[] { 1, 3, 2, 4 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("ascending"));
    }

    [TestMethod]
    public void UnsortedDescending_ReturnsFailure()
    {
        var attribute = new SortedAttribute(SortDirection.Descending);
        var context = CreateContext(nameof(TestModel.Values));

        var result = attribute.GetValidationResult(new[] { 5, 3, 4, 1 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("descending"));
    }

    [TestMethod]
    public void DuplicatesAllowed_WithDuplicates_ReturnsSuccess()
    {
        var attribute = new SortedAttribute(SortDirection.Ascending) { AllowDuplicates = true };
        var context = CreateContext(nameof(TestModel.Values));

        var result = attribute.GetValidationResult(new[] { 1, 2, 2, 3 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void DuplicatesDisallowed_WithDuplicates_ReturnsFailure()
    {
        var attribute = new SortedAttribute(SortDirection.Ascending) { AllowDuplicates = false };
        var context = CreateContext(nameof(TestModel.Values));

        var result = attribute.GetValidationResult(new[] { 1, 2, 2, 3 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void SingleElement_ReturnsSuccess()
    {
        var attribute = new SortedAttribute();
        var context = CreateContext(nameof(TestModel.Values));

        var result = attribute.GetValidationResult(new[] { 42 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void EmptyCollection_ReturnsSuccess()
    {
        var attribute = new SortedAttribute();
        var context = CreateContext(nameof(TestModel.Values));

        var result = attribute.GetValidationResult(Array.Empty<int>(), context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void StringsSortedAscending_ReturnsSuccess()
    {
        var attribute = new SortedAttribute(SortDirection.Ascending);
        var context = CreateContext(nameof(TestModel.Values));

        var result = attribute.GetValidationResult(new[] { "apple", "banana", "cherry" }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void DefaultDirection_IsAscending()
    {
        var attribute = new SortedAttribute();

        Assert.AreEqual(SortDirection.Ascending, attribute.Direction);
    }

    [TestMethod]
    public void NonEnumerableValue_ReturnsFailure()
    {
        var attribute = new SortedAttribute();
        var context = CreateContext(nameof(TestModel.Values));

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
        public List<int>? Values { get; set; }
    }
}
