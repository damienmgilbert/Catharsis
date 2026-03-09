using Catharsis.ComponentModel.Lifecycle;

namespace Catharsis.UnitTests.ComponentModel.Lifecycle;

///<summary>
///Unit tests for the <see cref="ComponentState"/> class.
///</summary>
[TestClass]
public sealed class ComponentStateTests
{
    [TestMethod]
    public void EnumValues_AreDefined()
    {
#pragma warning disable MSTEST0032 // Enum value guardrail assertions are intentionally constant
        Assert.AreEqual(0, (int)ComponentState.Created);
        Assert.AreEqual(1, (int)ComponentState.Initializing);
        Assert.AreEqual(2, (int)ComponentState.Initialized);
        Assert.AreEqual(3, (int)ComponentState.Activating);
        Assert.AreEqual(4, (int)ComponentState.Active);
        Assert.AreEqual(5, (int)ComponentState.Deactivating);
        Assert.AreEqual(6, (int)ComponentState.Deactivated);
        Assert.AreEqual(7, (int)ComponentState.Disposing);
        Assert.AreEqual(8, (int)ComponentState.Disposed);
#pragma warning restore MSTEST0032
    }
}
