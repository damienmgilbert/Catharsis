using System.ComponentModel;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class ComponentContainerSlimTests
{
    #region Public methods
    [TestMethod]
    public void Add_AfterDispose_Throws()
    {
        ComponentContainerSlim container = new ComponentContainerSlim();
        container.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => container.Add(new TestComponent()));
    }

    [TestMethod]
    public void Add_Component_IncreasesCount()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();
        TestComponent component = new TestComponent();

        container.Add(component);

        Assert.AreEqual(1, container.Count);
    }

    [TestMethod]
    public void Add_DuplicateName_Throws()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();
        container.Add(new TestComponent(), "name");

        Assert.ThrowsExactly<ArgumentException>(() => container.Add(new TestComponent(), "name"));
    }

    [TestMethod]
    public void Add_NullComponent_DoesNothing()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();

        container.Add(null);

        Assert.AreEqual(0, container.Count);
    }

    [TestMethod]
    public void Add_SetsComponentSite()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();
        TestComponent component = new TestComponent();

        container.Add(component);

        Assert.IsNotNull(component.Site);
        Assert.AreSame(container, component.Site.Container);
    }

    [TestMethod]
    public void Add_WithName_AssignsNameToSite()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();
        TestComponent component = new TestComponent();

        container.Add(component, "myComponent");

        Assert.IsNotNull(component.Site);
        Assert.AreEqual("myComponent", component.Site.Name);
    }

    [TestMethod]
    public void Components_ReturnsAllComponents()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();
        TestComponent c1 = new TestComponent();
        TestComponent c2 = new TestComponent();
        container.Add(c1, "first");
        container.Add(c2, "second");

        ComponentCollection components = container.Components;

        Assert.HasCount(2, components);
    }

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        ComponentContainerSlim container = new ComponentContainerSlim();
        container.Add(new TestComponent());

        container.Dispose();
        container.Dispose(); // should not throw
    }

    [TestMethod]
    public void Dispose_DisposesAllComponentsInReverseOrder()
    {
        ComponentContainerSlim container = new ComponentContainerSlim();
        TestComponent c1 = new TestComponent();
        TestComponent c2 = new TestComponent();
        container.Add(c1);
        container.Add(c2);

        container.Dispose();

        Assert.IsTrue(c1.WasDisposed);
        Assert.IsTrue(c2.WasDisposed);
    }

    [TestMethod]
    public void GetComponent_ByName_ReturnsComponent()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();
        TestComponent component = new TestComponent();
        container.Add(component, "test");

        IComponent? found = container.GetComponent("test");

        Assert.AreSame(component, found);
    }

    [TestMethod]
    public void GetComponent_NonExisting_ReturnsNull()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();

        IComponent? found = container.GetComponent("nope");

        Assert.IsNull(found);
    }

    [TestMethod]
    public void Remove_ExistingComponent_RemovesIt()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();
        TestComponent component = new TestComponent();
        container.Add(component);

        container.Remove(component);

        Assert.AreEqual(0, container.Count);
        Assert.IsNull(component.Site);
    }

    [TestMethod]
    public void Remove_NonExistingComponent_DoesNothing()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();
        container.Add(new TestComponent());

        container.Remove(new TestComponent());

        Assert.AreEqual(1, container.Count);
    }

    [TestMethod]
    public void Remove_NullComponent_DoesNothing()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();
        container.Add(new TestComponent());

        container.Remove(null);

        Assert.AreEqual(1, container.Count);
    }

    [TestMethod]
    public void Site_GetService_ReturnsContainerForIContainer()
    {
        using ComponentContainerSlim container = new ComponentContainerSlim();
        TestComponent component = new TestComponent();
        container.Add(component);

        object? service = component.Site!.GetService(typeof(IContainer));

        Assert.AreSame(container, service);
    }
    #endregion

    sealed class TestComponent : Component
    {
        #region Protected methods
        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
        #endregion

        #region Public properties
        public bool WasDisposed { get; private set; }
        #endregion
    }
}

