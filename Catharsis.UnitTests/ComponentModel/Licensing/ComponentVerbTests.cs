using Catharsis.ComponentModel.Licensing;

namespace Catharsis.UnitTests.ComponentModel.Licensing;

[TestClass]
public sealed class ComponentVerbTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_Defaults()
    {
        ComponentVerb verb = new ComponentVerb(
                             "Do",
                             () =>
        {
        });

        Assert.IsNull(verb.Description);
        Assert.IsTrue(verb.Enabled);
    }

    [TestMethod]
    public void Constructor_SetsProperties()
    {
        bool invoked = false;
        ComponentVerb verb = new ComponentVerb("Reset", () => invoked = true, "Resets state", Enabled: true);

        Assert.AreEqual("Reset", verb.Text);
        Assert.AreEqual("Resets state", verb.Description);
        Assert.IsTrue(verb.Enabled);
    }

    [TestMethod]
    public void Equality_SameValues_AreEqual()
    {
        Action action = () =>
        {
        };
        ComponentVerb a = new ComponentVerb("Do", action, "desc", true);
        ComponentVerb b = new ComponentVerb("Do", action, "desc", true);

        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void Invoke_Disabled_ThrowsInvalidOperationException()
    {
        ComponentVerb verb = new ComponentVerb(
                             "Do",
                             () =>
        {
        },
                             Enabled: false);

        Assert.ThrowsExactly<InvalidOperationException>(() => verb.Invoke());
    }

    [TestMethod]
    public void Invoke_Enabled_ExecutesAction()
    {
        bool invoked = false;
        ComponentVerb verb = new ComponentVerb("Do", () => invoked = true);

        verb.Invoke();

        Assert.IsTrue(invoked);
    }
    #endregion
}
