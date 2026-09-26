namespace Catharsis.ComponentModel.Lifecycle;

///<summary>
///Represents a valid transition between two <see cref="ComponentState"/> values, with an optional guard condition and
///action to execute during the transition.
///</summary>
///<remarks>
///Transitions are registered with a <see cref="ComponentStateMachine"/> to define the valid state graph. The <see
///cref="Guard"/> predicate must return <c>true</c> for the transition to proceed; the <see cref="OnTransition"/> action
///is invoked after the state change.
///</remarks>
///<remarks>
///Initializes a new instance of <see cref="ComponentTransition"/>.
///</remarks>
///<param name="from">The source state.</param>
///<param name="to">The destination state.</param>
///<param name="name">An optional human-readable name for this transition.</param>
public sealed class ComponentTransition(ComponentState from, ComponentState to, string? name = null)
{

    #region Constructors
    #endregion

    #region Public methods
    ///<summary>
    ///Evaluates whether this transition is currently allowed.
    ///</summary>
    ///<returns><c>true</c> if the guard is <c>null</c> or returns <c>true</c>; otherwise, <c>false</c>.</returns>
    public bool CanExecute() { return Guard?.Invoke() ?? true; }
    ///<inheritdoc/>
    public override string ToString() { return Name; }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the source state.
    ///</summary>
    public ComponentState From { get; } = from;

    ///<summary>
    ///Gets or sets an optional guard predicate. The transition only proceeds if this returns <c>true</c> (or is
    ///<c>null</c>).
    ///</summary>
    public Func<bool>? Guard { get; set; }

    ///<summary>
    ///Gets the human-readable name for this transition.
    ///</summary>
    public string Name { get; } = name ?? $"{from} -> {to}";

    ///<summary>
    ///Gets or sets an optional action invoked after the state change completes.
    ///</summary>
    public Action? OnTransition { get; set; }

    ///<summary>
    ///Gets the destination state.
    ///</summary>
    public ComponentState To { get; } = to;
    #endregion
}
