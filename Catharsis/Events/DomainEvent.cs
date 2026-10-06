namespace Catharsis.Events;

///<summary>
///Base type for a domain event: something meaningful that happened to an aggregate, recorded with the instant it
///occurred so handlers and event stores don't need to timestamp it themselves.
///</summary>
public abstract record DomainEvent
{
    #region Public properties
    ///<summary>
    ///Gets the instant this event occurred. Defaults to the moment the event is constructed.
    ///</summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    #endregion
}
