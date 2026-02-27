using System.ComponentModel.DataAnnotations;

using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class ValidationPipelineTests
{
    [TestMethod]
    public void Initial_State_IsEmpty()
    {
        var pipeline = new ValidationPipeline();

        Assert.AreEqual(0, pipeline.Count);
        Assert.IsFalse(pipeline.StopOnFirstError);
    }

    [TestMethod]
    public void AddRule_IncreasesCount()
    {
        var pipeline = new ValidationPipeline();

        pipeline.AddRule(new PassingRule());

        Assert.AreEqual(1, pipeline.Count);
    }

    [TestMethod]
    public void AddRule_NullRule_ThrowsArgumentNullException()
    {
        var pipeline = new ValidationPipeline();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => pipeline.AddRule(null!));
    }

    [TestMethod]
    public void AddRule_ReturnsSelf_ForFluentChaining()
    {
        var pipeline = new ValidationPipeline();

        var result = pipeline.AddRule(new PassingRule());

        Assert.AreSame(pipeline, result);
    }

    [TestMethod]
    public void AddRules_AddsMultipleRules()
    {
        var pipeline = new ValidationPipeline();

        pipeline.AddRules([new PassingRule(), new PassingRule()]);

        Assert.AreEqual(2, pipeline.Count);
    }

    [TestMethod]
    public void AddRules_NullRules_ThrowsArgumentNullException()
    {
        var pipeline = new ValidationPipeline();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => pipeline.AddRules(null!));
    }

    [TestMethod]
    public void Execute_AllRulesPass_AggregatorHasNoErrors()
    {
        var pipeline = new ValidationPipeline();
        pipeline.AddRule(new PassingRule());
        pipeline.AddRule(new PassingRule());

        var context = new ValidationContext(new object());
        var aggregator = pipeline.Execute("value", context);

        Assert.IsFalse(aggregator.HasErrors);
        Assert.AreEqual(0, aggregator.Count);
    }

    [TestMethod]
    public void Execute_RuleFails_AggregatorContainsError()
    {
        var pipeline = new ValidationPipeline();
        pipeline.AddRule(new FailingRule("Something wrong."));

        var context = new ValidationContext(new object());
        var aggregator = pipeline.Execute("value", context);

        Assert.IsTrue(aggregator.HasErrors);
        Assert.AreEqual(1, aggregator.Count);
    }

    [TestMethod]
    public void Execute_NullContext_ThrowsArgumentNullException()
    {
        var pipeline = new ValidationPipeline();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => pipeline.Execute("value", null!));
    }

    [TestMethod]
    public void Execute_StopOnFirstError_StopsAfterFirstError()
    {
        var secondRuleEvaluated = false;
        var pipeline = new ValidationPipeline { StopOnFirstError = true };
        pipeline.AddRule(new FailingRule("First error"));
        pipeline.AddRule(new TrackingRule(() => secondRuleEvaluated = true));

        var context = new ValidationContext(new object());
        pipeline.Execute("value", context);

        Assert.IsFalse(secondRuleEvaluated);
    }

    [TestMethod]
    public void Execute_StopOnFirstError_False_EvaluatesAllRules()
    {
        var secondRuleEvaluated = false;
        var pipeline = new ValidationPipeline { StopOnFirstError = false };
        pipeline.AddRule(new FailingRule("First error"));
        pipeline.AddRule(new TrackingRule(() => secondRuleEvaluated = true));

        var context = new ValidationContext(new object());
        pipeline.Execute("value", context);

        Assert.IsTrue(secondRuleEvaluated);
    }

    [TestMethod]
    public void Execute_StopOnFirstError_WarningDoesNotStop()
    {
        var secondRuleEvaluated = false;
        var pipeline = new ValidationPipeline { StopOnFirstError = true };
        pipeline.AddRule(new FailingRule("Warning", ValidationSeverity.Warning));
        pipeline.AddRule(new TrackingRule(() => secondRuleEvaluated = true));

        var context = new ValidationContext(new object());
        pipeline.Execute("value", context);

        Assert.IsTrue(secondRuleEvaluated);
    }

    [TestMethod]
    public void IsValid_AllPass_ReturnsTrue()
    {
        var pipeline = new ValidationPipeline();
        pipeline.AddRule(new PassingRule());

        var context = new ValidationContext(new object());

        Assert.IsTrue(pipeline.IsValid("value", context));
    }

    [TestMethod]
    public void IsValid_ErrorRule_ReturnsFalse()
    {
        var pipeline = new ValidationPipeline();
        pipeline.AddRule(new FailingRule("error"));

        var context = new ValidationContext(new object());

        Assert.IsFalse(pipeline.IsValid("value", context));
    }

    [TestMethod]
    public void Clear_RemovesAllRules()
    {
        var pipeline = new ValidationPipeline();
        pipeline.AddRule(new PassingRule());
        pipeline.AddRule(new PassingRule());

        pipeline.Clear();

        Assert.AreEqual(0, pipeline.Count);
    }

    private sealed class PassingRule : IValidationRule
    {
        public ValidationScope Scope => ValidationScope.Property;
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public ValidationResult? Validate(object? value, ValidationContext context) =>
            ValidationResult.Success;
    }

    private sealed class FailingRule(string message, ValidationSeverity severity = ValidationSeverity.Error) : IValidationRule
    {
        public ValidationScope Scope => ValidationScope.Property;
        public ValidationSeverity Severity => severity;
        public ValidationResult? Validate(object? value, ValidationContext context) =>
            new(message);
    }

    private sealed class TrackingRule(Action onValidate) : IValidationRule
    {
        public ValidationScope Scope => ValidationScope.Property;
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public ValidationResult? Validate(object? value, ValidationContext context)
        {
            onValidate();
            return ValidationResult.Success;
        }
    }
}
