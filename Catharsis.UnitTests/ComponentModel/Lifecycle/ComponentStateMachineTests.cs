using Catharsis.ComponentModel.Lifecycle;

namespace Catharsis.UnitTests.ComponentModel.Lifecycle;

///<summary>
///Unit tests for the <see cref="ComponentStateMachine"/> class.
///</summary>
[TestClass]
public sealed class ComponentStateMachineTests
{
    [TestMethod]
    public void AddTransition_IncreasesTransitionsCount()
    {
        ComponentStateMachine machine = new ComponentStateMachine();

        machine.AddTransition(new ComponentTransition(ComponentState.Created, ComponentState.Initializing));

        Assert.HasCount(1, machine.Transitions);
    }

    [TestMethod]
    public void AddTransition_NullTransition_ThrowsArgumentNullException()
    {
        ComponentStateMachine machine = new ComponentStateMachine();

        Assert.ThrowsExactly<ArgumentNullException>(() => machine.AddTransition(null!));
    }

    [TestMethod]
    public void CanTransitionTo_InvalidTransition_ReturnsFalse()
    {
        ComponentStateMachine machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.IsFalse(machine.CanTransitionTo(ComponentState.Active));
    }

    [TestMethod]
    public void CanTransitionTo_ValidTransition_ReturnsTrue()
    {
        ComponentStateMachine machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.IsTrue(machine.CanTransitionTo(ComponentState.Initializing));
    }

    [TestMethod]
    public void ConfigureDefaults_RegistersStandardTransitions()
    {
        ComponentStateMachine machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.IsNotEmpty(machine.Transitions);
        Assert.IsTrue(machine.CanTransitionTo(ComponentState.Initializing));
    }

    [TestMethod]
    public void FullLifecycle_CreatedToDisposed()
    {
        ComponentStateMachine machine = new ComponentStateMachine().ConfigureDefaults();

        machine.TransitionTo(ComponentState.Initializing);
        machine.TransitionTo(ComponentState.Initialized);
        machine.TransitionTo(ComponentState.Activating);
        machine.TransitionTo(ComponentState.Active);
        machine.TransitionTo(ComponentState.Deactivating);
        machine.TransitionTo(ComponentState.Deactivated);
        machine.TransitionTo(ComponentState.Disposing);
        machine.TransitionTo(ComponentState.Disposed);

        Assert.AreEqual(ComponentState.Disposed, machine.CurrentState);
    }

    [TestMethod]
    public void Initial_State_IsCreated()
    {
        ComponentStateMachine machine = new ComponentStateMachine();

        Assert.AreEqual(ComponentState.Created, machine.CurrentState);
    }

    [TestMethod]
    public void Reactivation_DeactivatedToActive()
    {
        ComponentStateMachine machine = new ComponentStateMachine().ConfigureDefaults();
        machine.TransitionTo(ComponentState.Initializing);
        machine.TransitionTo(ComponentState.Initialized);
        machine.TransitionTo(ComponentState.Activating);
        machine.TransitionTo(ComponentState.Active);
        machine.TransitionTo(ComponentState.Deactivating);
        machine.TransitionTo(ComponentState.Deactivated);

        machine.TransitionTo(ComponentState.Activating);
        machine.TransitionTo(ComponentState.Active);

        Assert.AreEqual(ComponentState.Active, machine.CurrentState);
    }

    [TestMethod]
    public void Reset_ResetsToCreated()
    {
        ComponentStateMachine machine = new ComponentStateMachine().ConfigureDefaults();
        machine.TransitionTo(ComponentState.Initializing);

        machine.Reset();

        Assert.AreEqual(ComponentState.Created, machine.CurrentState);
    }

    [TestMethod]
    public void TransitionTo_InvalidTransition_ThrowsInvalidOperationException()
    {
        ComponentStateMachine machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.ThrowsExactly<InvalidOperationException>(() => machine.TransitionTo(ComponentState.Active));
    }

    [TestMethod]
    public void TransitionTo_RaisesStateChanged()
    {
        ComponentStateMachine machine = new ComponentStateMachine().ConfigureDefaults();
        ComponentStateMachine.StateChangedEventArgs? args = null;
        machine.StateChanged += (s, e) => args = e;

        machine.TransitionTo(ComponentState.Initializing);

        Assert.IsNotNull(args);
        Assert.AreEqual(ComponentState.Created, args.PreviousState);
        Assert.AreEqual(ComponentState.Initializing, args.NewState);
    }

    [TestMethod]
    public void TransitionTo_ValidTransition_ChangesState()
    {
        ComponentStateMachine machine = new ComponentStateMachine().ConfigureDefaults();

        machine.TransitionTo(ComponentState.Initializing);

        Assert.AreEqual(ComponentState.Initializing, machine.CurrentState);
    }

    [TestMethod]
    public void TryTransitionTo_GuardBlocksTransition()
    {
        ComponentStateMachine machine = new ComponentStateMachine();
        machine.AddTransition(new ComponentTransition(ComponentState.Created, ComponentState.Initializing) { Guard = static () => false });

        Assert.IsFalse(machine.TryTransitionTo(ComponentState.Initializing));
        Assert.AreEqual(ComponentState.Created, machine.CurrentState);
    }

    [TestMethod]
    public void TryTransitionTo_InvalidTransition_ReturnsFalse()
    {
        ComponentStateMachine machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.IsFalse(machine.TryTransitionTo(ComponentState.Active));
        Assert.AreEqual(ComponentState.Created, machine.CurrentState);
    }

    [TestMethod]
    public void TryTransitionTo_OnTransitionAction_IsInvoked()
    {
        bool invoked = false;
        ComponentStateMachine machine = new ComponentStateMachine();
        machine.AddTransition(new ComponentTransition(ComponentState.Created, ComponentState.Initializing) { OnTransition = () => invoked = true });

        machine.TryTransitionTo(ComponentState.Initializing);

        Assert.IsTrue(invoked);
    }

    [TestMethod]
    public void TryTransitionTo_ValidTransition_ReturnsTrue()
    {
        ComponentStateMachine machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.IsTrue(machine.TryTransitionTo(ComponentState.Initializing));
        Assert.AreEqual(ComponentState.Initializing, machine.CurrentState);
    }
}
