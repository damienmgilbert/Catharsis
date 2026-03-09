using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class RuleSetTests
{
    #region Constructor

    [TestMethod]
    public void Ctor_Default_CreatesEmptySet()
    {
        RuleSet<int> set = new();

        Assert.AreEqual(0, set.Count);
    }

    [TestMethod]
    public void Ctor_WithRules_PopulatesSet()
    {
        Rule<int>[] rules =
        [
            new("A", x => x > 0),
            new("B", x => x < 10)
        ];

        RuleSet<int> set = new(rules);

        Assert.AreEqual(2, set.Count);
    }

    [TestMethod]
    public void Ctor_NullRules_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new RuleSet<int>(null!));
    }

    #endregion

    #region IReadOnlyList

    [TestMethod]
    public void Indexer_ReturnsCorrectRule()
    {
        Rule<int> rule = new("A", x => true);
        RuleSet<int> set = new([rule]);

        Assert.AreSame(rule, set[0]);
    }

    [TestMethod]
    public void GetEnumerator_EnumeratesAllRules()
    {
        RuleSet<int> set = new([new("A", x => true), new("B", x => true)]);

        List<Rule<int>> list = set.ToList();

        Assert.AreEqual(2, list.Count);
    }

    #endregion

    #region Add

    [TestMethod]
    public void Add_Rule_IncreasesCount()
    {
        RuleSet<int> set = new();
        set.Add(new Rule<int>("A", x => true));

        Assert.AreEqual(1, set.Count);
    }

    [TestMethod]
    public void Add_NameAndCondition_CreatesAndAddsRule()
    {
        RuleSet<int> set = new();
        set.Add("IsPositive", x => x > 0, priority: 3);

        Assert.AreEqual(1, set.Count);
        Assert.AreEqual("IsPositive", set[0].Name);
        Assert.AreEqual(3, set[0].Priority);
    }

    [TestMethod]
    public void Add_FluentChaining_ReturnsSameInstance()
    {
        RuleSet<int> set = new();
        RuleSet<int> returned = set.Add(new Rule<int>("A", x => true));

        Assert.AreSame(set, returned);
    }

    [TestMethod]
    public void Add_NullRule_ThrowsArgumentNullException()
    {
        RuleSet<int> set = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Add((Rule<int>)null!));
    }

    [TestMethod]
    public void Add_NullName_ThrowsArgumentNullException()
    {
        RuleSet<int> set = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Add(null!, x => true));
    }

    #endregion

    #region Remove

    [TestMethod]
    public void Remove_ExistingRule_DecreasesCount()
    {
        RuleSet<int> set = new();
        set.Add("A", x => true);
        set.Add("B", x => true);

        set.Remove("A");

        Assert.AreEqual(1, set.Count);
        Assert.AreEqual("B", set[0].Name);
    }

    [TestMethod]
    public void Remove_NonExistentName_DoesNothing()
    {
        RuleSet<int> set = new();
        set.Add("A", x => true);

        set.Remove("NotFound");

        Assert.AreEqual(1, set.Count);
    }

    [TestMethod]
    public void Remove_NullName_ThrowsArgumentNullException()
    {
        RuleSet<int> set = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Remove(null!));
    }

    #endregion

    #region Clear

    [TestMethod]
    public void Clear_RemovesAllRules()
    {
        RuleSet<int> set = new();
        set.Add("A", x => true);
        set.Add("B", x => true);

        set.Clear();

        Assert.AreEqual(0, set.Count);
    }

    [TestMethod]
    public void Clear_ReturnsSameInstance()
    {
        RuleSet<int> set = new();
        set.Add("A", x => true);

        Assert.AreSame(set, set.Clear());
    }

    #endregion

    #region Enabled

    [TestMethod]
    public void Enabled_ReturnsOnlyEnabledRules()
    {
        RuleSet<int> set = new();
        set.Add(new Rule<int>("A", x => true) { IsEnabled = true });
        set.Add(new Rule<int>("B", x => true) { IsEnabled = false });
        set.Add(new Rule<int>("C", x => true) { IsEnabled = true });

        List<Rule<int>> enabled = set.Enabled().ToList();

        Assert.AreEqual(2, enabled.Count);
        Assert.IsTrue(enabled.All(r => r.IsEnabled));
    }

    [TestMethod]
    public void Enabled_OrderedByPriorityAscending()
    {
        RuleSet<int> set = new();
        set.Add(new Rule<int>("High", x => true) { Priority = 10 });
        set.Add(new Rule<int>("Low", x => true) { Priority = 1 });
        set.Add(new Rule<int>("Mid", x => true) { Priority = 5 });

        List<Rule<int>> enabled = set.Enabled().ToList();

        Assert.AreEqual("Low", enabled[0].Name);
        Assert.AreEqual("Mid", enabled[1].Name);
        Assert.AreEqual("High", enabled[2].Name);
    }

    #endregion

    #region FindByName

    [TestMethod]
    public void FindByName_ExistingRule_ReturnsRule()
    {
        RuleSet<int> set = new();
        set.Add("Target", x => true);

        Rule<int>? found = set.FindByName("Target");

        Assert.IsNotNull(found);
        Assert.AreEqual("Target", found.Name);
    }

    [TestMethod]
    public void FindByName_NonExistent_ReturnsNull()
    {
        RuleSet<int> set = new();
        set.Add("A", x => true);

        Assert.IsNull(set.FindByName("NotFound"));
    }

    [TestMethod]
    public void FindByName_NullName_ThrowsArgumentNullException()
    {
        RuleSet<int> set = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.FindByName(null!));
    }

    #endregion

    #region WithAllTags / WithAnyTag

    [TestMethod]
    public void WithAllTags_ReturnsRulesMatchingAllTags()
    {
        RuleSet<int> set = new();
        set.Add(new Rule<int>("Both", x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "a", "b" } });
        set.Add(new Rule<int>("OnlyA", x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "a" } });

        List<Rule<int>> result = set.WithAllTags("a", "b").ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Both", result[0].Name);
    }

    [TestMethod]
    public void WithAnyTag_ReturnsRulesMatchingAnyTag()
    {
        RuleSet<int> set = new();
        set.Add(new Rule<int>("TagA", x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "a" } });
        set.Add(new Rule<int>("TagB", x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "b" } });
        set.Add(new Rule<int>("NoTag", x => true));

        List<Rule<int>> result = set.WithAnyTag("a").ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("TagA", result[0].Name);
    }

    [TestMethod]
    public void WithAllTags_NullTags_ThrowsArgumentNullException()
    {
        RuleSet<int> set = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.WithAllTags(null!).ToList());
    }

    [TestMethod]
    public void WithAnyTag_NullTags_ThrowsArgumentNullException()
    {
        RuleSet<int> set = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.WithAnyTag(null!).ToList());
    }

    #endregion

    #region GroupByTag

    [TestMethod]
    public void GroupByTag_GroupsRulesByEachTag()
    {
        RuleSet<int> set = new();
        set.Add(new Rule<int>("R1", x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "a", "b" } });
        set.Add(new Rule<int>("R2", x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "b" } });

        ILookup<string, Rule<int>> grouped = set.GroupByTag();

        Assert.AreEqual(1, grouped["a"].Count());
        Assert.AreEqual(2, grouped["b"].Count());
    }

    #endregion

    #region GroupByPriority

    [TestMethod]
    public void GroupByPriority_GroupsByPriorityValue()
    {
        RuleSet<int> set = new();
        set.Add(new Rule<int>("A", x => true) { Priority = 1 });
        set.Add(new Rule<int>("B", x => true) { Priority = 1 });
        set.Add(new Rule<int>("C", x => true) { Priority = 2 });

        ILookup<int, Rule<int>> grouped = set.GroupByPriority();

        Assert.AreEqual(2, grouped[1].Count());
        Assert.AreEqual(1, grouped[2].Count());
    }

    #endregion

    #region CombineWithAnd / CombineWithOr

    [TestMethod]
    public void CombineWithAnd_AllConditionsMustBeSatisfied()
    {
        RuleSet<int> set = new();
        set.Add("Positive", x => x > 0);
        set.Add("LessThan10", x => x < 10);

        Func<int, bool> combined = set.CombineWithAnd().Compile();

        Assert.IsTrue(combined(5));
        Assert.IsFalse(combined(-1));
        Assert.IsFalse(combined(15));
    }

    [TestMethod]
    public void CombineWithOr_AnyConditionCanBeSatisfied()
    {
        RuleSet<int> set = new();
        set.Add("Positive", x => x > 0);
        set.Add("IsMinusFive", x => x == -5);

        Func<int, bool> combined = set.CombineWithOr().Compile();

        Assert.IsTrue(combined(5));
        Assert.IsTrue(combined(-5));
        Assert.IsFalse(combined(-3));
    }

    [TestMethod]
    public void CombineWithAnd_EmptySet_ReturnsTrue()
    {
        RuleSet<int> set = new();

        Func<int, bool> combined = set.CombineWithAnd().Compile();

        Assert.IsTrue(combined(42));
    }

    [TestMethod]
    public void CombineWithOr_EmptySet_ReturnsFalse()
    {
        RuleSet<int> set = new();

        Func<int, bool> combined = set.CombineWithOr().Compile();

        Assert.IsFalse(combined(42));
    }

    #endregion

    #region Where

    [TestMethod]
    public void Where_FiltersRules_ReturnsNewSet()
    {
        RuleSet<int> set = new();
        set.Add(new Rule<int>("A", x => true) { Priority = 1 });
        set.Add(new Rule<int>("B", x => true) { Priority = 5 });

        RuleSet<int> filtered = set.Where(r => r.Priority > 2);

        Assert.AreEqual(1, filtered.Count);
        Assert.AreEqual("B", filtered[0].Name);
    }

    [TestMethod]
    public void Where_NullFilter_ThrowsArgumentNullException()
    {
        RuleSet<int> set = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Where(null!));
    }

    #endregion

    #region Merge

    [TestMethod]
    public void Merge_CombinesTwoSets()
    {
        RuleSet<int> set1 = new();
        set1.Add("A", x => true);

        RuleSet<int> set2 = new();
        set2.Add("B", x => true);

        RuleSet<int> merged = set1.Merge(set2);

        Assert.AreEqual(2, merged.Count);
    }

    [TestMethod]
    public void Merge_ReturnsNewInstance()
    {
        RuleSet<int> set1 = new();
        RuleSet<int> set2 = new();

        RuleSet<int> merged = set1.Merge(set2);

        Assert.AreNotSame(set1, merged);
        Assert.AreNotSame(set2, merged);
    }

    [TestMethod]
    public void Merge_NullOther_ThrowsArgumentNullException()
    {
        RuleSet<int> set = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Merge(null!));
    }

    #endregion
}
