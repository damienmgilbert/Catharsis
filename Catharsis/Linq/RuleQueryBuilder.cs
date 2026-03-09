using Catharsis.Linq.Expressions;
using System.Linq.Expressions;

namespace Catharsis.Linq;

///<summary>
///A fluent builder that constructs LINQ query pipelines from rules. Supports filtering by individual rules, combining
///rule conditions, projecting matched elements, grouping by rule, and paginating results. Works with both <see
///cref="IEnumerable{T}"/> and <see cref="IQueryable{T}"/> sources.
///</summary>
///<typeparam name="T">The element type.</typeparam>
public sealed class RuleQueryBuilder<T>
{
    #region Fields
    private FilterCombineMode _combineMode = FilterCombineMode.And;
    private readonly List<Expression<Func<T, bool>>> _filters = [];
    private readonly List<(string Name, Expression<Func<T, bool>> Condition)> _namedFilters = [];
    private readonly RuleSet<T> _ruleSet;
    private int? _skip;
    private int? _take;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a query builder backed by the specified rule set.
    ///</summary>
    ///<param name="ruleSet">The rule set to build queries from.</param>
    ///<exception cref="ArgumentNullException"><paramref name="ruleSet"/> is <c>null</c>.</exception>
    public RuleQueryBuilder(RuleSet<T> ruleSet)
    {
        ArgumentNullException.ThrowIfNull(ruleSet, nameof(ruleSet));
        _ruleSet = ruleSet;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Applies the built pipeline to a queryable source. The combined predicate is provider-translatable.
    ///</summary>
    ///<param name="source">The queryable source.</param>
    ///<returns>The filtered (and optionally paginated) queryable.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public IQueryable<T> Apply(IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        IQueryable<T> query = source.Where(BuildPredicate());

        if (_skip.HasValue)
        {
            query = query.Skip(_skip.Value);
        }

        if (_take.HasValue)
        {
            query = query.Take(_take.Value);
        }

        return query;
    }

    ///<summary>
    ///Applies the built pipeline to an in-memory sequence.
    ///</summary>
    ///<param name="source">The source sequence.</param>
    ///<returns>The filtered (and optionally paginated) sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public IEnumerable<T> Apply(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        Func<T, bool> compiled = BuildPredicate().Compile();
        IEnumerable<T> result = source.Where(compiled);

        if (_skip.HasValue)
        {
            result = result.Skip(_skip.Value);
        }

        if (_take.HasValue)
        {
            result = result.Take(_take.Value);
        }

        return result;
    }

    ///<summary>
    ///Applies the built pipeline and groups matching elements by the first matching rule name.
    ///</summary>
    ///<param name="source">The source sequence.</param>
    ///<returns>A lookup mapping rule names to matching elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public ILookup<string, T> ApplyAndGroupByRule(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        IEnumerable<T> filtered = Apply(source);

        List<(string Name, Func<T, bool> Compiled)> compiled = [.. _namedFilters.Select(nf => (nf.Name, Compiled: nf.Condition.Compile()))];

        return filtered
            .Select(
               element =>
               {
                   string matchedRule = compiled.FirstOrDefault(nf => nf.Compiled(element)).Name ?? "__unmatched__";
                   return (Rule: matchedRule, Element: element);
               })
            .ToLookup(x => x.Rule, x => x.Element);
    }

    ///<summary>
    ///Applies the built pipeline to a queryable source with a projection.
    ///</summary>
    ///<typeparam name="TResult">The projected type.</typeparam>
    ///<param name="source">The queryable source.</param>
    ///<param name="selector">The projection expression.</param>
    ///<returns>The filtered, optionally paginated, and projected queryable.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public IQueryable<TResult> ApplyAndProject<TResult>(IQueryable<T> source, Expression<Func<T, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return Apply(source).Select(selector);
    }

    ///<summary>
    ///Applies the built pipeline to an in-memory sequence with a projection.
    ///</summary>
    ///<typeparam name="TResult">The projected type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="selector">The projection function.</param>
    ///<returns>The filtered, optionally paginated, and projected sequence.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.</exception>
    public IEnumerable<TResult> ApplyAndProject<TResult>(IEnumerable<T> source, Func<T, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return Apply(source).Select(selector);
    }

    ///<summary>
    ///Builds the accumulated filters into a single expression-tree predicate.
    ///</summary>
    ///<returns>A combined predicate.</returns>
    public Expression<Func<T, bool>> BuildPredicate()
    {
        return _combineMode switch
        {
            FilterCombineMode.And => ExpressionComposer.AndAll<T>(_filters),
            FilterCombineMode.Or => ExpressionComposer.OrAny<T>(_filters),
            _ => ExpressionComposer.AndAll<T>(_filters)
        };
    }

    ///<summary>
    ///Removes all accumulated filters and resets pagination.
    ///</summary>
    ///<returns>The current builder for fluent chaining.</returns>
    public RuleQueryBuilder<T> Clear()
    {
        _filters.Clear();
        _namedFilters.Clear();
        _skip = null;
        _take = null;
        return this;
    }

    ///<summary>
    ///Skips the first <paramref name="count"/> elements from the result.
    ///</summary>
    ///<param name="count">The number of elements to skip.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public RuleQueryBuilder<T> Skip(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        _skip = count;
        return this;
    }

    ///<summary>
    ///Takes only the first <paramref name="count"/> elements from the result.
    ///</summary>
    ///<param name="count">The maximum number of elements to take.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public RuleQueryBuilder<T> Take(int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1, nameof(count));
        _take = count;
        return this;
    }

    ///<summary>
    ///Adds a custom predicate expression as a filter (not tied to a named rule).
    ///</summary>
    ///<param name="predicate">The predicate to add.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public RuleQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        _filters.Add(predicate);
        return this;
    }

    ///<summary>
    ///Adds all enabled rules' conditions as filters.
    ///</summary>
    ///<returns>The current builder for fluent chaining.</returns>
    public RuleQueryBuilder<T> WhereAllRules()
    {
        foreach (Rule<T> rule in _ruleSet.Enabled())
        {
            _filters.Add(rule.Condition);
            _namedFilters.Add((rule.Name, rule.Condition));
        }

        return this;
    }

    ///<summary>
    ///Adds a custom predicate only when <paramref name="condition"/> is <c>true</c>.
    ///</summary>
    ///<param name="condition">Whether to add the predicate.</param>
    ///<param name="predicate">The predicate to add.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public RuleQueryBuilder<T> WhereIf(bool condition, Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        if (condition)
        {
            _filters.Add(predicate);
        }

        return this;
    }

    ///<summary>
    ///Adds a single named rule's condition as a filter.
    ///</summary>
    ///<param name="ruleName">The name of the rule.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="ruleName"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">No rule with <paramref name="ruleName"/> exists.</exception>
    public RuleQueryBuilder<T> WhereRule(string ruleName)
    {
        ArgumentNullException.ThrowIfNull(ruleName, nameof(ruleName));

        Rule<T> rule = _ruleSet.FindByName(ruleName) ?? throw new InvalidOperationException($"Rule '{ruleName}' not found in the rule set.");

        _filters.Add(rule.Condition);
        _namedFilters.Add((rule.Name, rule.Condition));
        return this;
    }

    ///<summary>
    ///Adds conditions from rules matching the specified tags.
    ///</summary>
    ///<param name="tags">The tags to filter rules by.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="tags"/> is <c>null</c>.</exception>
    public RuleQueryBuilder<T> WhereRulesWithTags(params string[] tags)
    {
        ArgumentNullException.ThrowIfNull(tags, nameof(tags));

        foreach (Rule<T> rule in _ruleSet.WithAnyTag(tags).Where(r => r.IsEnabled))
        {
            _filters.Add(rule.Condition);
            _namedFilters.Add((rule.Name, rule.Condition));
        }

        return this;
    }

    ///<summary>
    ///Sets how accumulated conditions are combined. Defaults to <see cref="FilterCombineMode.And"/>.
    ///</summary>
    ///<param name="mode">The combination mode.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    public RuleQueryBuilder<T> WithCombineMode(FilterCombineMode mode)
    {
        _combineMode = mode;
        return this;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Returns the names of rules currently contributing conditions.
    ///</summary>
    public IReadOnlyList<string> ActiveRuleNames => _namedFilters.Select(nf => nf.Name).ToList().AsReadOnly();

    ///<summary>
    ///Returns the number of filters currently in the builder.
    ///</summary>
    public int FilterCount => _filters.Count;
    #endregion
}
