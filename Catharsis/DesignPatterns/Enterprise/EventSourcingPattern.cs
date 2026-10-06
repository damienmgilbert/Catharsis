namespace Catharsis.DesignPatterns.Enterprise;

///<summary>
///Implements the Event Sourcing design pattern.
///</summary>
public class EventSourcingPattern
{
    #region Public methods

    ///<summary>
    ///Event Sourcing — reconstructs current state by replaying a sequence of events onto an initial state, one
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
