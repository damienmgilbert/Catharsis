using Catharsis.Linq;

namespace Catharsis.RuleEngine;

///<summary>
///Published on the <see cref="Catharsis.Events.EventBus"/> by <see cref="ManagedRuleEngine{T}"/> each time an
///element matches a rule within one of its registered <see cref="RuleSet{T}"/> instances.
///</summary>
///<typeparam name="T">The type of element that was evaluated.</typeparam>
///<param name="RuleSetName">The name under which the matching rule set was registered.</param>
///<param name="Rule">The individual rule that matched.</param>
///<param name="Element">The element that matched the rule.</param>
///<param name="MatchedAtUtc">The UTC timestamp at which the match was recorded.</param>
public sealed record RuleMatchedEvent<T>(string RuleSetName, Rule<T> Rule, T Element, DateTimeOffset MatchedAtUtc);
