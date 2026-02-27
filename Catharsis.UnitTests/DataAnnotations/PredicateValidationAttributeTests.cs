using Catharsis.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class PredicateValidationAttributeTests
{
    [TestMethod]
    public void PredicateReturnsTrue_ReturnsSuccess()
    {
        var attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsPositive));
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult(5, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void PredicateReturnsFalse_ReturnsFailure()
    {
        var attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsPositive));
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult(-1, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("IsPositive"));
    }

    [TestMethod]
    public void NullValuePassedToPredicate_PredicateDecides()
    {
        var attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsNotNull));
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void StringPredicate_ValidValue_ReturnsSuccess()
    {
        var attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsUpperCase));
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("HELLO", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void StringPredicate_InvalidValue_ReturnsFailure()
    {
        var attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsUpperCase));
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("hello", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void NonExistentMethod_ReturnsFailure()
    {
        var attribute = new PredicateValidationAttribute(typeof(TestPredicates), "NonExistent");
        var context = CreateContext(nameof(TestModel.Value));

        var result = attribute.GetValidationResult("test", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("not found"));
    }

    [TestMethod]
    public void Constructor_NullType_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new PredicateValidationAttribute(null!, "Method"));
    }

    [TestMethod]
    public void Constructor_NullMethodName_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new PredicateValidationAttribute(typeof(TestPredicates), null!));
    }

    [TestMethod]
    public void ValidatorTypeAndMethodName_AreExposed()
    {
        var attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsPositive));

        Assert.AreEqual(typeof(TestPredicates), attribute.ValidatorType);
        Assert.AreEqual(nameof(TestPredicates.IsPositive), attribute.MethodName);
    }

    private static ValidationContext CreateContext(string memberName)
    {
        var model = new TestModel();
        return new ValidationContext(model) { MemberName = memberName, DisplayName = memberName };
    }

    private sealed class TestModel
    {
        public object? Value { get; set; }
    }

    public static class TestPredicates
    {
        public static bool IsPositive(object? value) =>
            value is int i && i > 0;

        public static bool IsNotNull(object? value) =>
            value is not null;

        public static bool IsUpperCase(object? value) =>
            value is string s && s == s.ToUpperInvariant();
    }
}
