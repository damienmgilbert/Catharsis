using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class CollectionCountAttributeTests
{
    [TestMethod]
    public void NullValue_ReturnsSuccess()
    {
        var attribute = new CollectionCountAttribute(1, 5);
        var context = CreateContext(nameof(TestModel.Items));

        var result = attribute.GetValidationResult(null, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void CountWithinRange_ReturnsSuccess()
    {
        var attribute = new CollectionCountAttribute(1, 5);
        var context = CreateContext(nameof(TestModel.Items));

        var result = attribute.GetValidationResult(new[] { 1, 2, 3 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void CountBelowMinimum_ReturnsFailure()
    {
        var attribute = new CollectionCountAttribute(2, 5);
        var context = CreateContext(nameof(TestModel.Items));

        var result = attribute.GetValidationResult(new[] { 1 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("between"));
    }

    [TestMethod]
    public void CountAboveMaximum_ReturnsFailure()
    {
        var attribute = new CollectionCountAttribute(1, 3);
        var context = CreateContext(nameof(TestModel.Items));

        var result = attribute.GetValidationResult(new[] { 1, 2, 3, 4 }, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void EmptyCollection_MinimumZero_ReturnsSuccess()
    {
        var attribute = new CollectionCountAttribute(0, 10);
        var context = CreateContext(nameof(TestModel.Items));

        var result = attribute.GetValidationResult(Array.Empty<int>(), context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void EmptyCollection_MinimumOne_ReturnsFailure()
    {
        var attribute = new CollectionCountAttribute(1, 10);
        var context = CreateContext(nameof(TestModel.Items));

        var result = attribute.GetValidationResult(Array.Empty<int>(), context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ExactCount_ReturnsSuccess()
    {
        var attribute = new CollectionCountAttribute(3, 3);
        var context = CreateContext(nameof(TestModel.Items));

        var result = attribute.GetValidationResult(new[] { 1, 2, 3 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void NonEnumerableValue_ReturnsFailure()
    {
        var attribute = new CollectionCountAttribute(1, 5);
        var context = CreateContext(nameof(TestModel.Items));

        var result = attribute.GetValidationResult(42, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("collection"));
    }

    [TestMethod]
    public void MinimumOnlyNoMaximum_ReturnsSuccess()
    {
        var attribute = new CollectionCountAttribute(2);
        var context = CreateContext(nameof(TestModel.Items));

        var result = attribute.GetValidationResult(new[] { 1, 2, 3 }, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void Constructor_NegativeMinimum_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new CollectionCountAttribute(-1, 5));
    }

    [TestMethod]
    public void Constructor_MaximumLessThanMinimum_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new CollectionCountAttribute(5, 2));
    }

    private static ValidationContext CreateContext(string memberName)
    {
        var model = new TestModel();
        return new ValidationContext(model) { MemberName = memberName, DisplayName = memberName };
    }

    private sealed class TestModel
    {
        public List<int>? Items { get; set; }
    }
}
