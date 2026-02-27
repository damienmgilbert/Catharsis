using Catharsis.ComponentModel.Lifecycle;

namespace Catharsis.UnitTests.ComponentModel.Lifecycle;

[TestClass]
public sealed class ComponentStateTests
{
    [TestMethod]
    public void EnumValues_AreDefined()
    {
        Assert.AreEqual(0, (int)ComponentState.Created);
        Assert.AreEqual(1, (int)ComponentState.Initializing);
        Assert.AreEqual(2, (int)ComponentState.Initialized);
        Assert.AreEqual(3, (int)ComponentState.Activating);
        Assert.AreEqual(4, (int)ComponentState.Active);
        Assert.AreEqual(5, (int)ComponentState.Deactivating);
        Assert.AreEqual(6, (int)ComponentState.Deactivated);
        Assert.AreEqual(7, (int)ComponentState.Disposing);
        Assert.AreEqual(8, (int)ComponentState.Disposed);
    }
}

[TestClass]
public sealed class ComponentTransitionTests
{
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing);

        Assert.AreEqual(ComponentState.Created, t.From);
        Assert.AreEqual(ComponentState.Initializing, t.To);
        Assert.IsNotNull(t.Name);
    }

    [TestMethod]
    public void Constructor_CustomName_SetsName()
    {
        var t = new ComponentTransition(ComponentState.Deactivated, ComponentState.Activating, "Reactivate");

        Assert.AreEqual("Reactivate", t.Name);
    }

    [TestMethod]
    public void CanExecute_NoGuard_ReturnsTrue()
    {
        var t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing);

        Assert.IsTrue(t.CanExecute());
    }

    [TestMethod]
    public void CanExecute_GuardReturnsTrue_ReturnsTrue()
    {
        var t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing)
        {
            Guard = () => true
        };

        Assert.IsTrue(t.CanExecute());
    }

    [TestMethod]
    public void CanExecute_GuardReturnsFalse_ReturnsFalse()
    {
        var t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing)
        {
            Guard = () => false
        };

        Assert.IsFalse(t.CanExecute());
    }

    [TestMethod]
    public void ToString_ReturnsName()
    {
        var t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing, "Init");

        Assert.AreEqual("Init", t.ToString());
    }
}

[TestClass]
public sealed class ComponentStateMachineTests
{
    [TestMethod]
    public void Initial_State_IsCreated()
    {
        var machine = new ComponentStateMachine();

        Assert.AreEqual(ComponentState.Created, machine.CurrentState);
    }

    [TestMethod]
    public void AddTransition_NullTransition_ThrowsArgumentNullException()
    {
        var machine = new ComponentStateMachine();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => machine.AddTransition(null!));
    }

    [TestMethod]
    public void AddTransition_IncreasesTransitionsCount()
    {
        var machine = new ComponentStateMachine();

        machine.AddTransition(new ComponentTransition(ComponentState.Created, ComponentState.Initializing));

        Assert.AreEqual(1, machine.Transitions.Count);
    }

    [TestMethod]
    public void ConfigureDefaults_RegistersStandardTransitions()
    {
        var machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.IsTrue(machine.Transitions.Count > 0);
        Assert.IsTrue(machine.CanTransitionTo(ComponentState.Initializing));
    }

    [TestMethod]
    public void CanTransitionTo_ValidTransition_ReturnsTrue()
    {
        var machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.IsTrue(machine.CanTransitionTo(ComponentState.Initializing));
    }

    [TestMethod]
    public void CanTransitionTo_InvalidTransition_ReturnsFalse()
    {
        var machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.IsFalse(machine.CanTransitionTo(ComponentState.Active));
    }

    [TestMethod]
    public void TryTransitionTo_ValidTransition_ReturnsTrue()
    {
        var machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.IsTrue(machine.TryTransitionTo(ComponentState.Initializing));
        Assert.AreEqual(ComponentState.Initializing, machine.CurrentState);
    }

    [TestMethod]
    public void TryTransitionTo_InvalidTransition_ReturnsFalse()
    {
        var machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.IsFalse(machine.TryTransitionTo(ComponentState.Active));
        Assert.AreEqual(ComponentState.Created, machine.CurrentState);
    }

    [TestMethod]
    public void TransitionTo_ValidTransition_ChangesState()
    {
        var machine = new ComponentStateMachine().ConfigureDefaults();

        machine.TransitionTo(ComponentState.Initializing);

        Assert.AreEqual(ComponentState.Initializing, machine.CurrentState);
    }

    [TestMethod]
    public void TransitionTo_InvalidTransition_ThrowsInvalidOperationException()
    {
        var machine = new ComponentStateMachine().ConfigureDefaults();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => machine.TransitionTo(ComponentState.Active));
    }

    [TestMethod]
    public void TransitionTo_RaisesStateChanged()
    {
        var machine = new ComponentStateMachine().ConfigureDefaults();
        ComponentStateMachine.StateChangedEventArgs? args = null;
        machine.StateChanged += (s, e) => args = e;

        machine.TransitionTo(ComponentState.Initializing);

        Assert.IsNotNull(args);
        Assert.AreEqual(ComponentState.Created, args.PreviousState);
        Assert.AreEqual(ComponentState.Initializing, args.NewState);
    }

    [TestMethod]
    public void TryTransitionTo_OnTransitionAction_IsInvoked()
    {
        var invoked = false;
        var machine = new ComponentStateMachine();
        machine.AddTransition(new ComponentTransition(ComponentState.Created, ComponentState.Initializing)
        {
            OnTransition = () => invoked = true
        });

        machine.TryTransitionTo(ComponentState.Initializing);

        Assert.IsTrue(invoked);
    }

    [TestMethod]
    public void TryTransitionTo_GuardBlocksTransition()
    {
        var machine = new ComponentStateMachine();
        machine.AddTransition(new ComponentTransition(ComponentState.Created, ComponentState.Initializing)
        {
            Guard = () => false
        });

        Assert.IsFalse(machine.TryTransitionTo(ComponentState.Initializing));
        Assert.AreEqual(ComponentState.Created, machine.CurrentState);
    }

    [TestMethod]
    public void Reset_ResetsToCreated()
    {
        var machine = new ComponentStateMachine().ConfigureDefaults();
        machine.TransitionTo(ComponentState.Initializing);

        machine.Reset();

        Assert.AreEqual(ComponentState.Created, machine.CurrentState);
    }

    [TestMethod]
    public void FullLifecycle_CreatedToDisposed()
    {
        var machine = new ComponentStateMachine().ConfigureDefaults();

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
    public void Reactivation_DeactivatedToActive()
    {
        var machine = new ComponentStateMachine().ConfigureDefaults();
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
}
