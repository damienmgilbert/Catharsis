using Catharsis.ComponentModel.Lifecycle;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ComponentTransition"/> class.
///</summary>
[TestClass]
public class ComponentTransitionTests
{
    #region Public methods
    [TestMethod]
    public void CanExecute_GuardReturnsFalse_ReturnsFalse()
    {
        ComponentTransition t = new ComponentTransition(ComponentState.Created, ComponentState.Initialized) { Guard = static () => false };
        Assert.IsFalse(t.CanExecute());
    }

    [TestMethod]
    public void CanExecute_NoGuard_ReturnsTrue()
    {
        ComponentTransition t = new ComponentTransition(ComponentState.Created, ComponentState.Initialized);
        Assert.IsTrue(t.CanExecute());
    }

    [TestMethod]
    public void Constructor_CustomName()
    {
        ComponentTransition t = new ComponentTransition(ComponentState.Active, ComponentState.Deactivating, "Pause");
        Assert.AreEqual("Pause", t.Name);
    }

    [TestMethod]
    public void Constructor_SetsProperties()
    {
        ComponentTransition t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing);
        Assert.AreEqual(ComponentState.Created, t.From);
        Assert.AreEqual(ComponentState.Initializing, t.To);
        Assert.AreEqual("Created -> Initializing", t.Name);
    }

    [TestMethod]
    public void OnTransition_Invoked()
    {
        bool invoked = false;
        ComponentTransition t = new ComponentTransition(ComponentState.Created, ComponentState.Initialized) { OnTransition = () => invoked = true };
        t.OnTransition?.Invoke();
        Assert.IsTrue(invoked);
    }

    [TestMethod]
    public void ToString_ReturnsName()
    {
        ComponentTransition t = new ComponentTransition(ComponentState.Active, ComponentState.Disposed);
        Assert.AreEqual("Active -> Disposed", t.ToString());
    }
    #endregion
}
