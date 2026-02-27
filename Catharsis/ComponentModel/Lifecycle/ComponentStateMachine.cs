using System.ComponentModel;

namespace Catharsis.ComponentModel.Lifecycle;

/// <summary>
/// A finite state machine that manages lifecycle state transitions for a
/// single component, enforcing valid transitions and executing associated
/// guards and actions.
/// </summary>
/// <remarks>
/// <para>
/// Register valid transitions via <see cref="AddTransition"/>. Call
/// <see cref="TryTransitionTo"/> to attempt a state change. The machine
/// raises <see cref="StateChanged"/> after each successful transition.
/// </para>
/// <para>
/// Use <see cref="ConfigureDefaults"/> to register the standard lifecycle
/// transitions (Created → Initialized → Active → Deactivated → Disposed).
/// </para>
/// </remarks>
public sealed class ComponentStateMachine
{
    private readonly List<ComponentTransition> _transitions = [];
    private ComponentState _currentState = ComponentState.Created;

    /// <summary>
    /// Gets the current lifecycle state.
    /// </summary>
    public ComponentState CurrentState => _currentState;

    /// <summary>
    /// Gets all registered transitions.
    /// </summary>
    public IReadOnlyList<ComponentTransition> Transitions => _transitions;

    /// <summary>
    /// Raised after a successful state transition.
    /// </summary>
    public event EventHandler<StateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Registers a valid transition.
    /// </summary>
    /// <param name="transition">The transition to register.</param>
    /// <returns>This machine, for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="transition"/> is <c>null</c>.
    /// </exception>
    public ComponentStateMachine AddTransition(ComponentTransition transition)
    {
        ArgumentNullException.ThrowIfNull(transition);
        _transitions.Add(transition);
        return this;
    }

    /// <summary>
    /// Registers the standard lifecycle transitions.
    /// </summary>
    /// <returns>This machine, for fluent chaining.</returns>
    public ComponentStateMachine ConfigureDefaults()
    {
        AddTransition(new ComponentTransition(ComponentState.Created, ComponentState.Initializing));
        AddTransition(new ComponentTransition(ComponentState.Initializing, ComponentState.Initialized));
        AddTransition(new ComponentTransition(ComponentState.Initialized, ComponentState.Activating));
        AddTransition(new ComponentTransition(ComponentState.Activating, ComponentState.Active));
        AddTransition(new ComponentTransition(ComponentState.Active, ComponentState.Deactivating));
        AddTransition(new ComponentTransition(ComponentState.Deactivating, ComponentState.Deactivated));
        AddTransition(new ComponentTransition(ComponentState.Deactivated, ComponentState.Activating, "Reactivate"));
        AddTransition(new ComponentTransition(ComponentState.Deactivated, ComponentState.Disposing));
        AddTransition(new ComponentTransition(ComponentState.Active, ComponentState.Disposing));
        AddTransition(new ComponentTransition(ComponentState.Disposing, ComponentState.Disposed));

        // Fault transitions: any non-terminal state can fault.
        AddTransition(new ComponentTransition(ComponentState.Initializing, ComponentState.Faulted));
        AddTransition(new ComponentTransition(ComponentState.Activating, ComponentState.Faulted));
        AddTransition(new ComponentTransition(ComponentState.Active, ComponentState.Faulted));
        AddTransition(new ComponentTransition(ComponentState.Deactivating, ComponentState.Faulted));

        return this;
    }

    /// <summary>
    /// Determines whether a transition to the specified state is valid from
    /// the current state.
    /// </summary>
    /// <param name="targetState">The desired target state.</param>
    /// <returns><c>true</c> if the transition is valid; otherwise, <c>false</c>.</returns>
    public bool CanTransitionTo(ComponentState targetState)
    {
        return FindTransition(targetState) is { } transition && transition.CanExecute();
    }

    /// <summary>
    /// Attempts to transition to the specified state.
    /// </summary>
    /// <param name="targetState">The desired target state.</param>
    /// <returns><c>true</c> if the transition succeeded; <c>false</c> if it was not valid.</returns>
    public bool TryTransitionTo(ComponentState targetState)
    {
        var transition = FindTransition(targetState);

        if (transition is null || !transition.CanExecute())
            return false;

        var previousState = _currentState;
        _currentState = targetState;

        transition.OnTransition?.Invoke();
        StateChanged?.Invoke(this, new StateChangedEventArgs(previousState, targetState, transition));

        return true;
    }

    /// <summary>
    /// Transitions to the specified state, or throws if the transition is invalid.
    /// </summary>
    /// <param name="targetState">The desired target state.</param>
    /// <exception cref="InvalidOperationException">
    /// No valid transition exists from the current state to <paramref name="targetState"/>.
    /// </exception>
    public void TransitionTo(ComponentState targetState)
    {
        if (!TryTransitionTo(targetState))
        {
            throw new InvalidOperationException(
                $"No valid transition from '{_currentState}' to '{targetState}'.");
        }
    }

    /// <summary>
    /// Resets the machine to the <see cref="ComponentState.Created"/> state
    /// without raising events.
    /// </summary>
    public void Reset() => _currentState = ComponentState.Created;

    private ComponentTransition? FindTransition(ComponentState targetState)
    {
        foreach (var transition in _transitions)
        {
            if (transition.From == _currentState && transition.To == targetState)
                return transition;
        }

        return null;
    }

    /// <summary>
    /// Event arguments for the <see cref="ComponentStateMachine.StateChanged"/> event.
    /// </summary>
    /// <param name="PreviousState">The state before the transition.</param>
    /// <param name="NewState">The state after the transition.</param>
    /// <param name="Transition">The transition that was executed.</param>
    public sealed record StateChangedEventArgs(
        ComponentState PreviousState,
        ComponentState NewState,
        ComponentTransition Transition);
}
