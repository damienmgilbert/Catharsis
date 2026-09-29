using Catharsis.Events;
using Catharsis.Operators;

namespace Catharsis.Domain;

///<summary>
///Raised when an <see cref="Order"/> is confirmed.
///</summary>
///<param name="OrderId">The confirmed order.</param>
///<param name="Total">The order total at confirmation.</param>
public sealed record OrderConfirmed(Guid OrderId, Money Total) : DomainEvent;
