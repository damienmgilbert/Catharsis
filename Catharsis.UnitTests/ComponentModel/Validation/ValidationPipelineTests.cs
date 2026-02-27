using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class ValidationPipelineTests
{
    #region Public methods
    [TestMethod]
    public void AddRule_IncreasesCount()
    {
        ValidationPipeline pipeline = new ValidationPipeline();

        pipeline.AddRule(new PassingRule());

        Assert.AreEqual(1, pipeline.Count);
    }

    [TestMethod]
    public void AddRule_NullRule_ThrowsArgumentNullException()
    {
        ValidationPipeline pipeline = new ValidationPipeline();

        Assert.ThrowsExactly<ArgumentNullException>(() => pipeline.AddRule(null!));
    }

    [TestMethod]
    public void AddRule_ReturnsSelf_ForFluentChaining()
    {
        ValidationPipeline pipeline = new ValidationPipeline();

        ValidationPipeline result = pipeline.AddRule(new PassingRule());

        Assert.AreSame(pipeline, result);
    }

    [TestMethod]
    public void AddRules_AddsMultipleRules()
    {
        ValidationPipeline pipeline = new ValidationPipeline();

        pipeline.AddRules([ new PassingRule(), new PassingRule() ]);

        Assert.AreEqual(2, pipeline.Count);
    }

    [TestMethod]
    public void AddRules_NullRules_ThrowsArgumentNullException()
    {
        ValidationPipeline pipeline = new ValidationPipeline();

        Assert.ThrowsExactly<ArgumentNullException>(() => pipeline.AddRules(null!));
    }

    [TestMethod]
    public void Clear_RemovesAllRules()
    {
        ValidationPipeline pipeline = new ValidationPipeline();
        pipeline.AddRule(new PassingRule());
        pipeline.AddRule(new PassingRule());

        pipeline.Clear();

        Assert.AreEqual(0, pipeline.Count);
    }

    [TestMethod]
    public void Execute_AllRulesPass_AggregatorHasNoErrors()
    {
        ValidationPipeline pipeline = new ValidationPipeline();
        pipeline.AddRule(new PassingRule());
        pipeline.AddRule(new PassingRule());

        ValidationContext context = new ValidationContext(new object());
        ValidationResultAggregator aggregator = pipeline.Execute("value", context);

        Assert.IsFalse(aggregator.HasErrors);
        Assert.AreEqual(0, aggregator.Count);
    }

    [TestMethod]
    public void Execute_NullContext_ThrowsArgumentNullException()
    {
        ValidationPipeline pipeline = new ValidationPipeline();

        Assert.ThrowsExactly<ArgumentNullException>(() => pipeline.Execute("value", null!));
    }

    [TestMethod]
    public void Execute_RuleFails_AggregatorContainsError()
    {
        ValidationPipeline pipeline = new ValidationPipeline();
        pipeline.AddRule(new FailingRule("Something wrong."));

        ValidationContext context = new ValidationContext(new object());
        ValidationResultAggregator aggregator = pipeline.Execute("value", context);

        Assert.IsTrue(aggregator.HasErrors);
        Assert.AreEqual(1, aggregator.Count);
    }

    [TestMethod]
    public void Execute_StopOnFirstError_False_EvaluatesAllRules()
    {
        bool secondRuleEvaluated = false;
        ValidationPipeline pipeline = new ValidationPipeline { StopOnFirstError = false };
        pipeline.AddRule(new FailingRule("First error"));
        pipeline.AddRule(new TrackingRule(() => secondRuleEvaluated = true));

        ValidationContext context = new ValidationContext(new object());
        pipeline.Execute("value", context);

        Assert.IsTrue(secondRuleEvaluated);
    }

    [TestMethod]
    public void Execute_StopOnFirstError_StopsAfterFirstError()
    {
        bool secondRuleEvaluated = false;
        ValidationPipeline pipeline = new ValidationPipeline { StopOnFirstError = true };
        pipeline.AddRule(new FailingRule("First error"));
        pipeline.AddRule(new TrackingRule(() => secondRuleEvaluated = true));

        ValidationContext context = new ValidationContext(new object());
        pipeline.Execute("value", context);

        Assert.IsFalse(secondRuleEvaluated);
    }

    [TestMethod]
    public void Execute_StopOnFirstError_WarningDoesNotStop()
    {
        bool secondRuleEvaluated = false;
        ValidationPipeline pipeline = new ValidationPipeline { StopOnFirstError = true };
        pipeline.AddRule(new FailingRule("Warning", ValidationSeverity.Warning));
        pipeline.AddRule(new TrackingRule(() => secondRuleEvaluated = true));

        ValidationContext context = new ValidationContext(new object());
        pipeline.Execute("value", context);

        Assert.IsTrue(secondRuleEvaluated);
    }

    [TestMethod]
    public void Initial_State_IsEmpty()
    {
        ValidationPipeline pipeline = new ValidationPipeline();

        Assert.AreEqual(0, pipeline.Count);
        Assert.IsFalse(pipeline.StopOnFirstError);
    }

    [TestMethod]
    public void IsValid_AllPass_ReturnsTrue()
    {
        ValidationPipeline pipeline = new ValidationPipeline();
        pipeline.AddRule(new PassingRule());

        ValidationContext context = new ValidationContext(new object());

        Assert.IsTrue(pipeline.IsValid("value", context));
    }

    [TestMethod]
    public void IsValid_ErrorRule_ReturnsFalse()
    {
        ValidationPipeline pipeline = new ValidationPipeline();
        pipeline.AddRule(new FailingRule("error"));

        ValidationContext context = new ValidationContext(new object());

        Assert.IsFalse(pipeline.IsValid("value", context));
    }
    #endregion

    sealed class PassingRule : IValidationRule
    {
        #region Public methods
        public ValidationResult? Validate(object? value, ValidationContext context) { return ValidationResult.Success; }
        #endregion

        #region Public properties
        public ValidationScope Scope => ValidationScope.Property;

        public ValidationSeverity Severity => ValidationSeverity.Error;
        #endregion
    }

    sealed class FailingRule(string message, ValidationSeverity severity = ValidationSeverity.Error) : IValidationRule
    {
        #region Public methods
        public ValidationResult? Validate(object? value, ValidationContext context) { return new(message); }
        #endregion

        #region Public properties
        public ValidationScope Scope => ValidationScope.Property;

        public ValidationSeverity Severity => severity;
        #endregion
    }

    sealed class TrackingRule(Action onValidate) : IValidationRule
    {
        #region Public methods
        public ValidationResult? Validate(object? value, ValidationContext context)
        {
            onValidate();
            return ValidationResult.Success;
        }
        #endregion

        #region Public properties
        public ValidationScope Scope => ValidationScope.Property;

        public ValidationSeverity Severity => ValidationSeverity.Error;
        #endregion
    }
}
