using Catharsis.Events;

namespace Catharsis.Domain;

///<summary>
///Base class for an aggregate root: the entity through which a cluster of related objects is changed, and which
///records the <see cref="DomainEvent"/>s describing those changes. Events are collected as the aggregate changes and
///published afterwards with <see cref="DispatchEventsAsync"/>, so nothing is announced until the change is complete.
///</summary>
///<typeparam name="TId">The identity type.</typeparam>
///<param name="id">The identity. Must not be <c>null</c>.</param>
///<exception cref="ArgumentNullException"><paramref name="id"/> is <c>null</c>.</exception>
public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id)
    where TId : notnull, IEquatable<TId>
{
    #region Fields
    readonly List<DomainEvent> _events = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Publishes every recorded event, in the order it was raised, and removes each once it has been handled.
    ///</summary>
    ///<remarks>
    ///If a handler throws, the failing event and those after it stay recorded, so calling this again retries them.
    ///Delivery is therefore at-least-once, and handlers should tolerate seeing an event twice.
    ///</remarks>
    ///<param name="dispatcher">The dispatcher that delivers events to subscribers.</param>
    ///<param name="cancellationToken">A token that can stop dispatching between events.</param>
    ///<returns>A task that completes when every event has been handled.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="dispatcher"/> is <c>null</c>.</exception>
    public async Task DispatchEventsAsync(DomainEventDispatcher dispatcher, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        while (_events.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await dispatcher.DispatchAsync(_events[0], cancellationToken).ConfigureAwait(false);
            _events.RemoveAt(0);
        }
    }

    ///<summary>
    ///Publishes every recorded event like <see cref="DispatchEventsAsync"/>, returning a <see cref="ValueTask"/> that is
    ///already complete, and allocates nothing, when there is nothing to publish.
    ///</summary>
    ///<param name="dispatcher">The dispatcher that delivers events to subscribers.</param>
    ///<param name="cancellationToken">A token that can stop dispatching between events.</param>
    ///<returns>A task that completes when every event has been handled.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="dispatcher"/> is <c>null</c>.</exception>
    public ValueTask DispatchEventsValueAsync(DomainEventDispatcher dispatcher, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        return _events.Count == 0 ? ValueTask.CompletedTask : new ValueTask(DispatchEventsAsync(dispatcher, cancellationToken));
    }

    ///<summary>
    ///Discards every recorded event without publishing it.
    ///</summary>
    public void ClearEvents() => _events.Clear();
    #endregion

    #region Public properties
    ///<summary>Gets the events raised since the last dispatch, oldest first.</summary>
    public IReadOnlyList<DomainEvent> DomainEvents => _events;
    #endregion

    #region Protected methods
    ///<summary>
    ///Records an event describing a change that has just been made.
    ///</summary>
    ///<param name="domainEvent">The event to record.</param>
    ///<exception cref="ArgumentNullException"><paramref name="domainEvent"/> is <c>null</c>.</exception>
    protected void Raise(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _events.Add(domainEvent);
    }
    #endregion
}
