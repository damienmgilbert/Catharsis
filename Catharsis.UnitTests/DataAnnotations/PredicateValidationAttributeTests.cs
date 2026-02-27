using System.ComponentModel.DataAnnotations;
using Catharsis.DataAnnotations;

namespace Catharsis.UnitTests.DataAnnotations;

[TestClass]
public class PredicateValidationAttributeTests
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
    public void Constructor_NullMethodName_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new PredicateValidationAttribute(typeof(TestPredicates), null!)); }
    [TestMethod]
    public void Constructor_NullType_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new PredicateValidationAttribute(null!, "Method")); }
    [TestMethod]
    public void NonExistentMethod_ReturnsFailure()
    {
        PredicateValidationAttribute attribute = new PredicateValidationAttribute(typeof(TestPredicates), "NonExistent");
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("test", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("not found"));
    }

    [TestMethod]
    public void NullValuePassedToPredicate_PredicateDecides()
    {
        PredicateValidationAttribute attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsNotNull));
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult(null, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void PredicateReturnsFalse_ReturnsFailure()
    {
        PredicateValidationAttribute attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsPositive));
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult(-1, context);

        Assert.AreNotEqual(ValidationResult.Success, result);
        Assert.IsTrue(result!.ErrorMessage!.Contains("IsPositive"));
    }

    [TestMethod]
    public void PredicateReturnsTrue_ReturnsSuccess()
    {
        PredicateValidationAttribute attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsPositive));
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult(5, context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void StringPredicate_InvalidValue_ReturnsFailure()
    {
        PredicateValidationAttribute attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsUpperCase));
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("hello", context);

        Assert.AreNotEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void StringPredicate_ValidValue_ReturnsSuccess()
    {
        PredicateValidationAttribute attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsUpperCase));
        ValidationContext context = CreateContext(nameof(TestModel.Value));

        ValidationResult? result = attribute.GetValidationResult("HELLO", context);

        Assert.AreEqual(ValidationResult.Success, result);
    }

    [TestMethod]
    public void ValidatorTypeAndMethodName_AreExposed()
    {
        PredicateValidationAttribute attribute = new PredicateValidationAttribute(typeof(TestPredicates), nameof(TestPredicates.IsPositive));

        Assert.AreEqual(typeof(TestPredicates), attribute.ValidatorType);
        Assert.AreEqual(nameof(TestPredicates.IsPositive), attribute.MethodName);
    }
    #endregion

    sealed class TestModel
    {
        #region Public properties
        public object? Value { get; set; }
        #endregion
    }

    public static class TestPredicates
    {
        #region Public methods
        public static bool IsNotNull(object? value) { return value is not null; }
        public static bool IsPositive(object? value) { return (value is int i) && (i > 0); }
        public static bool IsUpperCase(object? value) { return (value is string s) && (s == s.ToUpperInvariant()); }
        #endregion
    }
}
