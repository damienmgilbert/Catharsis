namespace Catharsis.DesignPatterns.Enterprise;

///<summary>
///Implements the Event Sourcing design pattern.
///</summary>
public class EventSourcingPattern
{
    #region Public methods
    ///<summary>
    ///Event Sourcing — reconstructs current state by replaying a sequence of events onto an initial state, one
    ///<paramref name="apply"/> call per event, in order.
    ///</summary>
    ///<typeparam name="TState">The type of the aggregate's state.</typeparam>
    ///<typeparam name="TEvent">The type of the events being replayed.</typeparam>
    ///<param name="initialState">The aggregate's state before any events are applied.</param>
    ///<param name="events">The events to replay, in the order they occurred.</param>
    ///<param name="apply">A delegate that folds a single event into the current state, producing the next state.</param>
    ///<returns>The state resulting from replaying every event in <paramref name="events"/> onto <paramref name="initialState"/>.</returns>
    public static TState Replay<TState, TEvent>(TState initialState, IEnumerable<TEvent> events, Func<TState, TEvent, TState> apply)
    {
        if(events is null)
        {
            throw new ArgumentNullException(nameof(events), "Events must not be null.");
        }

        if(apply is null)
        {
            throw new ArgumentNullException(nameof(apply), "Apply function must not be null.");
        }

        TState state = initialState;

        foreach(TEvent @event in events)
        {
            state = apply(state, @event);
        }

        return state;
    }
    #endregion
}
