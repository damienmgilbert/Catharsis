using System.ComponentModel.DataAnnotations;
using Catharsis.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class SortedAttributeTests
{
    #region Private methods
    static ValidationContext CreateContext(string memberName)
    {
        TestModel model = new TestModel();
        return new ValidationContext(model) { MemberName = memberName, DisplayName = memberName };
    }
    #endregion

    #region Public methods
    [TestMethod]
    public void AscendingSorted_ReturnsSuccess()
    {
        SortedAttribute attribute = new SortedAttribute(SortDirection.Ascending);
        ValidationContext context = CreateContext(nameof(TestModel.Values));

        ValidationResult? result = attribute.GetValidationResult(new[] { 1, 2, 3, 4, 5 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void DefaultDirection_IsAscending()
    {
        SortedAttribute attribute = new SortedAttribute();

        Assert.AreEqual(SortDirection.Ascending, attribute.Direction);
    }

    [TestMethod]
    public void DescendingSorted_ReturnsSuccess()
    {
        SortedAttribute attribute = new SortedAttribute(SortDirection.Descending);
        ValidationContext context = CreateContext(nameof(TestModel.Values));

        ValidationResult? result = attribute.GetValidationResult(new[] { 5, 4, 3, 2, 1 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void DuplicatesAllowed_WithDuplicates_ReturnsSuccess()
    {
        SortedAttribute attribute = new SortedAttribute(SortDirection.Ascending) { AllowDuplicates = true };
        ValidationContext context = CreateContext(nameof(TestModel.Values));

        ValidationResult? result = attribute.GetValidationResult(new[] { 1, 2, 2, 3 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void DuplicatesDisallowed_WithDuplicates_ReturnsFailure()
    {
        SortedAttribute attribute = new SortedAttribute(SortDirection.Ascending) { AllowDuplicates = false };
        ValidationContext context = CreateContext(nameof(TestModel.Values));

        ValidationResult? result = attribute.GetValidationResult(new[] { 1, 2, 2, 3 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void EmptyCollection_ReturnsSuccess()
    {
        SortedAttribute attribute = new SortedAttribute();
        ValidationContext context = CreateContext(nameof(TestModel.Values));

        ValidationResult? result = attribute.GetValidationResult(Array.Empty<int>(), context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void NonEnumerableValue_ReturnsFailure()
    {
        SortedAttribute attribute = new SortedAttribute();
        ValidationContext context = CreateContext(nameof(TestModel.Values));

        ValidationResult? result = attribute.GetValidationResult(42, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("collection", result!.ErrorMessage!);
    }

    [TestMethod]
    public void NullValue_ReturnsSuccess()
    {
        SortedAttribute attribute = new SortedAttribute();
        ValidationContext context = CreateContext(nameof(TestModel.Values));

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void SingleElement_ReturnsSuccess()
    {
        SortedAttribute attribute = new SortedAttribute();
        ValidationContext context = CreateContext(nameof(TestModel.Values));

        ValidationResult? result = attribute.GetValidationResult(new[] { 42 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void StringsSortedAscending_ReturnsSuccess()
    {
        SortedAttribute attribute = new SortedAttribute(SortDirection.Ascending);
        ValidationContext context = CreateContext(nameof(TestModel.Values));

        ValidationResult? result = attribute.GetValidationResult(new[] { "apple", "banana", "cherry" }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void UnsortedAscending_ReturnsFailure()
    {
        SortedAttribute attribute = new SortedAttribute(SortDirection.Ascending);
        ValidationContext context = CreateContext(nameof(TestModel.Values));

        ValidationResult? result = attribute.GetValidationResult(new[] { 1, 3, 2, 4 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("ascending", result!.ErrorMessage!);
    }

    [TestMethod]
    public void UnsortedDescending_ReturnsFailure()
    {
        SortedAttribute attribute = new SortedAttribute(SortDirection.Descending);
        ValidationContext context = CreateContext(nameof(TestModel.Values));

        ValidationResult? result = attribute.GetValidationResult(new[] { 5, 3, 4, 1 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("descending", result!.ErrorMessage!);
    }
    #endregion

    sealed class TestModel
    {
        #region Public properties
        public List<int>? Values { get; set; }
        #endregion
    }
}
