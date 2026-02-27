using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class RangeIfAttributeTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_DoubleRange_SetsProperties()
    {
        RangeIfAttribute attr = new RangeIfAttribute("Prop", true, 0.0, 1.0);

        Assert.AreEqual(0.0, attr.Minimum);
        Assert.AreEqual(1.0, attr.Maximum);
    }

    [TestMethod]
    public void Constructor_IntRange_SetsProperties()
    {
        RangeIfAttribute attr = new RangeIfAttribute("Prop", true, 1, 100);

        Assert.AreEqual("Prop", attr.DependentProperty);
        Assert.AreEqual(true, attr.TargetValue);
        Assert.AreEqual(1, attr.Minimum);
        Assert.AreEqual(100, attr.Maximum);
    }

    [TestMethod]
    public void Constructor_NullDependentProperty_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(() => new RangeIfAttribute(null!, true, 1, 100)); }
    [TestMethod]
    public void FormatErrorMessage_ContainsAllParameters()
    {
        RangeIfAttribute attr = new RangeIfAttribute("Flag", true, 1, 100);

        string msg = attr.FormatErrorMessage("Score");

        Assert.IsTrue(msg.Contains("Score"));
        Assert.IsTrue(msg.Contains("1"));
        Assert.IsTrue(msg.Contains("100"));
        Assert.IsTrue(msg.Contains("Flag"));
    }

    [TestMethod]
    public void Validate_ConditionMet_BoundaryMax_Passes()
    {
        TestModel model = new TestModel { EnforceRange = true, Score = 100 };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Score, context, null));
    }

    [TestMethod]
    public void Validate_ConditionMet_BoundaryMin_Passes()
    {
        TestModel model = new TestModel { EnforceRange = true, Score = 1 };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Score, context, null));
    }

    [TestMethod]
    public void Validate_ConditionMet_InRange_Passes()
    {
        TestModel model = new TestModel { EnforceRange = true, Score = 50 };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };

        bool result = Validator.TryValidateProperty(model.Score, context, null);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Validate_ConditionMet_OutOfRange_Fails()
    {
        TestModel model = new TestModel { EnforceRange = true, Score = 200 };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };
        List<ValidationResult> results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateProperty(model.Score, context, results);

        Assert.IsFalse(isValid);
        Assert.AreEqual(1, results.Count);
    }

    [TestMethod]
    public void Validate_ConditionNotMet_OutOfRange_Passes()
    {
        TestModel model = new TestModel { EnforceRange = false, Score = 999 };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };

        bool result = Validator.TryValidateProperty(model.Score, context, null);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Validate_DoubleRange_ConditionMet_InRange_Passes()
    {
        DoubleModel model = new DoubleModel { EnforceRange = true, Rate = 0.5 };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(DoubleModel.Rate) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Rate, context, null));
    }

    [TestMethod]
    public void Validate_DoubleRange_ConditionMet_OutOfRange_Fails()
    {
        DoubleModel model = new DoubleModel { EnforceRange = true, Rate = 1.5 };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(DoubleModel.Rate) };
        List<ValidationResult> results = new List<ValidationResult>();

        Assert.IsFalse(Validator.TryValidateProperty(model.Rate, context, results));
        Assert.AreEqual(1, results.Count);
    }

    [TestMethod]
    public void Validate_NonExistentDependentProperty_Passes()
    {
        RangeIfAttribute attr = new RangeIfAttribute("NonExistent", true, 1, 100);
        TestModel model = new TestModel { EnforceRange = true, Score = 999 };
        ValidationContext context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };

        ValidationResult? result = attr.GetValidationResult(model.Score, context);

        Assert.AreSame(ValidationResult.Success, result);
    }
    #endregion

    sealed class TestModel
    {
        #region Public properties
        public bool EnforceRange { get; set; }

        [RangeIf(nameof(EnforceRange), true, 1, 100)]
        public int Score { get; set; }
        #endregion
    }

    sealed class DoubleModel
    {
        #region Public properties
        public bool EnforceRange { get; set; }

        [RangeIf(nameof(EnforceRange), true, 0.0, 1.0)]
        public double Rate { get; set; }
        #endregion
    }
}
