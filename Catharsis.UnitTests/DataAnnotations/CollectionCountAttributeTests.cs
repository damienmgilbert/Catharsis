using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

///<summary>
///Unit tests for the <see cref="CollectionCountAttribute"/> class.
///</summary>
[TestClass]
public class CollectionCountAttributeTests
{
    #region Private methods
    private static ValidationContext CreateContext(string memberName)
    {
        TestModel model = new TestModel();
        return new ValidationContext(model) { MemberName = memberName, DisplayName = memberName };
    }
    #endregion

    #region Public methods
    [TestMethod]
    public void Constructor_MaximumLessThanMinimum_ThrowsArgumentOutOfRangeException() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new CollectionCountAttribute(5, 2)); }
    [TestMethod]
    public void Constructor_NegativeMinimum_ThrowsArgumentOutOfRangeException() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new CollectionCountAttribute(-1, 5)); }
    [TestMethod]
    public void CountAboveMaximum_ReturnsFailure()
    {
        CollectionCountAttribute attribute = new CollectionCountAttribute(1, 3);
        ValidationContext context = CreateContext(nameof(TestModel.Items));

        ValidationResult? result = attribute.GetValidationResult(new[] { 1, 2, 3, 4 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void CountBelowMinimum_ReturnsFailure()
    {
        CollectionCountAttribute attribute = new CollectionCountAttribute(2, 5);
        ValidationContext context = CreateContext(nameof(TestModel.Items));

        ValidationResult? result = attribute.GetValidationResult(new[] { 1 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("between", result!.ErrorMessage!);
    }

    [TestMethod]
    public void CountWithinRange_ReturnsSuccess()
    {
        CollectionCountAttribute attribute = new CollectionCountAttribute(1, 5);
        ValidationContext context = CreateContext(nameof(TestModel.Items));

        ValidationResult? result = attribute.GetValidationResult(new[] { 1, 2, 3 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void EmptyCollection_MinimumOne_ReturnsFailure()
    {
        CollectionCountAttribute attribute = new CollectionCountAttribute(1, 10);
        ValidationContext context = CreateContext(nameof(TestModel.Items));

        ValidationResult? result = attribute.GetValidationResult(Array.Empty<int>(), context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void EmptyCollection_MinimumZero_ReturnsSuccess()
    {
        CollectionCountAttribute attribute = new CollectionCountAttribute(0, 10);
        ValidationContext context = CreateContext(nameof(TestModel.Items));

        ValidationResult? result = attribute.GetValidationResult(Array.Empty<int>(), context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ExactCount_ReturnsSuccess()
    {
        CollectionCountAttribute attribute = new CollectionCountAttribute(3, 3);
        ValidationContext context = CreateContext(nameof(TestModel.Items));

        ValidationResult? result = attribute.GetValidationResult(new[] { 1, 2, 3 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void MinimumOnlyNoMaximum_ReturnsSuccess()
    {
        CollectionCountAttribute attribute = new CollectionCountAttribute(2);
        ValidationContext context = CreateContext(nameof(TestModel.Items));

        ValidationResult? result = attribute.GetValidationResult(new[] { 1, 2, 3 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void NonEnumerableValue_ReturnsFailure()
    {
        CollectionCountAttribute attribute = new CollectionCountAttribute(1, 5);
        ValidationContext context = CreateContext(nameof(TestModel.Items));

        ValidationResult? result = attribute.GetValidationResult(42, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.Contains("collection", result!.ErrorMessage!);
    }

    [TestMethod]
    public void NullValue_ReturnsSuccess()
    {
        CollectionCountAttribute attribute = new CollectionCountAttribute(1, 5);
        ValidationContext context = CreateContext(nameof(TestModel.Items));

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }
    #endregion

    private sealed class TestModel
    {
        #region Public properties
        public List<int>? Items { get; set; }
        #endregion
    }
}
