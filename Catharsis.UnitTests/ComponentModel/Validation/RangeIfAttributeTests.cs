using System.ComponentModel.DataAnnotations;

using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class RangeIfAttributeTests
{
    private sealed class TestModel
    {
        public bool EnforceRange { get; set; }

        [RangeIf(nameof(EnforceRange), true, 1, 100)]
        public int Score { get; set; }
    }

    private sealed class DoubleModel
    {
        public bool EnforceRange { get; set; }

        [RangeIf(nameof(EnforceRange), true, 0.0, 1.0)]
        public double Rate { get; set; }
    }

    [TestMethod]
    public void Constructor_NullDependentProperty_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new RangeIfAttribute(null!, true, 1, 100));
    }

    [TestMethod]
    public void Constructor_IntRange_SetsProperties()
    {
        var attr = new RangeIfAttribute("Prop", true, 1, 100);

        Assert.AreEqual("Prop", attr.DependentProperty);
        Assert.AreEqual(true, attr.TargetValue);
        Assert.AreEqual(1, attr.Minimum);
        Assert.AreEqual(100, attr.Maximum);
    }

    [TestMethod]
    public void Constructor_DoubleRange_SetsProperties()
    {
        var attr = new RangeIfAttribute("Prop", true, 0.0, 1.0);

        Assert.AreEqual(0.0, attr.Minimum);
        Assert.AreEqual(1.0, attr.Maximum);
    }

    [TestMethod]
    public void Validate_ConditionNotMet_OutOfRange_Passes()
    {
        var model = new TestModel { EnforceRange = false, Score = 999 };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };

        var result = Validator.TryValidateProperty(model.Score, context, null);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Validate_ConditionMet_InRange_Passes()
    {
        var model = new TestModel { EnforceRange = true, Score = 50 };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };

        var result = Validator.TryValidateProperty(model.Score, context, null);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Validate_ConditionMet_OutOfRange_Fails()
    {
        var model = new TestModel { EnforceRange = true, Score = 200 };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateProperty(model.Score, context, results);

        Assert.IsFalse(isValid);
        Assert.AreEqual(1, results.Count);
    }

    [TestMethod]
    public void Validate_ConditionMet_BoundaryMin_Passes()
    {
        var model = new TestModel { EnforceRange = true, Score = 1 };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Score, context, null));
    }

    [TestMethod]
    public void Validate_ConditionMet_BoundaryMax_Passes()
    {
        var model = new TestModel { EnforceRange = true, Score = 100 };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Score, context, null));
    }

    [TestMethod]
    public void Validate_DoubleRange_ConditionMet_InRange_Passes()
    {
        var model = new DoubleModel { EnforceRange = true, Rate = 0.5 };
        var context = new ValidationContext(model) { MemberName = nameof(DoubleModel.Rate) };

        Assert.IsTrue(Validator.TryValidateProperty(model.Rate, context, null));
    }

    [TestMethod]
    public void Validate_DoubleRange_ConditionMet_OutOfRange_Fails()
    {
        var model = new DoubleModel { EnforceRange = true, Rate = 1.5 };
        var context = new ValidationContext(model) { MemberName = nameof(DoubleModel.Rate) };
        var results = new List<ValidationResult>();

        Assert.IsFalse(Validator.TryValidateProperty(model.Rate, context, results));
        Assert.AreEqual(1, results.Count);
    }

    [TestMethod]
    public void FormatErrorMessage_ContainsAllParameters()
    {
        var attr = new RangeIfAttribute("Flag", true, 1, 100);

        var msg = attr.FormatErrorMessage("Score");

        Assert.IsTrue(msg.Contains("Score"));
        Assert.IsTrue(msg.Contains("1"));
        Assert.IsTrue(msg.Contains("100"));
        Assert.IsTrue(msg.Contains("Flag"));
    }

    [TestMethod]
    public void Validate_NonExistentDependentProperty_Passes()
    {
        var attr = new RangeIfAttribute("NonExistent", true, 1, 100);
        var model = new TestModel { EnforceRange = true, Score = 999 };
        var context = new ValidationContext(model) { MemberName = nameof(TestModel.Score) };

        var result = attr.GetValidationResult(model.Score, context);

        Assert.AreSame(ValidationResult.Success, result);
    }
}
