using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="RuleSet"/> class.
///</summary>
[TestClass]
public class RuleSetTests
{
    #region Constructor

    [TestMethod]
    public void Ctor_Default_CreatesEmptySet()
    {
        RuleSet<int> set = [];

        Assert.AreEqual(0, set.Count);
    }

    [TestMethod]
    public void Ctor_WithRules_PopulatesSet()
    {
        Rule<int>[] rules =
        [
            new("A", static x => x > 0),
            new("B", static x => x < 10)
        ];

        RuleSet<int> set = new(rules);

        Assert.AreEqual(2, set.Count);
    }

    [TestMethod]
    public void Ctor_NullRules_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => new RuleSet<int>(null!));
    }

    #endregion

    #region IReadOnlyList

    [TestMethod]
    public void Indexer_ReturnsCorrectRule()
    {
        Rule<int> rule = new("A", static x => true);
        RuleSet<int> set = new([rule]);

        Assert.AreSame(rule, set[0]);
    }

    [TestMethod]
    public void GetEnumerator_EnumeratesAllRules()
    {
        RuleSet<int> set = new([new("A", static x => true), new("B", static x => true)]);

        List<Rule<int>> list = set.ToList();

        Assert.HasCount(2, list);
    }

    #endregion

    #region Add

    [TestMethod]
    public void Add_Rule_IncreasesCount()
    {
        RuleSet<int> set = [new Rule<int>("A", static x => true)];

        Assert.AreEqual(1, set.Count);
    }

    [TestMethod]
    public void Add_NameAndCondition_CreatesAndAddsRule()
    {
        RuleSet<int> set = [];
        set.Add("IsPositive", static x => x > 0, priority: 3);

        Assert.AreEqual(1, set.Count);
        Assert.AreEqual("IsPositive", set[0].Name);
        Assert.AreEqual(3, set[0].Priority);
    }

    [TestMethod]
    public void Add_FluentChaining_ReturnsSameInstance()
    {
        RuleSet<int> set = [];
        RuleSet<int> returned = set.Add(new Rule<int>("A", static x => true));

        Assert.AreSame(set, returned);
    }

    [TestMethod]
    public void Add_NullRule_ThrowsArgumentNullException()
    {
        RuleSet<int> set = [];

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Add((Rule<int>)null!));
    }

    [TestMethod]
    public void Add_NullName_ThrowsArgumentNullException()
    {
        RuleSet<int> set = [];

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Add(null!, x => true));
    }

    #endregion

    #region Remove

    [TestMethod]
    public void Remove_ExistingRule_DecreasesCount()
    {
        RuleSet<int> set = new()
        {
            { "A", static x => true },
            { "B", static x => true }
        };

        set.Remove("A");

        Assert.AreEqual(1, set.Count);
        Assert.AreEqual("B", set[0].Name);
    }

    [TestMethod]
    public void Remove_NonExistentName_DoesNothing()
    {
        RuleSet<int> set = new()
        {
            { "A", static x => true }
        };

        set.Remove("NotFound");

        Assert.AreEqual(1, set.Count);
    }

    [TestMethod]
    public void Remove_NullName_ThrowsArgumentNullException()
    {
        RuleSet<int> set = [];

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Remove(null!));
    }

    #endregion

    #region Clear

    [TestMethod]
    public void Clear_RemovesAllRules()
    {
        RuleSet<int> set = new()
        {
            { "A", static x => true },
            { "B", static x => true }
        };

        set.Clear();

        Assert.AreEqual(0, set.Count);
    }

    [TestMethod]
    public void Clear_ReturnsSameInstance()
    {
        RuleSet<int> set = new()
        {
            { "A", static x => true }
        };

        Assert.AreSame(set, set.Clear());
    }

    #endregion

    #region Enabled

    [TestMethod]
    public void Enabled_ReturnsOnlyEnabledRules()
    {
        RuleSet<int> set =
        [
            new Rule<int>("A", static x => true) { IsEnabled = true },
            new Rule<int>("B", static x => true) { IsEnabled = false },
            new Rule<int>("C", static x => true) { IsEnabled = true },
        ];

        List<Rule<int>> enabled = set.Enabled().ToList();

        Assert.HasCount(2, enabled);
        Assert.IsTrue(enabled.All(static r => r.IsEnabled));
    }

    [TestMethod]
    public void Enabled_OrderedByPriorityAscending()
    {
        RuleSet<int> set =
        [
            new Rule<int>("High", static x => true) { Priority = 10 },
            new Rule<int>("Low", static x => true) { Priority = 1 },
            new Rule<int>("Mid", static x => true) { Priority = 5 },
        ];

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
        RuleSet<int> set = new()
        {
            { "Target", static x => true }
        };

        Rule<int>? found = set.FindByName("Target");

        Assert.IsNotNull(found);
        Assert.AreEqual("Target", found.Name);
    }

    [TestMethod]
    public void FindByName_NonExistent_ReturnsNull()
    {
        RuleSet<int> set = new()
        {
            { "A", static x => true }
        };

        Assert.IsNull(set.FindByName("NotFound"));
    }

    [TestMethod]
    public void FindByName_NullName_ThrowsArgumentNullException()
    {
        RuleSet<int> set = [];

        Assert.ThrowsExactly<ArgumentNullException>(() => set.FindByName(null!));
    }

    #endregion

    #region WithAllTags / WithAnyTag

    [TestMethod]
    public void WithAllTags_ReturnsRulesMatchingAllTags()
    {
        RuleSet<int> set =
        [
            new Rule<int>("Both", static x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "a", "b" } },
            new Rule<int>("OnlyA", static x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "a" } },
        ];

        List<Rule<int>> result = set.WithAllTags("a", "b").ToList();

        Assert.HasCount(1, result);
        Assert.AreEqual("Both", result[0].Name);
    }

    [TestMethod]
    public void WithAnyTag_ReturnsRulesMatchingAnyTag()
    {
        RuleSet<int> set =
        [
            new Rule<int>("TagA", static x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "a" } },
            new Rule<int>("TagB", static x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "b" } },
            new Rule<int>("NoTag", static x => true),
        ];

        List<Rule<int>> result = set.WithAnyTag("a").ToList();

        Assert.HasCount(1, result);
        Assert.AreEqual("TagA", result[0].Name);
    }

    [TestMethod]
    public void WithAllTags_NullTags_ThrowsArgumentNullException()
    {
        RuleSet<int> set = [];

        Assert.ThrowsExactly<ArgumentNullException>(() => set.WithAllTags(null!).ToList());
    }

    [TestMethod]
    public void WithAnyTag_NullTags_ThrowsArgumentNullException()
    {
        RuleSet<int> set = [];

        Assert.ThrowsExactly<ArgumentNullException>(() => set.WithAnyTag(null!).ToList());
    }

    #endregion

    #region GroupByTag

    [TestMethod]
    public void GroupByTag_GroupsRulesByEachTag()
    {
        RuleSet<int> set =
        [
            new Rule<int>("R1", static x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "a", "b" } },
            new Rule<int>("R2", static x => true) { Tags = new HashSet<string>(StringComparer.Ordinal) { "b" } },
        ];

        ILookup<string, Rule<int>> grouped = set.GroupByTag();

        Assert.AreEqual(1, grouped["a"].Count());
        Assert.AreEqual(2, grouped["b"].Count());
    }

    #endregion

    #region GroupByPriority

    [TestMethod]
    public void GroupByPriority_GroupsByPriorityValue()
    {
        RuleSet<int> set =
        [
            new Rule<int>("A", static x => true) { Priority = 1 },
            new Rule<int>("B", static x => true) { Priority = 1 },
            new Rule<int>("C", static x => true) { Priority = 2 },
        ];

        ILookup<int, Rule<int>> grouped = set.GroupByPriority();

        Assert.AreEqual(2, grouped[1].Count());
        Assert.AreEqual(1, grouped[2].Count());
    }

    #endregion

    #region CombineWithAnd / CombineWithOr

    [TestMethod]
    public void CombineWithAnd_AllConditionsMustBeSatisfied()
    {
        RuleSet<int> set = new()
        {
            { "Positive", static x => x > 0 },
            { "LessThan10", static x => x < 10 }
        };

        Func<int, bool> combined = set.CombineWithAnd().Compile();

        Assert.IsTrue(combined(5));
        Assert.IsFalse(combined(-1));
        Assert.IsFalse(combined(15));
    }

    [TestMethod]
    public void CombineWithOr_AnyConditionCanBeSatisfied()
    {
        RuleSet<int> set = new()
        {
            { "Positive", static x => x > 0 },
            { "IsMinusFive", static x => x == -5 }
        };

        Func<int, bool> combined = set.CombineWithOr().Compile();

        Assert.IsTrue(combined(5));
        Assert.IsTrue(combined(-5));
        Assert.IsFalse(combined(-3));
    }

    [TestMethod]
    public void CombineWithAnd_EmptySet_ReturnsTrue()
    {
        RuleSet<int> set = [];

        Func<int, bool> combined = set.CombineWithAnd().Compile();

        Assert.IsTrue(combined(42));
    }

    [TestMethod]
    public void CombineWithOr_EmptySet_ReturnsFalse()
    {
        RuleSet<int> set = [];

        Func<int, bool> combined = set.CombineWithOr().Compile();

        Assert.IsFalse(combined(42));
    }

    #endregion

    #region Where

    [TestMethod]
    public void Where_FiltersRules_ReturnsNewSet()
    {
        RuleSet<int> set =
        [
            new Rule<int>("A", static x => true) { Priority = 1 },
            new Rule<int>("B", static x => true) { Priority = 5 },
        ];

        RuleSet<int> filtered = set.Where(static r => r.Priority > 2);

        Assert.AreEqual(1, filtered.Count);
        Assert.AreEqual("B", filtered[0].Name);
    }

    [TestMethod]
    public void Where_NullFilter_ThrowsArgumentNullException()
    {
        RuleSet<int> set = [];

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Where(null!));
    }

    #endregion

    #region Merge

    [TestMethod]
    public void Merge_CombinesTwoSets()
    {
        RuleSet<int> set1 = new()
        {
            { "A", static x => true }
        };

        RuleSet<int> set2 = new()
        {
            { "B", static x => true }
        };

        RuleSet<int> merged = set1.Merge(set2);

        Assert.AreEqual(2, merged.Count);
    }

    [TestMethod]
    public void Merge_ReturnsNewInstance()
    {
        RuleSet<int> set1 = [];
        RuleSet<int> set2 = [];

        RuleSet<int> merged = set1.Merge(set2);

        Assert.AreNotSame(set1, merged);
        Assert.AreNotSame(set2, merged);
    }

    [TestMethod]
    public void Merge_NullOther_ThrowsArgumentNullException()
    {
        RuleSet<int> set = [];

        Assert.ThrowsExactly<ArgumentNullException>(() => set.Merge(null!));
    }

    #endregion
}
