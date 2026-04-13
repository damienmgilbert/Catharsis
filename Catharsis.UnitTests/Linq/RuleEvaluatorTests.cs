using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="RuleEvaluator"/> class.
///</summary>
[TestClass]
public class RuleEvaluatorTests
{
    #region Helpers

    private static RuleSet<int> CreateStandardRuleSet()
    {
        RuleSet<int> set = [];
        set.Add("IsPositive", static x => x > 0, priority: 1);
        set.Add("IsEven", static x => x % 2 == 0, priority: 2);
        return set;
    }

    #endregion

    #region Evaluate (IEnumerable)

    [TestMethod]
    public void Evaluate_ReturnsContextForEveryElement()
    {
        int[] source = [1, -2, 3, 0];
        RuleSet<int> rules = CreateStandardRuleSet();

        List<RuleContext<int>> contexts = source.Evaluate(rules).ToList();

        Assert.HasCount(4, contexts);
    }

    [TestMethod]
    public void Evaluate_MatchingElement_HasMatchIsTrue()
    {
        int[] source = [5];
        RuleSet<int> rules = CreateStandardRuleSet();

        RuleContext<int> context = source.Evaluate(rules).First();

        Assert.IsTrue(context.HasMatch);
        Assert.AreEqual(5, context.Element);
    }

    [TestMethod]
    public void Evaluate_NonMatchingElement_HasMatchIsFalse()
    {
        int[] source = [-3];
        RuleSet<int> rules = new()
        {
            { "IsPositive", static x => x > 0 },
            { "IsEven", static x => x % 2 == 0 }
        };

        RuleContext<int> context = source.Evaluate(rules).First();

        Assert.IsFalse(context.HasMatch);
    }

    [TestMethod]
    public void Evaluate_MultipleRulesMatch_AllRecorded()
    {
        int[] source = [4]; // positive AND even
        RuleSet<int> rules = CreateStandardRuleSet();

        RuleContext<int> context = source.Evaluate(rules).First();

        Assert.AreEqual(2, context.MatchCount);
    }

    [TestMethod]
    public void Evaluate_StopOnMatch_StopsAfterFirstMatch()
    {
        RuleSet<int> rules =
        [
            new Rule<int>("First", static x => x > 0) { StopOnMatch = true, Priority = 1 },
            new Rule<int>("Second", static x => x % 2 == 0) { Priority = 2 },
        ];

        int[] source = [4]; // matches both, but should stop after First
        RuleContext<int> context = source.Evaluate(rules).First();

        Assert.AreEqual(1, context.MatchCount);
        Assert.AreEqual("First", context.FirstMatch!.Name);
        Assert.IsTrue(context.WasStopped);
    }

    [TestMethod]
    public void Evaluate_OnMatchAction_IsInvoked()
    {
        List<int> matched = [];
        RuleSet<int> rules = [new Rule<int>("Tracker", x => x > 0) { OnMatch = x => matched.Add(x) }];

        int[] source = [1, -2, 3];
        _ = source.Evaluate(rules).ToList(); // Force enumeration

        CollectionAssert.AreEqual(new[] { 1, 3 }, matched);
    }

    [TestMethod]
    public void Evaluate_DisabledRule_IsSkipped()
    {
        RuleSet<int> rules =
        [
            new Rule<int>("Enabled", static x => x > 0) { IsEnabled = true },
            new Rule<int>("Disabled", static x => true) { IsEnabled = false },
        ];

        int[] source = [5];
        RuleContext<int> context = source.Evaluate(rules).First();

        Assert.AreEqual(1, context.MatchCount);
        Assert.AreEqual("Enabled", context.FirstMatch!.Name);
    }

    [TestMethod]
    public void Evaluate_NullSource_ThrowsArgumentNullException()
    {
        IEnumerable<int> source = null!;

        Assert.ThrowsExactly<ArgumentNullException>(() => source.Evaluate([]).ToList());
    }

    [TestMethod]
    public void Evaluate_NullRuleSet_ThrowsArgumentNullException()
    {
        int[] source = [1];

        Assert.ThrowsExactly<ArgumentNullException>(() => source.Evaluate(null!).ToList());
    }

    #endregion

    #region WhereAnyRuleMatches (IEnumerable)

    [TestMethod]
    public void WhereAnyRuleMatches_ReturnsElementsMatchingAtLeastOneRule()
    {
        int[] source = [1, -2, 3, -3];
        RuleSet<int> rules = CreateStandardRuleSet();

        List<int> result = source.WhereAnyRuleMatches(rules).ToList();

        // 1: positive, -2: even, 3: positive, -3: no match
        CollectionAssert.AreEqual(new[] { 1, -2, 3 }, result);
    }

    [TestMethod]
    public void WhereAnyRuleMatches_NullSource_ThrowsArgumentNullException()
    {
        IEnumerable<int> source = null!;

        Assert.ThrowsExactly<ArgumentNullException>(() => source.WhereAnyRuleMatches([]).ToList());
    }

    #endregion

    #region WhereAllRulesMatch (IEnumerable)

    [TestMethod]
    public void WhereAllRulesMatch_ReturnsElementsMatchingAllRules()
    {
        int[] source = [1, 2, 3, 4, -2];
        RuleSet<int> rules = CreateStandardRuleSet();

        List<int> result = source.WhereAllRulesMatch(rules).ToList();

        // Must be positive AND even: 2, 4
        CollectionAssert.AreEqual(new[] { 2, 4 }, result);
    }

    [TestMethod]
    public void WhereAllRulesMatch_EmptyRuleSet_ReturnsAll()
    {
        int[] source = [1, 2, 3];
        RuleSet<int> rules = [];

        List<int> result = source.WhereAllRulesMatch(rules).ToList();

        // 0 enabled rules => MatchCount == 0 == enabledCount for all
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    #endregion

    #region GroupByFirstMatch

    [TestMethod]
    public void GroupByFirstMatch_GroupsElementsByFirstMatchingRuleName()
    {
        int[] source = [2, 3, -4, -3];
        RuleSet<int> rules = CreateStandardRuleSet();

        // priority order: IsPositive(1), IsEven(2)
        // 2: IsPositive first, 3: IsPositive, -4: IsEven, -3: no match
        ILookup<string?, int> grouped = source.GroupByFirstMatch(rules);

        CollectionAssert.AreEqual(new[] { 2, 3 }, grouped["IsPositive"].ToList());
        CollectionAssert.AreEqual(new[] { -4 }, grouped["IsEven"].ToList());
        CollectionAssert.AreEqual(new[] { -3 }, grouped[null].ToList());
    }

    #endregion

    #region ProjectWithMatches

    [TestMethod]
    public void ProjectWithMatches_ReturnsElementsWithMatchedRules()
    {
        int[] source = [4, -3];
        RuleSet<int> rules = CreateStandardRuleSet();

        List<(int Element, IReadOnlyList<Rule<int>> MatchedRules)> result =
            source.ProjectWithMatches(rules).ToList();

        Assert.HasCount(2, result);

        // 4 matches both IsPositive and IsEven
        Assert.AreEqual(4, result[0].Element);
        Assert.HasCount(2, result[0].MatchedRules);

        // -3 matches nothing
        Assert.AreEqual(-3, result[1].Element);
        Assert.IsEmpty(result[1].MatchedRules);
    }

    #endregion

    #region IQueryable WhereAllRulesMatch

    [TestMethod]
    public void WhereAllRulesMatch_Queryable_FiltersWithAndPredicate()
    {
        int[] source = [1, 2, 3, 4, -2];
        IQueryable<int> queryable = source.AsQueryable();
        RuleSet<int> rules = CreateStandardRuleSet();

        List<int> result = queryable.WhereAllRulesMatch(rules).ToList();

        CollectionAssert.AreEqual(new[] { 2, 4 }, result);
    }

    [TestMethod]
    public void WhereAllRulesMatch_Queryable_NullSource_ThrowsArgumentNullException()
    {
        IQueryable<int> source = null!;

        Assert.ThrowsExactly<ArgumentNullException>(() => source.WhereAllRulesMatch([]));
    }

    #endregion

    #region IQueryable WhereAnyRuleMatches

    [TestMethod]
    public void WhereAnyRuleMatches_Queryable_FiltersWithOrPredicate()
    {
        int[] source = [1, -2, -3];
        IQueryable<int> queryable = source.AsQueryable();
        RuleSet<int> rules = CreateStandardRuleSet();

        List<int> result = queryable.WhereAnyRuleMatches(rules).ToList();

        // 1: positive, -2: even, -3: no match
        CollectionAssert.AreEqual(new[] { 1, -2 }, result);
    }

    #endregion

    #region IQueryable WhereRuleMatches

    [TestMethod]
    public void WhereRuleMatches_Queryable_AppliesSingleRuleCondition()
    {
        int[] source = [1, -2, 3, -4];
        IQueryable<int> queryable = source.AsQueryable();
        RuleSet<int> rules = CreateStandardRuleSet();

        List<int> result = queryable.WhereRuleMatches(rules, "IsEven").ToList();

        CollectionAssert.AreEqual(new[] { -2, -4 }, result);
    }

    [TestMethod]
    public void WhereRuleMatches_Queryable_NonExistentRule_ThrowsInvalidOperationException()
    {
        IQueryable<int> queryable = new[] { 1 }.AsQueryable();
        RuleSet<int> rules = [];

        Assert.ThrowsExactly<InvalidOperationException>(() => queryable.WhereRuleMatches(rules, "NotFound"));
    }

    #endregion

    #region IQueryable WhereRulesWithTagsMatch

    [TestMethod]
    public void WhereRulesWithTagsMatch_Queryable_FiltersbyTaggedRuleConditions()
    {
        RuleSet<int> rules =
        [
            new Rule<int>("Positive", static x => x > 0) { Tags = new HashSet<string>(StringComparer.Ordinal) { "sign" } },
            new Rule<int>("Even", static x => x % 2 == 0) { Tags = new HashSet<string>(StringComparer.Ordinal) { "parity" } },
        ];

        int[] source = [1, -2, -3];
        IQueryable<int> queryable = source.AsQueryable();

        List<int> result = queryable.WhereRulesWithTagsMatch(rules, "sign").ToList();

        // Only Positive rule (tag "sign") → OR of just that = x > 0
        CollectionAssert.AreEqual(new[] { 1 }, result);
    }

    #endregion

    #region RuleContext Properties

    [TestMethod]
    public void RuleContext_Properties_CanStoreCustomData()
    {
        int[] source = [1];
        RuleSet<int> rules = new()
        {
            { "Always", static x => true }
        };

        RuleContext<int> context = source.Evaluate(rules).First();
        context.Properties["key"] = "value";

        Assert.AreEqual("value", context.Properties["key"]);
    }

    [TestMethod]
    public void RuleContext_NoMatch_FirstMatchIsNull()
    {
        int[] source = [-1];
        RuleSet<int> rules = new()
        {
            { "Positive", static x => x > 0 }
        };

        RuleContext<int> context = source.Evaluate(rules).First();

        Assert.IsNull(context.FirstMatch);
        Assert.IsFalse(context.HasMatch);
        Assert.AreEqual(0, context.MatchCount);
        Assert.IsFalse(context.WasStopped);
    }

    #endregion
}
