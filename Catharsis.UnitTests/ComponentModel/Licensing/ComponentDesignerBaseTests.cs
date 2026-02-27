using System.ComponentModel;

using Catharsis.ComponentModel.Licensing;

namespace Catharsis.UnitTests.ComponentModel.Licensing;

[TestClass]
public sealed class ComponentDesignerBaseTests
{
    [TestMethod]
    public void Initialize_NullComponent_ThrowsArgumentNullException()
    {
        using var designer = new TestDesigner();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => designer.Initialize(null!));
    }

    [TestMethod]
    public void Initialize_ValidComponent_SetsComponent()
    {
        using var designer = new TestDesigner();
        var component = new StubComponent();

        designer.Initialize(component);

        Assert.AreSame(component, designer.Component);
    }

    [TestMethod]
    public void Initialize_CalledTwice_ThrowsInvalidOperationException()
    {
        using var designer = new TestDesigner();
        designer.Initialize(new StubComponent());

        Assert.ThrowsExactly<InvalidOperationException>(
            () => designer.Initialize(new StubComponent()));
    }

    [TestMethod]
    public void Initialize_AfterDispose_ThrowsObjectDisposedException()
    {
        var designer = new TestDesigner();
        designer.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => designer.Initialize(new StubComponent()));
    }

    [TestMethod]
    public void Initialize_CallsOnInitialize()
    {
        using var designer = new TestDesigner();

        designer.Initialize(new StubComponent());

        Assert.IsTrue(designer.OnInitializeCalled);
    }

    [TestMethod]
    public void Initialize_WithContainer_PassesContainerToContext()
    {
        using var designer = new TestDesigner();
        var container = new StubContainer();

        designer.Initialize(new StubComponent(), container);

        Assert.AreSame(container, designer.ExposedContext!.Container);
    }

    [TestMethod]
    public void Component_BeforeInitialize_ReturnsNull()
    {
        using var designer = new TestDesigner();

        Assert.IsNull(designer.Component);
    }

    [TestMethod]
    public void ActionList_DefaultCreateActionList_ReturnsNull()
    {
        using var designer = new TestDesigner();
        designer.Initialize(new StubComponent());

        Assert.IsNull(designer.ActionList);
    }

    [TestMethod]
    public void ActionList_OverriddenCreateActionList_ReturnsActionList()
    {
        using var designer = new DesignerWithActions();
        designer.Initialize(new StubComponent());

        Assert.IsNotNull(designer.ActionList);
    }

    [TestMethod]
    public void NotifyComponentChanged_NullPropertyName_ThrowsArgumentNullException()
    {
        using var designer = new TestDesigner();
        designer.Initialize(new StubComponent());

        Assert.ThrowsExactly<ArgumentNullException>(
            () => designer.NotifyComponentChanged(null!));
    }

    [TestMethod]
    public void NotifyComponentChanged_CallsOnComponentChanged()
    {
        using var designer = new TestDesigner();
        designer.Initialize(new StubComponent());

        designer.NotifyComponentChanged("Name");

        Assert.AreEqual("Name", designer.LastChangedProperty);
    }

    [TestMethod]
    public void Dispose_ClearsContextAndActionList()
    {
        var designer = new DesignerWithActions();
        designer.Initialize(new StubComponent());
        Assert.IsNotNull(designer.ActionList);

        designer.Dispose();

        Assert.IsNull(designer.Component);
        Assert.IsNull(designer.ActionList);
    }

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var designer = new TestDesigner();
        designer.Initialize(new StubComponent());

        designer.Dispose();
        designer.Dispose();
    }

    private class TestDesigner : ComponentDesignerBase
    {
        public bool OnInitializeCalled { get; private set; }
        public string? LastChangedProperty { get; private set; }
        public ComponentDesignContext? ExposedContext => Context;

        protected override void OnInitialize(ComponentDesignContext context) =>
            OnInitializeCalled = true;

        protected override void OnComponentChanged(string propertyName) =>
            LastChangedProperty = propertyName;
    }

    private sealed class DesignerWithActions : TestDesigner
    {
        protected override ComponentActionList? CreateActionList(ComponentDesignContext context) =>
            new TestActionList(context);

        private sealed class TestActionList(ComponentDesignContext context)
            : ComponentActionList(context);
    }

    private sealed class StubComponent : IComponent
    {
        public ISite? Site { get; set; }
        public event EventHandler? Disposed;
        public void Dispose() => Disposed?.Invoke(this, EventArgs.Empty);
    }

    private sealed class StubContainer : IContainer
    {
        public ComponentCollection Components => new([]);
        public void Add(IComponent? component) { }
        public void Add(IComponent? component, string? name) { }
        public void Remove(IComponent? component) { }
        public void Dispose() { }
    }
}
