namespace Catharsis.Linq;

///<summary>
///Carries the evaluation state for a single element after being tested against a <see cref="RuleSet{T}"/>. Contains the
///element, the list of matched rules, and whether evaluation was short-circuited by a ///<see
///cref="Rule{T}.StopOnMatch"/> rule.
///</summary>
///<typeparam name="T">The element type.</typeparam>
public sealed class RuleContext<T>
{
    #region Fields
    private readonly List<Rule<T>> _matchedRules = [];
    private readonly Dictionary<string, object?> _properties = [ with(StringComparer.Ordinal) ];
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a rule context for the specified element.
    ///</summary>
    ///<param name="element">The element being evaluated.</param>
    internal RuleContext(T element) { Element = element; }
    #endregion

    #region Internal methods
    ///<summary>
    ///Records a rule match and optionally stops further evaluation.
    ///</summary>
    ///<param name="rule">The matched rule.</param>
    internal void RecordMatch(Rule<T> rule)
    {
        _matchedRules.Add(rule);

        if(rule.StopOnMatch)
        {
            WasStopped = true;
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///The element being evaluated.
    ///</summary>
    public T Element { get; }

    ///<summary>
    ///The first matched rule, or <c>null</c> if none matched.
    ///</summary>
    public Rule<T>? FirstMatch => _matchedRules.Count > 0 ? _matchedRules[0] : null;

    ///<summary>
    ///Whether the element matched at least one rule.
    ///</summary>
    public bool HasMatch => _matchedRules.Count > 0;

    ///<summary>
    ///The number of rules that matched.
    ///</summary>
    public int MatchCount => _matchedRules.Count;

    ///<summary>
    ///The rules that matched this element, in evaluation order.
    ///</summary>
    public IReadOnlyList<Rule<T>> MatchedRules => _matchedRules.AsReadOnly();

    ///<summary>
    ///A general-purpose property bag for attaching custom data during evaluation.
    ///</summary>
    public IDictionary<string, object?> Properties => _properties;

    ///<summary>
    ///Whether evaluation was stopped early because a rule with <see cref="Rule{T}.StopOnMatch"/> matched.
    ///</summary>
    public bool WasStopped { get; private set; }
    #endregion
}
