using System.ComponentModel;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class ComponentContainerTests
{
    #region Public methods
    [TestMethod]
    public void Add_Component_IncreasesCount()
    {
        using ComponentContainer container = new ComponentContainer();
        container.Add(new TestComponent());
        Assert.AreEqual(1, container.Count);
    }

    [TestMethod]
    public void Add_DuplicateName_Throws()
    {
        using ComponentContainer container = new ComponentContainer();
        container.Add(new TestComponent(), "name");
        Assert.ThrowsExactly<ArgumentException>(() => container.Add(new TestComponent(), "name"));
    }

    [TestMethod]
    public void Add_Null_IsIgnored()
    {
        using ComponentContainer container = new ComponentContainer();
        container.Add(null);
        Assert.AreEqual(0, container.Count);
    }

    [TestMethod]
    public void Add_WithName_SitesComponent()
    {
        using ComponentContainer container = new ComponentContainer();
        TestComponent comp = new TestComponent();
        container.Add(comp, "test");
        Assert.IsNotNull(comp.Site);
        Assert.AreEqual("test", comp.Site.Name);
    }

    [TestMethod]
    public void Components_ReturnsAllComponents()
    {
        using ComponentContainer container = new ComponentContainer();
        container.Add(new TestComponent());
        container.Add(new TestComponent());
        Assert.HasCount(2, container.Components);
    }

    [TestMethod]
    public void GetComponent_ByName_ReturnsComponent()
    {
        using ComponentContainer container = new ComponentContainer();
        TestComponent comp = new TestComponent();
        container.Add(comp, "myComp");
        Assert.AreSame(comp, container.GetComponent("myComp"));
    }

    [TestMethod]
    public void Remove_Component_DecreasesCount()
    {
        using ComponentContainer container = new ComponentContainer();
        TestComponent comp = new TestComponent();
        container.Add(comp);
        container.Remove(comp);
        Assert.AreEqual(0, container.Count);
        Assert.IsNull(comp.Site);
    }
    #endregion

    sealed class TestComponent : Component
    {
    }
}
