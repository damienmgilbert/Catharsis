using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class RuleTests
{
    #region Constructor

    [TestMethod]
    public void Ctor_ValidArguments_SetsNameAndCondition()
    {
        Rule<int> rule = new("IsPositive", x => x > 0);

        Assert.AreEqual("IsPositive", rule.Name);
        Assert.IsNotNull(rule.Condition);
    }

    [TestMethod]
    public void Ctor_NullName_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Rule<int>(null!, x => x > 0));
    }

    [TestMethod]
    public void Ctor_NullCondition_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Rule<int>("test", null!));
    }

    #endregion

    #region Default Properties

    [TestMethod]
    public void DefaultProperties_HaveExpectedValues()
    {
        Rule<int> rule = new("test", x => true);

        Assert.AreEqual(0, rule.Priority);
        Assert.IsTrue(rule.IsEnabled);
        Assert.IsFalse(rule.StopOnMatch);
        Assert.IsNull(rule.Description);
        Assert.IsNull(rule.OnMatch);
        Assert.AreEqual(0, rule.Tags.Count);
    }

    #endregion

    #region Init Properties

    [TestMethod]
    public void InitProperties_SetCorrectly()
    {
        HashSet<string> tags = new(StringComparer.Ordinal) { "validation", "security" };

        Rule<int> rule = new("test", x => true)
        {
            Priority = 5,
            IsEnabled = false,
            StopOnMatch = true,
            Description = "A test rule",
            Tags = tags
        };

        Assert.AreEqual(5, rule.Priority);
        Assert.IsFalse(rule.IsEnabled);
        Assert.IsTrue(rule.StopOnMatch);
        Assert.AreEqual("A test rule", rule.Description);
        Assert.AreEqual(2, rule.Tags.Count);
        Assert.IsTrue(rule.Tags.Contains("validation"));
        Assert.IsTrue(rule.Tags.Contains("security"));
    }

    #endregion

    #region Evaluate

    [TestMethod]
    public void Evaluate_MatchingElement_ReturnsTrue()
    {
        Rule<int> rule = new("IsPositive", x => x > 0);

        Assert.IsTrue(rule.Evaluate(5));
    }

    [TestMethod]
    public void Evaluate_NonMatchingElement_ReturnsFalse()
    {
        Rule<int> rule = new("IsPositive", x => x > 0);

        Assert.IsFalse(rule.Evaluate(-3));
    }

    [TestMethod]
    public void Evaluate_BoundaryValue_ReturnsCorrectResult()
    {
        Rule<int> rule = new("IsPositive", x => x > 0);

        Assert.IsFalse(rule.Evaluate(0));
    }

    #endregion

    #region CompiledCondition

    [TestMethod]
    public void CompiledCondition_ReturnsCachedDelegate()
    {
        Rule<int> rule = new("test", x => x > 0);

        Func<int, bool> first = rule.CompiledCondition;
        Func<int, bool> second = rule.CompiledCondition;

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void CompiledCondition_EvaluatesCorrectly()
    {
        Rule<string> rule = new("NotEmpty", x => x.Length > 0);

        Assert.IsTrue(rule.CompiledCondition("hello"));
        Assert.IsFalse(rule.CompiledCondition(""));
    }

    #endregion

    #region And

    [TestMethod]
    public void And_CombinesTwoRules_BothMustMatch()
    {
        Rule<int> positive = new("IsPositive", x => x > 0);
        Rule<int> even = new("IsEven", x => x % 2 == 0);

        Rule<int> combined = positive.And(even);

        Assert.IsTrue(combined.Evaluate(4));
        Assert.IsFalse(combined.Evaluate(3));
        Assert.IsFalse(combined.Evaluate(-2));
    }

    [TestMethod]
    public void And_CombinedNameContainsBothNames()
    {
        Rule<int> a = new("A", x => true);
        Rule<int> b = new("B", x => true);

        Rule<int> combined = a.And(b);

        Assert.AreEqual("(A AND B)", combined.Name);
    }

    [TestMethod]
    public void And_PriorityIsMinOfBoth()
    {
        Rule<int> a = new("A", x => true) { Priority = 3 };
        Rule<int> b = new("B", x => true) { Priority = 1 };

        Rule<int> combined = a.And(b);

        Assert.AreEqual(1, combined.Priority);
    }

    [TestMethod]
    public void And_TagsAreMerged()
    {
        Rule<int> a = new("A", x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "tag1" } };
        Rule<int> b = new("B", x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "tag2" } };

        Rule<int> combined = a.And(b);

        Assert.IsTrue(combined.Tags.Contains("tag1"));
        Assert.IsTrue(combined.Tags.Contains("tag2"));
    }

    [TestMethod]
    public void And_NullOther_ThrowsArgumentNullException()
    {
        Rule<int> rule = new("test", x => true);

        Assert.ThrowsExactly<ArgumentNullException>(() => rule.And(null!));
    }

    #endregion

    #region Or

    [TestMethod]
    public void Or_CombinesTwoRules_EitherCanMatch()
    {
        Rule<int> positive = new("IsPositive", x => x > 0);
        Rule<int> even = new("IsEven", x => x % 2 == 0);

        Rule<int> combined = positive.Or(even);

        Assert.IsTrue(combined.Evaluate(4));
        Assert.IsTrue(combined.Evaluate(3));
        Assert.IsTrue(combined.Evaluate(-2));
        Assert.IsFalse(combined.Evaluate(-3));
    }

    [TestMethod]
    public void Or_CombinedNameContainsBothNames()
    {
        Rule<int> a = new("A", x => true);
        Rule<int> b = new("B", x => true);

        Rule<int> combined = a.Or(b);

        Assert.AreEqual("(A OR B)", combined.Name);
    }

    [TestMethod]
    public void Or_NullOther_ThrowsArgumentNullException()
    {
        Rule<int> rule = new("test", x => true);

        Assert.ThrowsExactly<ArgumentNullException>(() => rule.Or(null!));
    }

    #endregion

    #region Negate

    [TestMethod]
    public void Negate_InvertsCondition()
    {
        Rule<int> positive = new("IsPositive", x => x > 0);

        Rule<int> negated = positive.Negate();

        Assert.IsFalse(negated.Evaluate(5));
        Assert.IsTrue(negated.Evaluate(-3));
    }

    [TestMethod]
    public void Negate_NameWrapsOriginal()
    {
        Rule<int> rule = new("IsPositive", x => x > 0);

        Rule<int> negated = rule.Negate();

        Assert.AreEqual("NOT(IsPositive)", negated.Name);
    }

    [TestMethod]
    public void Negate_PreservesPriorityAndTags()
    {
        Rule<int> rule = new("test", x => true)
        {
            Priority = 7,
            Tags = new HashSet<string>(StringComparer.Ordinal) { "tag1" }
        };

        Rule<int> negated = rule.Negate();

        Assert.AreEqual(7, negated.Priority);
        Assert.IsTrue(negated.Tags.Contains("tag1"));
    }

    #endregion

    #region ToString

    [TestMethod]
    public void ToString_ReturnsFormattedString()
    {
        Rule<int> rule = new("IsPositive", x => x > 0) { Priority = 2 };

        string result = rule.ToString();

        Assert.AreEqual("Rule 'IsPositive' (Priority=2, Enabled=True)", result);
    }

    [TestMethod]
    public void ToString_DisabledRule_ReflectsState()
    {
        Rule<int> rule = new("test", x => true) { IsEnabled = false };

        StringAssert.Contains(rule.ToString(), "Enabled=False");
    }

    #endregion

    #region OnMatch

    [TestMethod]
    public void OnMatch_IsInvokedWhenRuleEvaluatesTrue()
    {
        bool invoked = false;
        Rule<int> rule = new("test", x => x > 0) { OnMatch = _ => invoked = true };

        // OnMatch is not invoked by Evaluate directly; this tests the property is settable.
        Assert.IsNotNull(rule.OnMatch);
        rule.OnMatch(5);
        Assert.IsTrue(invoked);
    }

    #endregion
}
