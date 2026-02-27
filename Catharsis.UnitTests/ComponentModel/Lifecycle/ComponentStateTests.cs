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
