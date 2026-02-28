using Catharsis.Linq.Expressions;
using System.Collections;
using System.Linq.Expressions;

namespace Catharsis.Linq;

///<summary>
///A composable, immutable-by-convention collection of <see cref="Rule{T}"/> instances. Supports LINQ-style filtering,
///grouping by tag, ordering by priority, and composition into combined predicates for both ///<see
///cref="IEnumerable{T}"/> and <see cref="IQueryable{T}"/> evaluation.
///</summary>
///<typeparam name="T">The element type the rules apply to.</typeparam>
public sealed class RuleSet<T> : IReadOnlyList<Rule<T>>
{
    #region Fields
    private readonly List<Rule<T>> _rules;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates an empty rule set.
    ///</summary>
    public RuleSet() { _rules = []; }

    ///<summary>
    ///Creates a rule set from an existing collection of rules.
    ///</summary>
    ///<param name="rules">The rules to include.</param>
    ///<exception cref="ArgumentNullException"><paramref name="rules"/> is <c>null</c>.</exception>
    public RuleSet(IEnumerable<Rule<T>> rules)
    {
        ArgumentNullException.ThrowIfNull(rules, nameof(rules));
        _rules = rules.ToList();
    }
    #endregion

    #region Indexers
    ///<inheritdoc/>
    public Rule<T> this[int index] => _rules[index];
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a rule to the set.
    ///</summary>
    ///<param name="rule">The rule to add.</param>
    ///<returns>The current set for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="rule"/> is <c>null</c>.</exception>
    public RuleSet<T> Add(Rule<T> rule)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));
        _rules.Add(rule);
        return this;
    }

    ///<summary>
    ///Adds a rule built from a name and condition expression.
    ///</summary>
    ///<param name="name">The rule name.</param>
    ///<param name="condition">The condition expression.</param>
    ///<param name="priority">Optional priority (lower = earlier). Defaults to 0.</param>
    ///<returns>The current set for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="condition"/> is <c>null</c>.</exception>
    public RuleSet<T> Add(string name, Expression<Func<T, bool>> condition, int priority = 0)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        ArgumentNullException.ThrowIfNull(condition, nameof(condition));
        _rules.Add(new Rule<T>(name, condition) { Priority = priority });
        return this;
    }

    ///<summary>
    ///Removes all rules.
    ///</summary>
    ///<returns>The current set for fluent chaining.</returns>
    public RuleSet<T> Clear()
    {
        _rules.Clear();
        return this;
    }

    ///<summary>
    ///Combines all enabled rule conditions with logical AND into a single expression-tree predicate. Returns a constant
    ///<c>true</c> predicate if no enabled rules exist.
    ///</summary>
    ///<returns>A combined AND predicate expression.</returns>
    public Expression<Func<T, bool>> CombineWithAnd() { return ExpressionComposer.AndAll(Enabled().Select(r => r.Condition)); }
    ///<summary>
    ///Combines all enabled rule conditions with logical OR into a single expression-tree predicate. Returns a constant
    ///<c>false</c> predicate if no enabled rules exist.
    ///</summary>
    ///<returns>A combined OR predicate expression.</returns>
    public Expression<Func<T, bool>> CombineWithOr() { return ExpressionComposer.OrAny(Enabled().Select(r => r.Condition)); }
    ///<summary>
    ///Returns only enabled rules, ordered by priority (ascending).
    ///</summary>
    ///<returns>An ordered sequence of enabled rules.</returns>
    public IEnumerable<Rule<T>> Enabled() { return _rules.Where(r => r.IsEnabled).OrderBy(r => r.Priority); }

    ///<summary>
    ///Finds a rule by name.
    ///</summary>
    ///<param name="name">The rule name.</param>
    ///<returns>The rule, or <c>null</c> if not found.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    public Rule<T>? FindByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        return _rules.Find(r => string.Equals(r.Name, name, StringComparison.Ordinal));
    }

    ///<inheritdoc/>
    public IEnumerator<Rule<T>> GetEnumerator() { return _rules.GetEnumerator(); }
    ///<summary>
    ///Groups enabled rules by priority.
    ///</summary>
    ///<returns>A lookup mapping priority values to rules.</returns>
    public ILookup<int, Rule<T>> GroupByPriority() { return _rules.Where(r => r.IsEnabled).ToLookup(r => r.Priority); }
    ///<summary>
    ///Groups enabled rules by tag. Each rule appears in every tag group it belongs to.
    ///</summary>
    ///<returns>A lookup mapping tags to rules.</returns>
    public ILookup<string, Rule<T>> GroupByTag() { return _rules.Where(r => r.IsEnabled).SelectMany(r => r.Tags.Select(t => (Tag: t, Rule: r))).ToLookup(x => x.Tag, x => x.Rule, StringComparer.Ordinal); }

    ///<summary>
    ///Merges another rule set into this one, returning a new combined set.
    ///</summary>
    ///<param name="other">The rule set to merge.</param>
    ///<returns>A new rule set containing rules from both sets.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public RuleSet<T> Merge(RuleSet<T> other)
    {
        ArgumentNullException.ThrowIfNull(other, nameof(other));
        return new RuleSet<T>(_rules.Concat(other._rules));
    }

    ///<summary>
    ///Removes a rule by name.
    ///</summary>
    ///<param name="name">The name of the rule to remove.</param>
    ///<returns>The current set for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    public RuleSet<T> Remove(string name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        _rules.RemoveAll(r => string.Equals(r.Name, name, StringComparison.Ordinal));
        return this;
    }

    ///<summary>
    ///Creates a new rule set containing only rules from this set whose conditions match ///<paramref
    ///name="ruleFilter"/>.
    ///</summary>
    ///<param name="ruleFilter">A predicate to apply to each rule.</param>
    ///<returns>A filtered rule set.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="ruleFilter"/> is <c>null</c>.</exception>
    public RuleSet<T> Where(Func<Rule<T>, bool> ruleFilter)
    {
        ArgumentNullException.ThrowIfNull(ruleFilter, nameof(ruleFilter));
        return new RuleSet<T>(_rules.Where(ruleFilter));
    }

    ///<summary>
    ///Returns rules that have all of the specified tags.
    ///</summary>
    ///<param name="tags">The tags to require.</param>
    ///<returns>Rules matching all tags.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="tags"/> is <c>null</c>.</exception>
    public IEnumerable<Rule<T>> WithAllTags(params string[] tags)
    {
        ArgumentNullException.ThrowIfNull(tags, nameof(tags));
        return _rules.Where(r => tags.All(t => r.Tags.Contains(t)));
    }

    ///<summary>
    ///Returns rules that have at least one of the specified tags.
    ///</summary>
    ///<param name="tags">The tags to search for.</param>
    ///<returns>Rules matching any tag.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="tags"/> is <c>null</c>.</exception>
    public IEnumerable<Rule<T>> WithAnyTag(params string[] tags)
    {
        ArgumentNullException.ThrowIfNull(tags, nameof(tags));
        return _rules.Where(r => tags.Any(t => r.Tags.Contains(t)));
    }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public int Count => _rules.Count;
    #endregion
}
