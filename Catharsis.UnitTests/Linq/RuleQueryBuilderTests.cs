using Catharsis.Linq;
using System.Linq.Expressions;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="RuleQueryBuilder"/> class.
///</summary>
[TestClass]
public class RuleQueryBuilderTests
{
    #region Helpers

    private static RuleSet<int> CreateTaggedRuleSet()
    {
        RuleSet<int> set =
        [
            new Rule<int>("Positive", static x => x > 0) { Priority = 1, Tags = new HashSet<string>(StringComparer.Ordinal) { "sign" } },
            new Rule<int>("Even", static x => x % 2 == 0) { Priority = 2, Tags = new HashSet<string>(StringComparer.Ordinal) { "parity" } },
            new Rule<int>("LessThan10", static x => x < 10) { Priority = 3, Tags = new HashSet<string>(StringComparer.Ordinal) { "range" } },
        ];
        return set;
    }

    #endregion

    #region Constructor

    [TestMethod]
    public void Ctor_NullRuleSet_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => new RuleQueryBuilder<int>(null!));
    }

    [TestMethod]
    public void Ctor_ValidRuleSet_CreatesBuilder()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.AreEqual(0, builder.FilterCount);
    }

    #endregion

    #region WhereRule

    [TestMethod]
    public void WhereRule_AddsNamedRuleCondition()
    {
        RuleSet<int> rules = CreateTaggedRuleSet();
        RuleQueryBuilder<int> builder = new(rules);

        builder.WhereRule("Positive");

        Assert.AreEqual(1, builder.FilterCount);
        Assert.AreEqual("Positive", builder.ActiveRuleNames[0]);
    }

    [TestMethod]
    public void WhereRule_NonExistentRule_ThrowsInvalidOperationException()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.ThrowsExactly<InvalidOperationException>(() => builder.WhereRule("NotFound"));
    }

    [TestMethod]
    public void WhereRule_NullRuleName_ThrowsArgumentNullException()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.ThrowsExactly<ArgumentNullException>(() => builder.WhereRule(null!));
    }

    #endregion

    #region WhereAllRules

    [TestMethod]
    public void WhereAllRules_AddsAllEnabledRuleConditions()
    {
        RuleSet<int> rules = CreateTaggedRuleSet();
        RuleQueryBuilder<int> builder = new(rules);

        builder.WhereAllRules();

        Assert.AreEqual(3, builder.FilterCount);
        Assert.AreEqual(3, builder.ActiveRuleNames.Count);
    }

    [TestMethod]
    public void WhereAllRules_SkipsDisabledRules()
    {
        RuleSet<int> rules = new();
        rules.Add(new Rule<int>("Enabled", static x => true) { IsEnabled = true });
        rules.Add(new Rule<int>("Disabled", static x => true) { IsEnabled = false });

        RuleQueryBuilder<int> builder = new(rules);
        builder.WhereAllRules();

        Assert.AreEqual(1, builder.FilterCount);
    }

    #endregion

    #region WhereRulesWithTags

    [TestMethod]
    public void WhereRulesWithTags_AddsConditionsFromMatchingRules()
    {
        RuleSet<int> rules = CreateTaggedRuleSet();
        RuleQueryBuilder<int> builder = new(rules);

        builder.WhereRulesWithTags("sign", "parity");

        Assert.AreEqual(2, builder.FilterCount);
    }

    [TestMethod]
    public void WhereRulesWithTags_NullTags_ThrowsArgumentNullException()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.ThrowsExactly<ArgumentNullException>(() => builder.WhereRulesWithTags(null!));
    }

    #endregion

    #region Where

    [TestMethod]
    public void Where_AddsCustomPredicate()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        builder.Where(static x => x > 100);

        Assert.AreEqual(1, builder.FilterCount);
        Assert.AreEqual(0, builder.ActiveRuleNames.Count);
    }

    [TestMethod]
    public void Where_NullPredicate_ThrowsArgumentNullException()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.ThrowsExactly<ArgumentNullException>(() => builder.Where(null!));
    }

    #endregion

    #region WhereIf

    [TestMethod]
    public void WhereIf_ConditionTrue_AddsPredicate()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        builder.WhereIf(true, static x => x > 0);

        Assert.AreEqual(1, builder.FilterCount);
    }

    [TestMethod]
    public void WhereIf_ConditionFalse_DoesNotAddPredicate()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        builder.WhereIf(false, static x => x > 0);

        Assert.AreEqual(0, builder.FilterCount);
    }

    [TestMethod]
    public void WhereIf_NullPredicate_ThrowsArgumentNullException()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.ThrowsExactly<ArgumentNullException>(() => builder.WhereIf(true, null!));
    }

    #endregion

    #region Skip / Take

    [TestMethod]
    public void Skip_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => builder.Skip(-1));
    }

    [TestMethod]
    public void Take_ZeroCount_ThrowsArgumentOutOfRangeException()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => builder.Take(0));
    }

    #endregion

    #region BuildPredicate

    [TestMethod]
    public void BuildPredicate_AndMode_CombinesWithAnd()
    {
        RuleSet<int> rules = CreateTaggedRuleSet();
        RuleQueryBuilder<int> builder = new(rules);
        builder.WhereRule("Positive").WhereRule("Even");

        Expression<Func<int, bool>> predicate = builder.BuildPredicate();
        Func<int, bool> compiled = predicate.Compile();

        Assert.IsTrue(compiled(4));   // positive AND even
        Assert.IsFalse(compiled(3));  // positive but odd
        Assert.IsFalse(compiled(-2)); // even but negative
    }

    [TestMethod]
    public void BuildPredicate_OrMode_CombinesWithOr()
    {
        RuleSet<int> rules = CreateTaggedRuleSet();
        RuleQueryBuilder<int> builder = new(rules);
        builder.WithCombineMode(FilterCombineMode.Or);
        builder.WhereRule("Positive").WhereRule("Even");

        Expression<Func<int, bool>> predicate = builder.BuildPredicate();
        Func<int, bool> compiled = predicate.Compile();

        Assert.IsTrue(compiled(3));   // positive
        Assert.IsTrue(compiled(-2));  // even
        Assert.IsFalse(compiled(-3)); // neither
    }

    [TestMethod]
    public void BuildPredicate_NoFilters_AndMode_ReturnsTrue()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Func<int, bool> compiled = builder.BuildPredicate().Compile();

        Assert.IsTrue(compiled(42));
    }

    #endregion

    #region Apply (IEnumerable)

    [TestMethod]
    public void Apply_Enumerable_FiltersElements()
    {
        RuleSet<int> rules = CreateTaggedRuleSet();
        RuleQueryBuilder<int> builder = new(rules);
        builder.WhereRule("Positive").WhereRule("Even");

        int[] source = [1, 2, 3, 4, -2, -4];
        List<int> result = builder.Apply((IEnumerable<int>)source).ToList();

        CollectionAssert.AreEqual(new[] { 2, 4 }, result);
    }

    [TestMethod]
    public void Apply_Enumerable_WithSkipAndTake()
    {
        RuleSet<int> rules = new();
        rules.Add("Always", static x => true);

        RuleQueryBuilder<int> builder = new(rules);
        builder.WhereAllRules().Skip(1).Take(2);

        int[] source = [10, 20, 30, 40, 50];
        List<int> result = builder.Apply((IEnumerable<int>)source).ToList();

        CollectionAssert.AreEqual(new[] { 20, 30 }, result);
    }

    [TestMethod]
    public void Apply_Enumerable_NullSource_ThrowsArgumentNullException()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.ThrowsExactly<ArgumentNullException>(() => builder.Apply((IEnumerable<int>)null!));
    }

    #endregion

    #region Apply (IQueryable)

    [TestMethod]
    public void Apply_Queryable_FiltersElements()
    {
        RuleSet<int> rules = CreateTaggedRuleSet();
        RuleQueryBuilder<int> builder = new(rules);
        builder.WhereRule("Positive").WhereRule("LessThan10");

        IQueryable<int> source = new[] { -5, 1, 5, 10, 15 }.AsQueryable();
        List<int> result = builder.Apply(source).ToList();

        CollectionAssert.AreEqual(new[] { 1, 5 }, result);
    }

    [TestMethod]
    public void Apply_Queryable_WithSkipAndTake()
    {
        RuleSet<int> rules = new();
        rules.Add("Always", static x => true);

        RuleQueryBuilder<int> builder = new(rules);
        builder.WhereAllRules().Skip(2).Take(2);

        IQueryable<int> source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();
        List<int> result = builder.Apply(source).ToList();

        CollectionAssert.AreEqual(new[] { 3, 4 }, result);
    }

    [TestMethod]
    public void Apply_Queryable_NullSource_ThrowsArgumentNullException()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.ThrowsExactly<ArgumentNullException>(() => builder.Apply((IQueryable<int>)null!));
    }

    #endregion

    #region ApplyAndProject

    [TestMethod]
    public void ApplyAndProject_Enumerable_ProjectsFilteredElements()
    {
        RuleSet<int> rules = new();
        rules.Add("Positive", static x => x > 0);

        RuleQueryBuilder<int> builder = new(rules);
        builder.WhereAllRules();

        int[] source = [-1, 2, 3];
        List<string> result = builder.ApplyAndProject((IEnumerable<int>)source, static x => $"val:{x}").ToList();

        CollectionAssert.AreEqual(new[] { "val:2", "val:3" }, result);
    }

    [TestMethod]
    public void ApplyAndProject_Queryable_ProjectsFilteredElements()
    {
        RuleSet<int> rules = new();
        rules.Add("Positive", static x => x > 0);

        RuleQueryBuilder<int> builder = new(rules);
        builder.WhereAllRules();

        IQueryable<int> source = new[] { -1, 2, 3 }.AsQueryable();
        Expression<Func<int, string>> selector = static x => "val:" + x;
        List<string> result = builder.ApplyAndProject(source, selector).ToList();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void ApplyAndProject_Enumerable_NullSelector_ThrowsArgumentNullException()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.ThrowsExactly<ArgumentNullException>(() => builder.ApplyAndProject((IEnumerable<int>)[], (Func<int, string>)null!));
    }

    #endregion

    #region ApplyAndGroupByRule

    [TestMethod]
    public void ApplyAndGroupByRule_GroupsFilteredElementsByFirstMatchingRule()
    {
        RuleSet<int> rules = new();
        rules.Add("Positive", static x => x > 0, priority: 1);
        rules.Add("Even", static x => x % 2 == 0, priority: 2);

        RuleQueryBuilder<int> builder = new(rules);
        builder.WithCombineMode(FilterCombineMode.Or).WhereAllRules();

        int[] source = [2, 3, -4, -3];
        ILookup<string, int> grouped = builder.ApplyAndGroupByRule(source);

        // 2 matches Positive first, 3 matches Positive, -4 matches Even
        // -3 is filtered out (no rule matches in OR mode)
        Assert.IsTrue(grouped["Positive"].Contains(2));
        Assert.IsTrue(grouped["Positive"].Contains(3));
        Assert.IsTrue(grouped["Even"].Contains(-4));
    }

    [TestMethod]
    public void ApplyAndGroupByRule_NullSource_ThrowsArgumentNullException()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.ThrowsExactly<ArgumentNullException>(() => builder.ApplyAndGroupByRule(null!));
    }

    #endregion

    #region Clear

    [TestMethod]
    public void Clear_ResetsAllState()
    {
        RuleSet<int> rules = CreateTaggedRuleSet();
        RuleQueryBuilder<int> builder = new(rules);
        builder.WhereAllRules().Skip(5).Take(10);

        builder.Clear();

        Assert.AreEqual(0, builder.FilterCount);
        Assert.AreEqual(0, builder.ActiveRuleNames.Count);
    }

    [TestMethod]
    public void Clear_ReturnsSameInstance()
    {
        RuleQueryBuilder<int> builder = new(new RuleSet<int>());

        Assert.AreSame(builder, builder.Clear());
    }

    #endregion

    #region Fluent Chaining

    [TestMethod]
    public void FluentChaining_AllMethodsReturnSameBuilder()
    {
        RuleSet<int> rules = CreateTaggedRuleSet();
        RuleQueryBuilder<int> builder = new(rules);

        RuleQueryBuilder<int> result = builder
            .WithCombineMode(FilterCombineMode.And)
            .WhereRule("Positive")
            .WhereRulesWithTags("parity")
            .Where(static x => x < 100)
            .WhereIf(true, static x => x > -100)
            .Skip(0)
            .Take(50);

        Assert.AreSame(builder, result);
    }

    #endregion

    #region WithCombineMode

    [TestMethod]
    public void WithCombineMode_AffectsPredicateCombination()
    {
        RuleSet<int> rules = new();
        rules.Add("Positive", static x => x > 0);
        rules.Add("Even", static x => x % 2 == 0);

        // AND mode
        RuleQueryBuilder<int> andBuilder = new(rules);
        andBuilder.WhereAllRules();
        List<int> andResult = andBuilder.Apply((IEnumerable<int>)new[] { 1, 2, 3, 4, -2 }).ToList();

        // OR mode
        RuleQueryBuilder<int> orBuilder = new(rules);
        orBuilder.WithCombineMode(FilterCombineMode.Or).WhereAllRules();
        List<int> orResult = orBuilder.Apply((IEnumerable<int>)new[] { 1, 2, 3, 4, -2 }).ToList();

        // AND: positive AND even → 2, 4
        CollectionAssert.AreEqual(new[] { 2, 4 }, andResult);

        // OR: positive OR even → 1, 2, 3, 4, -2
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, -2 }, orResult);
    }

    #endregion
}
