using Catharsis.ComponentModel.Lifecycle;

namespace Catharsis.UnitTests.ComponentModel.Lifecycle;

[TestClass]
public sealed class ComponentTransitionTests
{
    [TestMethod]
    public void CanExecute_GuardReturnsFalse_ReturnsFalse()
    {
        ComponentTransition t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing) { Guard = () => false };

        Assert.IsFalse(t.CanExecute());
    }

    [TestMethod]
    public void CanExecute_GuardReturnsTrue_ReturnsTrue()
    {
        ComponentTransition t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing) { Guard = () => true };

        Assert.IsTrue(t.CanExecute());
    }

    [TestMethod]
    public void CanExecute_NoGuard_ReturnsTrue()
    {
        ComponentTransition t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing);

        Assert.IsTrue(t.CanExecute());
    }

    [TestMethod]
    public void Constructor_CustomName_SetsName()
    {
        ComponentTransition t = new ComponentTransition(ComponentState.Deactivated, ComponentState.Activating, "Reactivate");

        Assert.AreEqual("Reactivate", t.Name);
    }

    [TestMethod]
    public void Constructor_SetsProperties()
    {
        ComponentTransition t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing);

        Assert.AreEqual(ComponentState.Created, t.From);
        Assert.AreEqual(ComponentState.Initializing, t.To);
        Assert.IsNotNull(t.Name);
    }

    [TestMethod]
    public void ToString_ReturnsName()
    {
        ComponentTransition t = new ComponentTransition(ComponentState.Created, ComponentState.Initializing, "Init");

        Assert.AreEqual("Init", t.ToString());
    }
}
