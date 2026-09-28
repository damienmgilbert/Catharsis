using Catharsis.Configuration;

namespace Catharsis.UnitTests.Configuration;

///<summary>
///Unit tests for the <see cref="OptionsValidator{TOptions}"/> class.
///</summary>
[TestClass]
public class OptionsValidatorTests
{
    #region AddRule

    [TestMethod]
    public void AddRule_NullPredicate_Throws()
    {
        OptionsValidator<TestOptions> validator = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => validator.AddRule(null!, "message"));
    }

    [TestMethod]
    public void AddRule_NullMessage_Throws()
    {
        OptionsValidator<TestOptions> validator = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => validator.AddRule(static _ => true, null!));
    }

    [TestMethod]
    public void AddRule_ReturnsSameInstance_ForChaining()
    {
        OptionsValidator<TestOptions> validator = new();
        OptionsValidator<TestOptions> result = validator.AddRule(static _ => true, "a").AddRule(static _ => true, "b");
        Assert.AreSame(validator, result);
    }

    #endregion

    #region Validate

    [TestMethod]
    public void Validate_NullOptions_Throws()
    {
        IOptionsValidator<TestOptions> validator = new OptionsValidator<TestOptions>();
        Assert.ThrowsExactly<ArgumentNullException>(() => validator.Validate(null!));
    }

    [TestMethod]
    public void Validate_NoRules_Succeeds()
    {
        IOptionsValidator<TestOptions> validator = new OptionsValidator<TestOptions>();
        OptionsValidationResult result = validator.Validate(new TestOptions(5));

        Assert.IsTrue(result.Succeeded);
        Assert.IsEmpty(result.Failures);
    }

    [TestMethod]
    public void Validate_AllRulesPass_Succeeds()
    {
        IOptionsValidator<TestOptions> validator = new OptionsValidator<TestOptions>().AddRule(static o => o.Value > 0, "must be positive");
        OptionsValidationResult result = validator.Validate(new TestOptions(5));

        Assert.IsTrue(result.Succeeded);
    }

    [TestMethod]
    public void Validate_FailingRule_ReturnsFailureMessage()
    {
        IOptionsValidator<TestOptions> validator = new OptionsValidator<TestOptions>().AddRule(static o => o.Value > 0, "must be positive");
        OptionsValidationResult result = validator.Validate(new TestOptions(-1));

        Assert.IsFalse(result.Succeeded);
        CollectionAssert.Contains(result.Failures.ToList(), "must be positive");
    }

    [TestMethod]
    public void Validate_MultipleFailingRules_ReturnsAllMessages()
    {
        IOptionsValidator<TestOptions> validator = new OptionsValidator<TestOptions>()
            .AddRule(static o => o.Value > 0, "must be positive")
            .AddRule(static o => (o.Value % 2) == 0, "must be even");

        OptionsValidationResult result = validator.Validate(new TestOptions(-1));

        Assert.HasCount(2, result.Failures);
    }

    #endregion

    private sealed record TestOptions(int Value);
}
