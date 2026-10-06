using System.ComponentModel;
using Catharsis.ComponentModel.Lifecycle;

namespace Catharsis.UnitTests.ComponentModel.Lifecycle;

///<summary>
///Unit tests for the <see cref="ComponentLifecycleManager"/> class.
///</summary>
[TestClass]
public sealed class ComponentLifecycleManagerTests
{
    #region Private methods
    static ComponentGraph BuildGraph(params IComponent[] components)
    {
        ComponentGraphBuilder builder = new();
        foreach (IComponent c in components)
        {
            builder.AddComponent(c);
        }

        return builder.Build();
    }
    #endregion

    #region Public methods
    [TestMethod]
    public void Activate_SingleComponent_WithSatisfiedDependencies()
    {
        StubComponent db = new();
        StubComponent app = new();
        ComponentGraph graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();
        using ComponentLifecycleManager manager = new(graph);
        manager.Activate(db);

        manager.Activate(app);

        Assert.AreEqual(ComponentState.Active, graph.GetNode(app)!.State);
    }

    [TestMethod]
    public void Activate_UnsatisfiedDependencies_ThrowsInvalidOperationException()
    {
        StubComponent db = new();
        StubComponent app = new();
        ComponentGraph graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();
        using ComponentLifecycleManager manager = new(graph);

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.Activate(app));
    }

    [TestMethod]
    public void ActivateAll_InitializesFirst()
    {
        StubComponent c = new();
        ComponentGraph graph = BuildGraph(c);
        using ComponentLifecycleManager manager = new(graph);

        manager.ActivateAll();

        ComponentStateMachine? machine = manager.GetStateMachine(c);
        Assert.IsNotNull(machine);
        Assert.AreEqual(ComponentState.Active, machine.CurrentState);
    }

    [TestMethod]
    public void ActivateAll_SetsNodesActive()
    {
        StubComponent c = new();
        ComponentGraph graph = BuildGraph(c);
        using ComponentLifecycleManager manager = new(graph);

        manager.ActivateAll();

        ComponentGraphNode? node = graph.GetNode(c);
        Assert.IsNotNull(node);
        Assert.AreEqual(ComponentState.Active, node.State);
    }

    [TestMethod]
    public void Constructor_NullGraph_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ComponentLifecycleManager(null!)); }
    [TestMethod]
    public void Deactivate_AlsoDeactivatesDependents()
    {
        StubComponent db = new();
        StubComponent app = new();
        ComponentGraph graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();
        using ComponentLifecycleManager manager = new(graph);
        manager.ActivateAll();

        manager.Deactivate(db);

        Assert.AreEqual(ComponentState.Deactivated, graph.GetNode(app)!.State);
        Assert.AreEqual(ComponentState.Deactivated, graph.GetNode(db)!.State);
    }

    [TestMethod]
    public void Deactivate_SingleComponent()
    {
        StubComponent c = new();
        ComponentGraph graph = BuildGraph(c);
        using ComponentLifecycleManager manager = new(graph);
        manager.ActivateAll();

        manager.Deactivate(c);

        Assert.AreEqual(ComponentState.Deactivated, graph.GetNode(c)!.State);
    }

    [TestMethod]
    public void DeactivateAll_SetsNodesDeactivated()
    {
        StubComponent c = new();
        ComponentGraph graph = BuildGraph(c);
        using ComponentLifecycleManager manager = new(graph);
        manager.ActivateAll();

        manager.DeactivateAll();

        ComponentGraphNode? node = graph.GetNode(c);
        Assert.IsNotNull(node);
        Assert.AreEqual(ComponentState.Deactivated, node.State);
    }

    [TestMethod]
    public void DependencyOrder_ActivationRespectsDependencies()
    {
        List<string> activationOrder = [];
        TrackingComponent db = new("DB", activationOrder);
        TrackingComponent cache = new("Cache", activationOrder);
        TrackingComponent app = new("App", activationOrder);

        ComponentGraph graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(cache, "Cache")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .AddDependency(app, cache)
            .AddDependency(cache, db)
            .Build();

        using ComponentLifecycleManager manager = new(graph);
        manager.ActivateAll();

        // DB must be activated before Cache, and both before App
        int dbIdx = activationOrder.IndexOf("DB");
        int cacheIdx = activationOrder.IndexOf("Cache");
        int appIdx = activationOrder.IndexOf("App");
        Assert.IsLessThan(cacheIdx, dbIdx);
        Assert.IsLessThan(appIdx, cacheIdx);
    }

    [TestMethod]
    public void Dispose_DisposesComponents()
    {
        DisposableComponent c = new();
        ComponentGraph graph = BuildGraph(c);
        ComponentLifecycleManager manager = new(graph);
        manager.ActivateAll();

        manager.Dispose();

        Assert.IsTrue(c.IsDisposed);
    }

    [TestMethod]
    public void Dispose_SetsNodesDisposed()
    {
        StubComponent c = new();
        ComponentGraph graph = BuildGraph(c);
        ComponentLifecycleManager manager = new(graph);
        manager.ActivateAll();

        manager.Dispose();

        Assert.AreEqual(ComponentState.Disposed, graph.GetNode(c)!.State);
    }

    [TestMethod]
    public void GetStateMachine_RegisteredComponent_ReturnsMachine()
    {
        StubComponent c = new();
        ComponentGraph graph = BuildGraph(c);
        using ComponentLifecycleManager manager = new(graph);

        ComponentStateMachine? machine = manager.GetStateMachine(c);

        Assert.IsNotNull(machine);
        Assert.AreEqual(ComponentState.Created, machine.CurrentState);
    }

    [TestMethod]
    public void GetStateMachine_UnregisteredComponent_ReturnsNull()
    {
        ComponentGraph graph = BuildGraph(new StubComponent());
        using ComponentLifecycleManager manager = new(graph);

        Assert.IsNull(manager.GetStateMachine(new StubComponent()));
    }

    [TestMethod]
    public void Graph_ReturnsSameGraph()
    {
        ComponentGraph graph = BuildGraph(new StubComponent());
        using ComponentLifecycleManager manager = new(graph);

        Assert.AreSame(graph, manager.Graph);
    }

    [TestMethod]
    public void Initialize_NullComponent_ThrowsArgumentNullException()
    {
        ComponentGraph graph = BuildGraph(new StubComponent());
        using ComponentLifecycleManager manager = new(graph);

        Assert.ThrowsExactly<ArgumentNullException>(() => manager.Initialize(null!));
    }

    [TestMethod]
    public void Initialize_SingleComponent()
    {
        StubComponent c = new();
        ComponentGraph graph = BuildGraph(c);
        using ComponentLifecycleManager manager = new(graph);

        manager.Initialize(c);

        ComponentGraphNode? node = graph.GetNode(c);
        Assert.IsNotNull(node);
        Assert.AreEqual(ComponentState.Initialized, node.State);
    }

    [TestMethod]
    public void InitializeAll_CallsISupportInitialize()
    {
        InitializableComponent c = new();
        ComponentGraph graph = BuildGraph(c);
        using ComponentLifecycleManager manager = new(graph);

        manager.InitializeAll();

        Assert.IsTrue(c.BeginInitCalled);
        Assert.IsTrue(c.EndInitCalled);
    }

    [TestMethod]
    public void InitializeAll_SetsNodesInitialized()
    {
        StubComponent c = new();
        ComponentGraph graph = BuildGraph(c);
        using ComponentLifecycleManager manager = new(graph);

        manager.InitializeAll();

        ComponentGraphNode? node = graph.GetNode(c);
        Assert.IsNotNull(node);
        Assert.AreEqual(ComponentState.Initialized, node.State);
    }
    #endregion

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

    sealed class InitializableComponent : IComponent, ISupportInitialize
    {
        #region Events
        public event EventHandler? Disposed;
        #endregion

        #region Public methods
        public void BeginInit() { BeginInitCalled = true; }
        public void Dispose() { Disposed?.Invoke(this, EventArgs.Empty); }
        public void EndInit() { EndInitCalled = true; }
        #endregion

        #region Public properties
        public bool BeginInitCalled { get; private set; }

        public bool EndInitCalled { get; private set; }

        public ISite? Site { get; set; }
        #endregion
    }

    sealed class DisposableComponent : IComponent, IDisposable
    {
        #region Events
        public event EventHandler? Disposed;
        #endregion

        #region Public methods
        public void Dispose()
        {
            IsDisposed = true;
            Disposed?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Public properties
        public bool IsDisposed { get; private set; }

        public ISite? Site { get; set; }
        #endregion
    }

    sealed class TrackingComponent(string name, List<string> activationOrder) : IComponent, ISupportInitialize
    {
        #region Events
        public event EventHandler? Disposed;
        #endregion

        #region Public methods
        public void BeginInit()
        {
        }
        public void Dispose() { Disposed?.Invoke(this, EventArgs.Empty); }
        public void EndInit() { activationOrder.Add(name); }
        #endregion

        #region Public properties
        public ISite? Site { get; set; }
        #endregion
    }
}
