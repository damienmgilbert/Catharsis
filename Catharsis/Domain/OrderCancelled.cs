using Catharsis.Events;

namespace Catharsis.Domain;

///<summary>
///Raised when an <see cref="Order"/> is cancelled.
///</summary>
///<param name="OrderId">The cancelled order.</param>
///<param name="Reason">Why it was cancelled.</param>
///<param name="WasConfirmed"><c>true</c> if the order had already been confirmed, so stock may need releasing.</param>
public sealed record OrderCancelled(Guid OrderId, string Reason, bool WasConfirmed) : DomainEvent;
