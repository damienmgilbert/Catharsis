using Catharsis.ComponentModel.Lifecycle;
using System.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel.Lifecycle;

[TestClass]
public sealed class ComponentGraphNodeTests
{
    [TestMethod]
    public void Constructor_NullComponent_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new ComponentGraphNode(null!));
    }

    [TestMethod]
    public void Constructor_SetsComponentAndDefaultName()
    {
        var component = new StubComponent();
        var node = new ComponentGraphNode(component);

        Assert.AreSame(component, node.Component);
        Assert.AreEqual("StubComponent", node.Name);
    }

    [TestMethod]
    public void Constructor_CustomName_SetsName()
    {
        var node = new ComponentGraphNode(new StubComponent(), "MyNode");

        Assert.AreEqual("MyNode", node.Name);
    }

    [TestMethod]
    public void State_DefaultIsCreated()
    {
        var node = new ComponentGraphNode(new StubComponent());

        Assert.AreEqual(ComponentState.Created, node.State);
    }

    [TestMethod]
    public void Dependencies_InitiallyEmpty()
    {
        var node = new ComponentGraphNode(new StubComponent());

        Assert.AreEqual(0, node.Dependencies.Count);
    }

    [TestMethod]
    public void Dependents_InitiallyEmpty()
    {
        var node = new ComponentGraphNode(new StubComponent());

        Assert.AreEqual(0, node.Dependents.Count);
    }

    [TestMethod]
    public void AreDependenciesSatisfied_NoDependencies_ReturnsTrue()
    {
        var node = new ComponentGraphNode(new StubComponent());

        Assert.IsTrue(node.AreDependenciesSatisfied);
    }

    [TestMethod]
    public void ToString_ContainsNameAndState()
    {
        var node = new ComponentGraphNode(new StubComponent(), "DB");

        var result = node.ToString();

        Assert.IsTrue(result.Contains("DB"));
        Assert.IsTrue(result.Contains("Created"));
    }

    internal sealed class StubComponent : IComponent
    {
        public ISite? Site { get; set; }
        public event EventHandler? Disposed;
        public void Dispose() => Disposed?.Invoke(this, EventArgs.Empty);
    }
}

[TestClass]
public sealed class ComponentGraphTests
{
    [TestMethod]
    public void Empty_Graph_HasNoNodes()
    {
        var graph = BuildGraph();

        Assert.AreEqual(0, graph.Count);
    }

    [TestMethod]
    public void Contains_RegisteredComponent_ReturnsTrue()
    {
        var c = new ComponentGraphNodeTests.StubComponent();
        var graph = BuildGraph(c);

        Assert.IsTrue(graph.Contains(c));
    }

    [TestMethod]
    public void Contains_NullComponent_ThrowsArgumentNullException()
    {
        var graph = BuildGraph();

        Assert.ThrowsExactly<ArgumentNullException>(() => graph.Contains(null!));
    }

    [TestMethod]
    public void GetNode_RegisteredComponent_ReturnsNode()
    {
        var c = new ComponentGraphNodeTests.StubComponent();
        var graph = BuildGraph(c);

        var node = graph.GetNode(c);

        Assert.IsNotNull(node);
        Assert.AreSame(c, node.Component);
    }

    [TestMethod]
    public void GetNode_UnregisteredComponent_ReturnsNull()
    {
        var graph = BuildGraph();

        Assert.IsNull(graph.GetNode(new ComponentGraphNodeTests.StubComponent()));
    }

    [TestMethod]
    public void GetActivationOrder_NoDependencies_ReturnsAllNodes()
    {
        var c1 = new ComponentGraphNodeTests.StubComponent();
        var c2 = new ComponentGraphNodeTests.StubComponent();
        var graph = BuildGraph(c1, c2);

        var order = graph.GetActivationOrder();

        Assert.AreEqual(2, order.Count);
    }

    [TestMethod]
    public void GetActivationOrder_DependenciesBeforeDependents()
    {
        var db = new ComponentGraphNodeTests.StubComponent();
        var app = new ComponentGraphNodeTests.StubComponent();
        var graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();

        var order = graph.GetActivationOrder();

        var dbIndex = order.ToList().FindIndex(n => n.Name == "DB");
        var appIndex = order.ToList().FindIndex(n => n.Name == "App");
        Assert.IsTrue(dbIndex < appIndex);
    }

    [TestMethod]
    public void GetDeactivationOrder_DependentsBeforeDependencies()
    {
        var db = new ComponentGraphNodeTests.StubComponent();
        var app = new ComponentGraphNodeTests.StubComponent();
        var graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();

        var order = graph.GetDeactivationOrder();

        var dbIndex = order.ToList().FindIndex(n => n.Name == "DB");
        var appIndex = order.ToList().FindIndex(n => n.Name == "App");
        Assert.IsTrue(appIndex < dbIndex);
    }

    [TestMethod]
    public void GetRoots_ReturnsNodesWithNoDependencies()
    {
        var db = new ComponentGraphNodeTests.StubComponent();
        var app = new ComponentGraphNodeTests.StubComponent();
        var graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();

        var roots = graph.GetRoots();

        Assert.AreEqual(1, roots.Count);
        Assert.AreEqual("DB", roots[0].Name);
    }

    [TestMethod]
    public void GetLeaves_ReturnsNodesWithNoDependents()
    {
        var db = new ComponentGraphNodeTests.StubComponent();
        var app = new ComponentGraphNodeTests.StubComponent();
        var graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();

        var leaves = graph.GetLeaves();

        Assert.AreEqual(1, leaves.Count);
        Assert.AreEqual("App", leaves[0].Name);
    }

    private static ComponentGraph BuildGraph(params IComponent[] components)
    {
        var builder = new ComponentGraphBuilder();
        foreach (var c in components)
            builder.AddComponent(c);
        return builder.Build();
    }
}
