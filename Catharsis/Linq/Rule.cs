using Catharsis.Linq.Expressions;
using System.Linq.Expressions;

namespace Catharsis.Linq;

///<summary>
///Represents a named, prioritised rule with an expression-tree condition and an optional action. Rules are the
///fundamental building blocks of the rule engine. The condition is an <see cref="Expression{TDelegate}"/> so it can be
///translated by <see cref="IQueryProvider"/> implementations (e.g. to SQL) or composed with ///<see
///cref="ExpressionComposer"/>.
///</summary>
///<typeparam name="T">The type of element the rule applies to.</typeparam>
public sealed class Rule<T>
{
    #region Fields
    private Func<T, bool>? _compiledCondition;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a rule with the specified name and condition.
    ///</summary>
    ///<param name="name">A unique name identifying this rule.</param>
    ///<param name="condition">An expression-tree predicate that determines whether an element matches.</param>
    ///<exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="condition"/> is <c>null</c>.</exception>
    public Rule(string name, Expression<Func<T, bool>> condition)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        ArgumentNullException.ThrowIfNull(condition, nameof(condition));

        Name = name;
        Condition = condition;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a new rule whose condition is the logical AND of this rule's condition and ///<paramref name="other"/>'s
    ///condition.
    ///</summary>
    ///<param name="other">The rule to combine with.</param>
    ///<returns>A new combined rule.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public Rule<T> And(Rule<T> other)
    {
        ArgumentNullException.ThrowIfNull(other, nameof(other));
        return new Rule<T>($"({Name} AND {other.Name})", ExpressionComposer.AndAlso(Condition, other.Condition)) { Priority = Math.Min(Priority, other.Priority), Tags = new HashSet<string>(Tags.Concat(other.Tags), StringComparer.Ordinal) };
    }

    ///<summary>
    ///Evaluates the condition against a single element.
    ///</summary>
    ///<param name="element">The element to test.</param>
    ///<returns><c>true</c> if the element satisfies the condition; otherwise <c>false</c>.</returns>
    public bool Evaluate(T element) { return CompiledCondition(element); }
    ///<summary>
    ///Creates a new rule whose condition is the negation of this rule's condition.
    ///</summary>
    ///<returns>A new rule with a negated condition.</returns>
    public Rule<T> Negate() { return new($"NOT({Name})", ExpressionComposer.Not(Condition)) { Priority = Priority, Tags = Tags }; }

    ///<summary>
    ///Creates a new rule whose condition is the logical OR of this rule's condition and ///<paramref name="other"/>'s
    ///condition.
    ///</summary>
    ///<param name="other">The rule to combine with.</param>
    ///<returns>A new combined rule.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public Rule<T> Or(Rule<T> other)
    {
        ArgumentNullException.ThrowIfNull(other, nameof(other));
        return new Rule<T>($"({Name} OR {other.Name})", ExpressionComposer.OrElse(Condition, other.Condition)) { Priority = Math.Min(Priority, other.Priority), Tags = new HashSet<string>(Tags.Concat(other.Tags), StringComparer.Ordinal) };
    }

    ///<inheritdoc/>
    public override string ToString() { return $"Rule '{Name}' (Priority={Priority}, Enabled={IsEnabled})"; }
    #endregion

    #region Public properties
    ///<summary>
    ///Returns the compiled delegate for the condition, caching it on first access.
    ///</summary>
    ///<returns>A delegate that evaluates the condition.</returns>
    public Func<T, bool> CompiledCondition => _compiledCondition ??= Condition.Compile();

    ///<summary>
    ///An expression-tree predicate that determines whether an element matches.
    ///</summary>
    public Expression<Func<T, bool>> Condition { get; }

    ///<summary>
    ///An optional description for documentation or diagnostics.
    ///</summary>
    public string? Description { get; init; }

    ///<summary>
    ///Whether this rule is enabled. Disabled rules are skipped during evaluation. Defaults to <c>true</c>.
    ///</summary>
    public bool IsEnabled { get; init; } = true;

    ///<summary>
    ///A unique name identifying this rule.
    ///</summary>
    public string Name { get; }

    ///<summary>
    ///An optional action to execute when the rule matches an element. This is used by the in-memory evaluator and is
    ///not translatable to a query provider.
    ///</summary>
    public Action<T>? OnMatch { get; init; }

    ///<summary>
    ///The evaluation priority. Lower values are evaluated first. Defaults to <c>0</c>.
    ///</summary>
    public int Priority { get; init; }

    ///<summary>
    ///When <c>true</c>, a match on this rule prevents subsequent rules from being evaluated for the same element.
    ///Defaults to <c>false</c>.
    ///</summary>
    public bool StopOnMatch { get; init; }

    ///<summary>
    ///An optional set of tags for categorising and filtering rules.
    ///</summary>
    public IReadOnlySet<string> Tags { get; init; } = new HashSet<string>(StringComparer.Ordinal);
    #endregion
}
