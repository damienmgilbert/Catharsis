using System.ComponentModel;
using Catharsis.ComponentModel.Licensing;

namespace Catharsis.UnitTests.ComponentModel.Licensing;

///<summary>
///Unit tests for the <see cref="ComponentDesignerBase"/> class.
///</summary>
[TestClass]
public sealed class ComponentDesignerBaseTests
{
    #region Public methods
    [TestMethod]
    public void ActionList_DefaultCreateActionList_ReturnsNull()
    {
        using TestDesigner designer = new();
        designer.Initialize(new StubComponent());

        Assert.IsNull(designer.ActionList);
    }

    [TestMethod]
    public void ActionList_OverriddenCreateActionList_ReturnsActionList()
    {
        using DesignerWithActions designer = new();
        designer.Initialize(new StubComponent());

        Assert.IsNotNull(designer.ActionList);
    }

    [TestMethod]
    public void Component_BeforeInitialize_ReturnsNull()
    {
        using TestDesigner designer = new();

        Assert.IsNull(designer.Component);
    }

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        TestDesigner designer = new();
        designer.Initialize(new StubComponent());

        designer.Dispose();
        designer.Dispose();
    }

    [TestMethod]
    public void Dispose_ClearsContextAndActionList()
    {
        DesignerWithActions designer = new();
        designer.Initialize(new StubComponent());
        Assert.IsNotNull(designer.ActionList);

        designer.Dispose();

        Assert.IsNull(designer.Component);
        Assert.IsNull(designer.ActionList);
    }

    [TestMethod]
    public void Initialize_AfterDispose_ThrowsObjectDisposedException()
    {
        TestDesigner designer = new();
        designer.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => designer.Initialize(new StubComponent()));
    }

    [TestMethod]
    public void Initialize_CalledTwice_ThrowsInvalidOperationException()
    {
        using TestDesigner designer = new();
        designer.Initialize(new StubComponent());

        Assert.ThrowsExactly<InvalidOperationException>(() => designer.Initialize(new StubComponent()));
    }

    [TestMethod]
    public void Initialize_CallsOnInitialize()
    {
        using TestDesigner designer = new();

        designer.Initialize(new StubComponent());

        Assert.IsTrue(designer.OnInitializeCalled);
    }

    [TestMethod]
    public void Initialize_NullComponent_ThrowsArgumentNullException()
    {
        using TestDesigner designer = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => designer.Initialize(null!));
    }

    [TestMethod]
    public void Initialize_ValidComponent_SetsComponent()
    {
        using TestDesigner designer = new();
        StubComponent component = new();

        designer.Initialize(component);

        Assert.AreSame(component, designer.Component);
    }

    [TestMethod]
    public void Initialize_WithContainer_PassesContainerToContext()
    {
        using TestDesigner designer = new();
        StubContainer container = new();

        designer.Initialize(new StubComponent(), container);

        Assert.AreSame(container, designer.ExposedContext!.Container);
    }

    [TestMethod]
    public void NotifyComponentChanged_CallsOnComponentChanged()
    {
        using TestDesigner designer = new();
        designer.Initialize(new StubComponent());

        designer.NotifyComponentChanged("Name");

        Assert.AreEqual("Name", designer.LastChangedProperty);
    }

    [TestMethod]
    public void NotifyComponentChanged_NullPropertyName_ThrowsArgumentNullException()
    {
        using TestDesigner designer = new();
        designer.Initialize(new StubComponent());

        Assert.ThrowsExactly<ArgumentNullException>(() => designer.NotifyComponentChanged(null!));
    }
    #endregion

    class TestDesigner : ComponentDesignerBase
    {
        #region Protected methods
        protected override void OnComponentChanged(string propertyName) { LastChangedProperty = propertyName; }
        protected override void OnInitialize(ComponentDesignContext context) { OnInitializeCalled = true; }
        #endregion

        #region Public properties
        public ComponentDesignContext? ExposedContext => Context;

        public string? LastChangedProperty { get; private set; }

        public bool OnInitializeCalled { get; private set; }
        #endregion
    }

    sealed class DesignerWithActions : TestDesigner
    {
        #region Protected methods
        protected override ComponentActionList? CreateActionList(ComponentDesignContext context) { return new TestActionList(context); }
        #endregion

        sealed class TestActionList(ComponentDesignContext context) : ComponentActionList(context);
    }

    sealed class StubComponent : IComponent
    {
        #region Events
        public event EventHandler? Disposed;
        #endregion

        #region Public methods
        public void Dispose() { Disposed?.Invoke(this, EventArgs.Empty); }
        #endregion

        #region Public properties
        public ISite? Site { get; set; }
        #endregion
    }

    sealed class StubContainer : IContainer
    {
        #region Public methods
        public void Add(IComponent? component)
        {
        }
        public void Add(IComponent? component, string? name)
        {
        }
        public void Dispose()
        {
        }
        public void Remove(IComponent? component)
        {
        }
        #endregion

        #region Public properties
        public ComponentCollection Components => new([]);
        #endregion
    }
}
