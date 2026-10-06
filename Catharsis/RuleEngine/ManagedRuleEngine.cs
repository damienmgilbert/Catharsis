using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Catharsis.Configuration;
using Catharsis.Diagnostics;
using Catharsis.Events;
using Catharsis.Linq;

namespace Catharsis.RuleEngine;

///<summary>
///A stateful, observable orchestrator around the stateless <see cref="Linq"/> rule-matching primitives (<see
///cref="Rule{T}"/>, <see cref="RuleSet{T}"/>, <see cref="RuleEvaluator"/>). Where those types are pure and side-effect-
///free, <see cref="ManagedRuleEngine{T}"/> adds the three things a running application actually needs around them: a
///live registry of named rule sets that can be updated without redeploying, execution metrics via a ///<see
///cref="MetricsRecorder"/>, and per-match notifications via an <see cref="EventBus"/>. An optional ///<see
///cref="FeatureFlagEvaluator"/> can gate entire rule sets on or off, enabling gradual rollout of a new rule set by
///name.
///</summary>
///<typeparam name="T">The type of element the engine's rule sets evaluate.</typeparam>
public sealed class ManagedRuleEngine<T>
{
    #region Fields
    private readonly Histogram<double> _evaluationDuration;
    private readonly EventBus _eventBus;
    private readonly FeatureFlagEvaluator? _featureFlags;
    private readonly Counter<long> _matchCounter;
    private readonly ConcurrentDictionary<string, RuleSet<T>> _ruleSets = new(StringComparer.Ordinal);
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a rule engine that publishes match notifications through <paramref name="eventBus"/> and records
    ///evaluation metrics through <paramref name="metrics"/>.
    ///</summary>
    ///<param name="eventBus">The event bus that <see cref="RuleMatchedEvent{T}"/> notifications are published to.</param>
    ///<param name="metrics">The metrics recorder used to create the engine's counter and histogram instruments.</param>
    ///<param name="featureFlags">
    ///An optional evaluator used to gate whole rule sets on or off by name. When <c>null</c>, every registered rule set
    ///always participates in evaluation.
    ///</param>
    ///<exception cref="ArgumentNullException"><paramref name="eventBus"/> or <paramref name="metrics"/> is <c>null</c>.</exception>
    public ManagedRuleEngine(EventBus eventBus, MetricsRecorder metrics, FeatureFlagEvaluator? featureFlags = null)
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(metrics);

        _eventBus = eventBus;
        _featureFlags = featureFlags;
        _matchCounter = metrics.CreateCounter<long>("rule_engine.matches", description: "Number of rule matches recorded by the engine.");
        _evaluationDuration = metrics.CreateHistogram<double>("rule_engine.evaluation_duration", unit: "ms", description: "Duration of a single rule set's evaluation against one element.");
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Evaluates <paramref name="element"/> against every registered rule set that is not gated off by the engine's <see
    ///cref="FeatureFlagEvaluator"/>, publishing a <see cref="RuleMatchedEvent{T}"/> for every individual rule match and
    ///recording per-rule-set duration and match-count metrics.
    ///</summary>
    ///<param name="element">The element to evaluate.</param>
    ///<param name="stickyId">
    ///An optional identifier (e.g. a user or session ID) used to deterministically bucket a rule set's rollout
    ///percentage when a <see cref="FeatureFlagEvaluator"/> was supplied. See ///<see
    ///cref="FeatureFlagEvaluator.IsEnabled"/> for bucketing semantics.
    ///</param>
    ///<param name="cancellationToken">A token that can abandon evaluation between rule sets.</param>
    ///<returns>
    ///A dictionary mapping each participating rule set's name to the <see cref="RuleContext{T}"/> produced by
    ///evaluating it against <paramref name="element"/>. Rule sets skipped due to a feature flag gate are absent.
    ///</returns>
    public async Task<IReadOnlyDictionary<string, RuleContext<T>>> EvaluateAsync(T element, string? stickyId = null, CancellationToken cancellationToken = default)
    {
        Dictionary<string, RuleContext<T>> results = new(StringComparer.Ordinal);
        T[] single = [ element ];

        foreach((string name, RuleSet<T> ruleSet) in _ruleSets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if((_featureFlags is not null) && !_featureFlags.IsEnabled(name, stickyId))
            {
                continue;
            }

            long startTimestamp = Stopwatch.GetTimestamp();
            RuleContext<T> context = single.Evaluate(ruleSet).Single();
            _evaluationDuration.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);

            if(context.HasMatch)
            {
                _matchCounter.Add(context.MatchCount);

                foreach(Rule<T> rule in context.MatchedRules)
                {
                    await _eventBus.PublishAsync(new RuleMatchedEvent<T>(name, rule, element, DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
                }
            }

            results[name] = context;
        }

        return results;
    }

    ///<summary>
    ///Registers a new rule set under the specified name.
    ///</summary>
    ///<param name="name">The unique name to register the rule set under.</param>
    ///<param name="ruleSet">The rule set to register.</param>
    ///<returns>This engine, for fluent chaining.</returns>
    ///<exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="ruleSet"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">A rule set is already registered under <paramref name="name"/>.</exception>
    public ManagedRuleEngine<T> RegisterRuleSet(string name, RuleSet<T> ruleSet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(ruleSet);

        if(!_ruleSets.TryAdd(name, ruleSet))
        {
            throw new InvalidOperationException($"A rule set is already registered under the name '{name}'.");
        }

        return this;
    }

    ///<summary>
    ///Removes the rule set registered under the specified name, if any.
    ///</summary>
    ///<param name="name">The name of the rule set to remove.</param>
    ///<returns><c>true</c> if a rule set was removed; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty, or whitespace.</exception>
    public bool RemoveRuleSet(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _ruleSets.TryRemove(name, out _);
    }

    ///<summary>
    ///Registers a rule set under the specified name, overwriting any rule set already registered under that name.
    ///</summary>
    ///<param name="name">The name to register the rule set under.</param>
    ///<param name="ruleSet">The rule set to register.</param>
    ///<returns>This engine, for fluent chaining.</returns>
    ///<exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="ruleSet"/> is <c>null</c>.</exception>
    public ManagedRuleEngine<T> ReplaceRuleSet(string name, RuleSet<T> ruleSet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(ruleSet);

        _ruleSets[name] = ruleSet;
        return this;
    }

    ///<summary>
    ///Attempts to retrieve the rule set registered under the specified name.
    ///</summary>
    ///<param name="name">The name to look up.</param>
    ///<param name="ruleSet">The registered rule set, if found.</param>
    ///<returns><c>true</c> if a rule set is registered under <paramref name="name"/>; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty, or whitespace.</exception>
    public bool TryGetRuleSet(string name, out RuleSet<T>? ruleSet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _ruleSets.TryGetValue(name, out ruleSet);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///The names of every currently registered rule set.
    ///</summary>
    public IReadOnlyList<string> RuleSetNames => [ .. _ruleSets.Keys ];
    #endregion
}
