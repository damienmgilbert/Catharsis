using Catharsis.ComponentModel.Licensing;

namespace Catharsis.UnitTests.ComponentModel.Licensing;

[TestClass]
public sealed class ComponentVerbTests
{
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var invoked = false;
        var verb = new ComponentVerb("Reset", () => invoked = true, "Resets state", Enabled: true);

        Assert.AreEqual("Reset", verb.Text);
        Assert.AreEqual("Resets state", verb.Description);
        Assert.IsTrue(verb.Enabled);
    }

    [TestMethod]
    public void Constructor_Defaults()
    {
        var verb = new ComponentVerb("Do", () => { });

        Assert.IsNull(verb.Description);
        Assert.IsTrue(verb.Enabled);
    }

    [TestMethod]
    public void Invoke_Enabled_ExecutesAction()
    {
        var invoked = false;
        var verb = new ComponentVerb("Do", () => invoked = true);

        verb.Invoke();

        Assert.IsTrue(invoked);
    }

    [TestMethod]
    public void Invoke_Disabled_ThrowsInvalidOperationException()
    {
        var verb = new ComponentVerb("Do", () => { }, Enabled: false);

        Assert.ThrowsExactly<InvalidOperationException>(() => verb.Invoke());
    }

    [TestMethod]
    public void Equality_SameValues_AreEqual()
    {
        Action action = () => { };
        var a = new ComponentVerb("Do", action, "desc", true);
        var b = new ComponentVerb("Do", action, "desc", true);

        Assert.AreEqual(a, b);
    }
}
