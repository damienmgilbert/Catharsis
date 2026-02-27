using System.ComponentModel;

using Catharsis.ComponentModel.Lifecycle;

namespace Catharsis.UnitTests.ComponentModel.Lifecycle;

[TestClass]
public sealed class ComponentLifecycleManagerTests
{
    [TestMethod]
    public void Constructor_NullGraph_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new ComponentLifecycleManager(null!));
    }

    [TestMethod]
    public void GetStateMachine_RegisteredComponent_ReturnsMachine()
    {
        var c = new StubComponent();
        var graph = BuildGraph(c);
        using var manager = new ComponentLifecycleManager(graph);

        var machine = manager.GetStateMachine(c);

        Assert.IsNotNull(machine);
        Assert.AreEqual(ComponentState.Created, machine.CurrentState);
    }

    [TestMethod]
    public void GetStateMachine_UnregisteredComponent_ReturnsNull()
    {
        var graph = BuildGraph(new StubComponent());
        using var manager = new ComponentLifecycleManager(graph);

        Assert.IsNull(manager.GetStateMachine(new StubComponent()));
    }

    [TestMethod]
    public void Graph_ReturnsSameGraph()
    {
        var graph = BuildGraph(new StubComponent());
        using var manager = new ComponentLifecycleManager(graph);

        Assert.AreSame(graph, manager.Graph);
    }

    [TestMethod]
    public void InitializeAll_SetsNodesInitialized()
    {
        var c = new StubComponent();
        var graph = BuildGraph(c);
        using var manager = new ComponentLifecycleManager(graph);

        manager.InitializeAll();

        var node = graph.GetNode(c);
        Assert.IsNotNull(node);
        Assert.AreEqual(ComponentState.Initialized, node.State);
    }

    [TestMethod]
    public void InitializeAll_CallsISupportInitialize()
    {
        var c = new InitializableComponent();
        var graph = BuildGraph(c);
        using var manager = new ComponentLifecycleManager(graph);

        manager.InitializeAll();

        Assert.IsTrue(c.BeginInitCalled);
        Assert.IsTrue(c.EndInitCalled);
    }

    [TestMethod]
    public void ActivateAll_SetsNodesActive()
    {
        var c = new StubComponent();
        var graph = BuildGraph(c);
        using var manager = new ComponentLifecycleManager(graph);

        manager.ActivateAll();

        var node = graph.GetNode(c);
        Assert.IsNotNull(node);
        Assert.AreEqual(ComponentState.Active, node.State);
    }

    [TestMethod]
    public void ActivateAll_InitializesFirst()
    {
        var c = new StubComponent();
        var graph = BuildGraph(c);
        using var manager = new ComponentLifecycleManager(graph);

        manager.ActivateAll();

        var machine = manager.GetStateMachine(c);
        Assert.IsNotNull(machine);
        Assert.AreEqual(ComponentState.Active, machine.CurrentState);
    }

    [TestMethod]
    public void DeactivateAll_SetsNodesDeactivated()
    {
        var c = new StubComponent();
        var graph = BuildGraph(c);
        using var manager = new ComponentLifecycleManager(graph);
        manager.ActivateAll();

        manager.DeactivateAll();

        var node = graph.GetNode(c);
        Assert.IsNotNull(node);
        Assert.AreEqual(ComponentState.Deactivated, node.State);
    }

    [TestMethod]
    public void Initialize_SingleComponent()
    {
        var c = new StubComponent();
        var graph = BuildGraph(c);
        using var manager = new ComponentLifecycleManager(graph);

        manager.Initialize(c);

        var node = graph.GetNode(c);
        Assert.IsNotNull(node);
        Assert.AreEqual(ComponentState.Initialized, node.State);
    }

    [TestMethod]
    public void Initialize_NullComponent_ThrowsArgumentNullException()
    {
        var graph = BuildGraph(new StubComponent());
        using var manager = new ComponentLifecycleManager(graph);

        Assert.ThrowsExactly<ArgumentNullException>(() => manager.Initialize(null!));
    }

    [TestMethod]
    public void Activate_SingleComponent_WithSatisfiedDependencies()
    {
        var db = new StubComponent();
        var app = new StubComponent();
        var graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();
        using var manager = new ComponentLifecycleManager(graph);
        manager.Activate(db);

        manager.Activate(app);

        Assert.AreEqual(ComponentState.Active, graph.GetNode(app)!.State);
    }

    [TestMethod]
    public void Activate_UnsatisfiedDependencies_ThrowsInvalidOperationException()
    {
        var db = new StubComponent();
        var app = new StubComponent();
        var graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();
        using var manager = new ComponentLifecycleManager(graph);

        Assert.ThrowsExactly<InvalidOperationException>(() => manager.Activate(app));
    }

    [TestMethod]
    public void Deactivate_SingleComponent()
    {
        var c = new StubComponent();
        var graph = BuildGraph(c);
        using var manager = new ComponentLifecycleManager(graph);
        manager.ActivateAll();

        manager.Deactivate(c);

        Assert.AreEqual(ComponentState.Deactivated, graph.GetNode(c)!.State);
    }

    [TestMethod]
    public void Deactivate_AlsoDeactivatesDependents()
    {
        var db = new StubComponent();
        var app = new StubComponent();
        var graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();
        using var manager = new ComponentLifecycleManager(graph);
        manager.ActivateAll();

        manager.Deactivate(db);

        Assert.AreEqual(ComponentState.Deactivated, graph.GetNode(app)!.State);
        Assert.AreEqual(ComponentState.Deactivated, graph.GetNode(db)!.State);
    }

    [TestMethod]
    public void Dispose_DisposesComponents()
    {
        var c = new DisposableComponent();
        var graph = BuildGraph(c);
        var manager = new ComponentLifecycleManager(graph);
        manager.ActivateAll();

        manager.Dispose();

        Assert.IsTrue(c.IsDisposed);
    }

    [TestMethod]
    public void Dispose_SetsNodesDisposed()
    {
        var c = new StubComponent();
        var graph = BuildGraph(c);
        var manager = new ComponentLifecycleManager(graph);
        manager.ActivateAll();

        manager.Dispose();

        Assert.AreEqual(ComponentState.Disposed, graph.GetNode(c)!.State);
    }

    [TestMethod]
    public void DependencyOrder_ActivationRespectsDependencies()
    {
        var activationOrder = new List<string>();
        var db = new TrackingComponent("DB", activationOrder);
        var cache = new TrackingComponent("Cache", activationOrder);
        var app = new TrackingComponent("App", activationOrder);

        var graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(cache, "Cache")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .AddDependency(app, cache)
            .AddDependency(cache, db)
            .Build();

        using var manager = new ComponentLifecycleManager(graph);
        manager.ActivateAll();

        // DB must be activated before Cache, and both before App
        var dbIdx = activationOrder.IndexOf("DB");
        var cacheIdx = activationOrder.IndexOf("Cache");
        var appIdx = activationOrder.IndexOf("App");
        Assert.IsTrue(dbIdx < cacheIdx);
        Assert.IsTrue(cacheIdx < appIdx);
    }

    private static ComponentGraph BuildGraph(params IComponent[] components)
    {
        var builder = new ComponentGraphBuilder();
        foreach (var c in components)
            builder.AddComponent(c);
        return builder.Build();
    }

    private sealed class StubComponent : IComponent
    {
        public ISite? Site { get; set; }
        public event EventHandler? Disposed;
        public void Dispose() => Disposed?.Invoke(this, EventArgs.Empty);
    }

    private sealed class InitializableComponent : IComponent, ISupportInitialize
    {
        public ISite? Site { get; set; }
        public bool BeginInitCalled { get; private set; }
        public bool EndInitCalled { get; private set; }
        public event EventHandler? Disposed;

        public void BeginInit() => BeginInitCalled = true;
        public void EndInit() => EndInitCalled = true;
        public void Dispose() => Disposed?.Invoke(this, EventArgs.Empty);
    }

    private sealed class DisposableComponent : IComponent, IDisposable
    {
        public ISite? Site { get; set; }
        public bool IsDisposed { get; private set; }
        public event EventHandler? Disposed;

        public void Dispose()
        {
            IsDisposed = true;
            Disposed?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class TrackingComponent(string name, List<string> activationOrder) : IComponent, ISupportInitialize
    {
        public ISite? Site { get; set; }
        public event EventHandler? Disposed;

        public void BeginInit() { }
        public void EndInit() => activationOrder.Add(name);
        public void Dispose() => Disposed?.Invoke(this, EventArgs.Empty);
    }
}
