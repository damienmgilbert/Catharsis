using Catharsis.Configuration;
using Catharsis.Diagnostics;
using Catharsis.Events;
using Catharsis.Linq;
using Catharsis.RuleEngine;

namespace Catharsis.UnitTests.RuleEngine;

///<summary>
///Unit tests for the <see cref="ManagedRuleEngine{T}"/> class.
///</summary>
[TestClass]
public class ManagedRuleEngineTests
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

    #region Constructor

    [TestMethod]
    public void Constructor_NullEventBus_Throws()
    {
        using MetricsRecorder metrics = new("test.rule-engine.ctor-null-bus");
        Assert.ThrowsExactly<ArgumentNullException>(() => new ManagedRuleEngine<int>(null!, metrics));
    }

    [TestMethod]
    public void Constructor_NullMetrics_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new ManagedRuleEngine<int>(new EventBus(), null!));
    }

    #endregion

    #region RegisterRuleSet

    [TestMethod]
    public void RegisterRuleSet_NewName_AddsToRuleSetNames()
    {
        using MetricsRecorder metrics = new("test.rule-engine.register");
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics);

        engine.RegisterRuleSet("standard", CreateStandardRuleSet());

        CollectionAssert.Contains(engine.RuleSetNames.ToList(), "standard");
    }

    [TestMethod]
    public void RegisterRuleSet_DuplicateName_Throws()
    {
        using MetricsRecorder metrics = new("test.rule-engine.register-dup");
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics);
        engine.RegisterRuleSet("standard", CreateStandardRuleSet());

        Assert.ThrowsExactly<InvalidOperationException>(() => engine.RegisterRuleSet("standard", CreateStandardRuleSet()));
    }

    [TestMethod]
    public void RegisterRuleSet_NullOrWhitespaceName_Throws()
    {
        using MetricsRecorder metrics = new("test.rule-engine.register-blank");
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics);

        Assert.ThrowsExactly<ArgumentException>(() => engine.RegisterRuleSet("  ", CreateStandardRuleSet()));
    }

    [TestMethod]
    public void RegisterRuleSet_NullRuleSet_Throws()
    {
        using MetricsRecorder metrics = new("test.rule-engine.register-null-set");
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics);

        Assert.ThrowsExactly<ArgumentNullException>(() => engine.RegisterRuleSet("standard", null!));
    }

    #endregion

    #region ReplaceRuleSet / RemoveRuleSet / TryGetRuleSet

    [TestMethod]
    public void ReplaceRuleSet_ExistingName_OverwritesRuleSet()
    {
        using MetricsRecorder metrics = new("test.rule-engine.replace");
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics);
        engine.RegisterRuleSet("standard", CreateStandardRuleSet());

        RuleSet<int> replacement = [];
        replacement.Add("IsNegative", static x => x < 0);
        engine.ReplaceRuleSet("standard", replacement);

        Assert.IsTrue(engine.TryGetRuleSet("standard", out RuleSet<int>? ruleSet));
        Assert.IsNotNull(ruleSet!.FindByName("IsNegative"));
    }

    [TestMethod]
    public void RemoveRuleSet_ExistingName_ReturnsTrueAndRemoves()
    {
        using MetricsRecorder metrics = new("test.rule-engine.remove");
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics);
        engine.RegisterRuleSet("standard", CreateStandardRuleSet());

        bool removed = engine.RemoveRuleSet("standard");

        Assert.IsTrue(removed);
        Assert.IsFalse(engine.TryGetRuleSet("standard", out _));
    }

    [TestMethod]
    public void RemoveRuleSet_UnknownName_ReturnsFalse()
    {
        using MetricsRecorder metrics = new("test.rule-engine.remove-unknown");
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics);

        Assert.IsFalse(engine.RemoveRuleSet("missing"));
    }

    [TestMethod]
    public void TryGetRuleSet_UnknownName_ReturnsFalse()
    {
        using MetricsRecorder metrics = new("test.rule-engine.get-unknown");
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics);

        Assert.IsFalse(engine.TryGetRuleSet("missing", out RuleSet<int>? ruleSet));
        Assert.IsNull(ruleSet);
    }

    #endregion

    #region EvaluateAsync

    [TestMethod]
    public async Task EvaluateAsync_NoRuleSetsRegistered_ReturnsEmptyResult()
    {
        using MetricsRecorder metrics = new("test.rule-engine.evaluate-empty");
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics);

        IReadOnlyDictionary<string, RuleContext<int>> results = await engine.EvaluateAsync(5);

        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public async Task EvaluateAsync_MatchingElement_ResultContainsMatchedContext()
    {
        using MetricsRecorder metrics = new("test.rule-engine.evaluate-match");
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics);
        engine.RegisterRuleSet("standard", CreateStandardRuleSet());

        IReadOnlyDictionary<string, RuleContext<int>> results = await engine.EvaluateAsync(4);

        Assert.IsTrue(results["standard"].HasMatch);
        Assert.AreEqual(2, results["standard"].MatchCount);
    }

    [TestMethod]
    public async Task EvaluateAsync_MatchingElement_PublishesRuleMatchedEventPerMatch()
    {
        using MetricsRecorder metrics = new("test.rule-engine.evaluate-publish");
        EventBus eventBus = new();
        ManagedRuleEngine<int> engine = new(eventBus, metrics);
        engine.RegisterRuleSet("standard", CreateStandardRuleSet());

        List<RuleMatchedEvent<int>> published = [];
        using IDisposable subscription = eventBus.Subscribe<RuleMatchedEvent<int>>(evt =>
        {
            published.Add(evt);
            return Task.CompletedTask;
        });

        await engine.EvaluateAsync(4);

        Assert.HasCount(2, published);
        CollectionAssert.AreEquivalent(new[] { "IsPositive", "IsEven" }, published.Select(static e => e.Rule.Name).ToList());
        Assert.IsTrue(published.All(static e => e.RuleSetName == "standard"));
        Assert.IsTrue(published.All(static e => e.Element == 4));
    }

    [TestMethod]
    public async Task EvaluateAsync_NonMatchingElement_PublishesNoEvents()
    {
        using MetricsRecorder metrics = new("test.rule-engine.evaluate-no-match");
        EventBus eventBus = new();
        ManagedRuleEngine<int> engine = new(eventBus, metrics);
        engine.RegisterRuleSet("standard", CreateStandardRuleSet());

        int publishCount = 0;
        using IDisposable subscription = eventBus.Subscribe<RuleMatchedEvent<int>>(_ =>
        {
            publishCount++;
            return Task.CompletedTask;
        });

        await engine.EvaluateAsync(-3);

        Assert.AreEqual(0, publishCount);
    }

    [TestMethod]
    public async Task EvaluateAsync_FeatureFlagDisabled_SkipsRuleSet()
    {
        using MetricsRecorder metrics = new("test.rule-engine.evaluate-flag-disabled");
        FeatureFlagEvaluator flags = new([new FeatureFlag("standard", Enabled: false)]);
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics, flags);
        engine.RegisterRuleSet("standard", CreateStandardRuleSet());

        IReadOnlyDictionary<string, RuleContext<int>> results = await engine.EvaluateAsync(4);

        Assert.IsFalse(results.ContainsKey("standard"));
    }

    [TestMethod]
    public async Task EvaluateAsync_FeatureFlagEnabled_IncludesRuleSet()
    {
        using MetricsRecorder metrics = new("test.rule-engine.evaluate-flag-enabled");
        FeatureFlagEvaluator flags = new([new FeatureFlag("standard", Enabled: true)]);
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics, flags);
        engine.RegisterRuleSet("standard", CreateStandardRuleSet());

        IReadOnlyDictionary<string, RuleContext<int>> results = await engine.EvaluateAsync(4);

        Assert.IsTrue(results.ContainsKey("standard"));
    }

    [TestMethod]
    public async Task EvaluateAsync_CanceledToken_ThrowsOperationCanceled()
    {
        using MetricsRecorder metrics = new("test.rule-engine.evaluate-canceled");
        ManagedRuleEngine<int> engine = new(new EventBus(), metrics);
        engine.RegisterRuleSet("standard", CreateStandardRuleSet());

        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => engine.EvaluateAsync(4, cancellationToken: cts.Token));
    }

    #endregion
}
