using System.Linq.Expressions;
using Catharsis.Linq.Expressions;

namespace Catharsis.Linq;

///<summary>
///Provides static methods for evaluating a <see cref="RuleSet{T}"/> against <see cref="IEnumerable{T}"/> and ///<see
///cref="IQueryable{T}"/> sources. In-memory evaluation runs compiled delegates, fires ///<see cref="Rule{T}.OnMatch"/>
///actions, and produces <see cref="RuleContext{T}"/> results. Queryable evaluation composes rule conditions into
///expression trees for provider-side translation.
///</summary>
public static class RuleEvaluator
{
    #region Private methods
    private static IEnumerable<RuleContext<T>> EvaluateIterator<T>(IEnumerable<T> source, RuleSet<T> ruleSet)
    {
        List<Rule<T>> enabledRules = [ .. ruleSet.Enabled() ];

        foreach(T element in source)
        {
            RuleContext<T> context = new(element);

            foreach(Rule<T> rule in enabledRules)
            {
                if(rule.Evaluate(element))
                {
                    context.RecordMatch(rule);
                    rule.OnMatch?.Invoke(element);

                    if(context.WasStopped)
                    {
                        break;
                    }
                }
            }

            yield return context;
        }
    }
    #endregion

    extension<T>(IEnumerable<T> source)
    {
                ///<summary>
///Evaluates all enabled rules in <paramref name="ruleSet"/> against each element. Returns a ///<see
///cref="RuleContext{T}"/> for every element regardless of whether any rule matched.
///</summary>
        ///<typeparam name="T">The element type.</typeparam>
        ///<param name="source">The source sequence.</param>
        ///<param name="ruleSet">The rule set to evaluate.</param>
        ///<returns>A sequence of rule contexts, one per element.</returns>
        ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="ruleSet"/> is <c>null</c>.</exception>
        public IEnumerable<RuleContext<T>> Evaluate(RuleSet<T> ruleSet)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(ruleSet, nameof(ruleSet));
            return EvaluateIterator(source, ruleSet);
        }

        ///<summary>
        ///Groups elements by the first rule they match. Elements that match no rule are grouped under <c>null</c>.
        ///</summary>
        ///<typeparam name="T">The element type.</typeparam>
        ///<param name="source">The source sequence.</param>
        ///<param name="ruleSet">The rule set to evaluate.</param>
        ///<returns>A lookup mapping rule names (or <c>null</c>) to matching elements.</returns>
        ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="ruleSet"/> is <c>null</c>.</exception>
        public ILookup<string?, T> GroupByFirstMatch(RuleSet<T> ruleSet)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(ruleSet, nameof(ruleSet));
            return EvaluateIterator(source, ruleSet).ToLookup(static ctx => ctx.FirstMatch?.Name, static ctx => ctx.Element);
        }

        ///<summary>
        ///Projects each element into a tuple of the element and all rules it matched.
        ///</summary>
        ///<typeparam name="T">The element type.</typeparam>
        ///<param name="source">The source sequence.</param>
        ///<param name="ruleSet">The rule set to evaluate.</param>
        ///<returns>A sequence of (element, matched rules) tuples.</returns>
        ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="ruleSet"/> is <c>null</c>.</exception>
        public IEnumerable<(T Element, IReadOnlyList<Rule<T>> MatchedRules)> ProjectWithMatches(RuleSet<T> ruleSet)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(ruleSet, nameof(ruleSet));
            return EvaluateIterator(source, ruleSet).Select(static ctx => (ctx.Element, ctx.MatchedRules));
        }

        ///<summary>
        ///Evaluates all enabled rules and returns only elements that matched all enabled rules.
        ///</summary>
        ///<typeparam name="T">The element type.</typeparam>
        ///<param name="source">The source sequence.</param>
        ///<param name="ruleSet">The rule set to evaluate.</param>
        ///<returns>Elements that matched every enabled rule.</returns>
        ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="ruleSet"/> is <c>null</c>.</exception>
        public IEnumerable<T> WhereAllRulesMatch(RuleSet<T> ruleSet)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(ruleSet, nameof(ruleSet));

            int enabledCount = ruleSet.Enabled().Count();
            return EvaluateIterator(source, ruleSet).Where(ctx => ctx.MatchCount == enabledCount).Select(ctx => ctx.Element);
        }

        ///<summary>
        ///Evaluates all enabled rules and returns only elements that matched at least one rule.
        ///</summary>
        ///<typeparam name="T">The element type.</typeparam>
        ///<param name="source">The source sequence.</param>
        ///<param name="ruleSet">The rule set to evaluate.</param>
        ///<returns>Elements that matched at least one rule.</returns>
        ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="ruleSet"/> is <c>null</c>.</exception>
        public IEnumerable<T> WhereAnyRuleMatches(RuleSet<T> ruleSet)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(ruleSet, nameof(ruleSet));
            return EvaluateIterator(source, ruleSet).Where(static ctx => ctx.HasMatch).Select(static ctx => ctx.Element);
        }
    }

    extension<T>(IQueryable<T> source)
    {
        ///<summary>
        ///Applies all enabled rule conditions combined with AND as a single <c>Where</c> clause on the queryable source. The
        ///combined expression tree is provider-translatable.
        ///</summary>
        ///<typeparam name="T">The element type.</typeparam>
        ///<param name="source">The queryable source.</param>
        ///<param name="ruleSet">The rule set whose conditions are combined.</param>
        ///<returns>A filtered queryable.</returns>
        ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="ruleSet"/> is <c>null</c>.</exception>
        public IQueryable<T> WhereAllRulesMatch(RuleSet<T> ruleSet)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(ruleSet, nameof(ruleSet));
            return source.Where(ruleSet.CombineWithAnd());
        }

        ///<summary>
        ///Applies all enabled rule conditions combined with OR as a single <c>Where</c> clause on the queryable source.
        ///</summary>
        ///<typeparam name="T">The element type.</typeparam>
        ///<param name="source">The queryable source.</param>
        ///<param name="ruleSet">The rule set whose conditions are combined.</param>
        ///<returns>A filtered queryable.</returns>
        ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="ruleSet"/> is <c>null</c>.</exception>
        public IQueryable<T> WhereAnyRuleMatches(RuleSet<T> ruleSet)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(ruleSet, nameof(ruleSet));
            return source.Where(ruleSet.CombineWithOr());
        }

        ///<summary>
        ///Applies a single named rule's condition as a <c>Where</c> clause on the queryable source.
        ///</summary>
        ///<typeparam name="T">The element type.</typeparam>
        ///<param name="source">The queryable source.</param>
        ///<param name="ruleSet">The rule set to search.</param>
        ///<param name="ruleName">The name of the rule to apply.</param>
        ///<returns>A filtered queryable.</returns>
        ///<exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        ///<exception cref="InvalidOperationException">No rule with <paramref name="ruleName"/> exists.</exception>
        public IQueryable<T> WhereRuleMatches(RuleSet<T> ruleSet, string ruleName)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(ruleSet, nameof(ruleSet));
            ArgumentNullException.ThrowIfNull(ruleName, nameof(ruleName));

            Rule<T> rule = ruleSet.FindByName(ruleName) ?? throw new InvalidOperationException($"Rule '{ruleName}' not found in the rule set.");

            return source.Where(rule.Condition);
        }

        ///<summary>
        ///Applies enabled rules with the specified tags (combined with OR) as a <c>Where</c> clause.
        ///</summary>
        ///<typeparam name="T">The element type.</typeparam>
        ///<param name="source">The queryable source.</param>
        ///<param name="ruleSet">The rule set to search.</param>
        ///<param name="tags">The tags to filter rules by.</param>
        ///<returns>A filtered queryable.</returns>
        ///<exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public IQueryable<T> WhereRulesWithTagsMatch(RuleSet<T> ruleSet, params string[] tags)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(ruleSet, nameof(ruleSet));
            ArgumentNullException.ThrowIfNull(tags, nameof(tags));

            IEnumerable<Expression<Func<T, bool>>> conditions = ruleSet.WithAnyTag(tags).Where(static r => r.IsEnabled).Select(static r => r.Condition);

            return source.Where(ExpressionComposer.OrAny(conditions));
        }
    }
}
