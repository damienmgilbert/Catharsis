using Catharsis.ComponentModel;
using System.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class ComponentContainerSlimTests
{
    private sealed class TestComponent : Component
    {
        public bool WasDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }

    [TestMethod]
    public void Add_Component_IncreasesCount()
    {
        using var container = new ComponentContainerSlim();
        var component = new TestComponent();

        container.Add(component);

        Assert.AreEqual(1, container.Count);
    }

    [TestMethod]
    public void Add_WithName_AssignsNameToSite()
    {
        using var container = new ComponentContainerSlim();
        var component = new TestComponent();

        container.Add(component, "myComponent");

        Assert.IsNotNull(component.Site);
        Assert.AreEqual("myComponent", component.Site.Name);
    }

    [TestMethod]
    public void Add_SetsComponentSite()
    {
        using var container = new ComponentContainerSlim();
        var component = new TestComponent();

        container.Add(component);

        Assert.IsNotNull(component.Site);
        Assert.AreSame(container, component.Site.Container);
    }

    [TestMethod]
    public void Add_NullComponent_DoesNothing()
    {
        using var container = new ComponentContainerSlim();

        container.Add(null);

        Assert.AreEqual(0, container.Count);
    }

    [TestMethod]
    public void Add_DuplicateName_Throws()
    {
        using var container = new ComponentContainerSlim();
        container.Add(new TestComponent(), "name");

        Assert.ThrowsExactly<ArgumentException>(() => container.Add(new TestComponent(), "name"));
    }

    [TestMethod]
    public void Remove_ExistingComponent_RemovesIt()
    {
        using var container = new ComponentContainerSlim();
        var component = new TestComponent();
        container.Add(component);

        container.Remove(component);

        Assert.AreEqual(0, container.Count);
        Assert.IsNull(component.Site);
    }

    [TestMethod]
    public void Remove_NullComponent_DoesNothing()
    {
        using var container = new ComponentContainerSlim();
        container.Add(new TestComponent());

        container.Remove(null);

        Assert.AreEqual(1, container.Count);
    }

    [TestMethod]
    public void Remove_NonExistingComponent_DoesNothing()
    {
        using var container = new ComponentContainerSlim();
        container.Add(new TestComponent());

        container.Remove(new TestComponent());

        Assert.AreEqual(1, container.Count);
    }

    [TestMethod]
    public void GetComponent_ByName_ReturnsComponent()
    {
        using var container = new ComponentContainerSlim();
        var component = new TestComponent();
        container.Add(component, "test");

        var found = container.GetComponent("test");

        Assert.AreSame(component, found);
    }

    [TestMethod]
    public void GetComponent_NonExisting_ReturnsNull()
    {
        using var container = new ComponentContainerSlim();

        var found = container.GetComponent("nope");

        Assert.IsNull(found);
    }

    [TestMethod]
    public void Components_ReturnsAllComponents()
    {
        using var container = new ComponentContainerSlim();
        var c1 = new TestComponent();
        var c2 = new TestComponent();
        container.Add(c1, "first");
        container.Add(c2, "second");

        var components = container.Components;

        Assert.AreEqual(2, components.Count);
    }

    [TestMethod]
    public void Dispose_DisposesAllComponentsInReverseOrder()
    {
        var container = new ComponentContainerSlim();
        var c1 = new TestComponent();
        var c2 = new TestComponent();
        container.Add(c1);
        container.Add(c2);

        container.Dispose();

        Assert.IsTrue(c1.WasDisposed);
        Assert.IsTrue(c2.WasDisposed);
    }

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var container = new ComponentContainerSlim();
        container.Add(new TestComponent());

        container.Dispose();
        container.Dispose(); // should not throw
    }

    [TestMethod]
    public void Add_AfterDispose_Throws()
    {
        var container = new ComponentContainerSlim();
        container.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => container.Add(new TestComponent()));
    }

    [TestMethod]
    public void Site_GetService_ReturnsContainerForIContainer()
    {
        using var container = new ComponentContainerSlim();
        var component = new TestComponent();
        container.Add(component);

        var service = component.Site!.GetService(typeof(IContainer));

        Assert.AreSame(container, service);
    }
}
