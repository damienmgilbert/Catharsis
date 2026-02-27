namespace Catharsis.DesignPatterns.Behavioral;

/// <summary>
/// Implements the State design pattern.
/// </summary>
public class StatePattern
{
    /// <summary>
    /// State — selects a behavior based on the current <paramref name="state"/>
    /// via <paramref name="behaviorSelector"/> and applies it to <paramref name="obj"/>.
    /// </summary>
    /// <typeparam name="T">The context type.</typeparam>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="obj">The context object.</param>
    /// <param name="state">The current state.</param>
    /// <param name="behaviorSelector">A delegate that maps a state to an action.</param>
    /// <returns>The original <paramref name="obj"/> after the state-specific behavior executes.</returns>
    public T State<T, TState>(T obj, TState state, Func<TState, Action<T>> behaviorSelector)
    {
        if (behaviorSelector is null) throw new ArgumentNullException(nameof(behaviorSelector), "Behavior selector must not be null.");

        var behavior = behaviorSelector(state);
        behavior(obj);
        return obj;
    }

    /// <summary>
    /// State — selects a behavior based on the current <paramref name="state"/>
    /// via <paramref name="behaviorSelector"/>, applies it to <paramref name="obj"/>,
    /// and returns the result.
    /// </summary>
    /// <typeparam name="T">The context type.</typeparam>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="obj">The context object.</param>
    /// <param name="state">The current state.</param>
    /// <param name="behaviorSelector">A delegate that maps a state to a function.</param>
    /// <returns>The result of the state-specific behavior.</returns>
    public TResult State<T, TState, TResult>(T obj, TState state, Func<TState, Func<T, TResult>> behaviorSelector)
    {
        if (behaviorSelector is null) throw new ArgumentNullException(nameof(behaviorSelector), "Behavior selector must not be null.");

        var behavior = behaviorSelector(state);
        return behavior(obj);
    }
}
