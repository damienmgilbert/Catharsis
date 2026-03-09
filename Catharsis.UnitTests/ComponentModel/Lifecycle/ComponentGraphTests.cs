using System.ComponentModel;
using Catharsis.ComponentModel.Lifecycle;

namespace Catharsis.UnitTests.ComponentModel.Lifecycle;

///<summary>
///Unit tests for the <see cref="ComponentGraphNode"/> class.
///</summary>
[TestClass]
public sealed class ComponentGraphNodeTests
{
    #region Public methods
    [TestMethod]
    public void AreDependenciesSatisfied_NoDependencies_ReturnsTrue()
    {
        ComponentGraphNode node = new ComponentGraphNode(new StubComponent());

        Assert.IsTrue(node.AreDependenciesSatisfied);
    }

    [TestMethod]
    public void Constructor_CustomName_SetsName()
    {
        ComponentGraphNode node = new ComponentGraphNode(new StubComponent(), "MyNode");

        Assert.AreEqual("MyNode", node.Name);
    }

    [TestMethod]
    public void Constructor_NullComponent_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ComponentGraphNode(null!)); }
    [TestMethod]
    public void Constructor_SetsComponentAndDefaultName()
    {
        StubComponent component = new StubComponent();
        ComponentGraphNode node = new ComponentGraphNode(component);

        Assert.AreSame(component, node.Component);
        Assert.AreEqual("StubComponent", node.Name);
    }

    [TestMethod]
    public void Dependencies_InitiallyEmpty()
    {
        ComponentGraphNode node = new ComponentGraphNode(new StubComponent());

        Assert.IsEmpty(node.Dependencies);
    }

    [TestMethod]
    public void Dependents_InitiallyEmpty()
    {
        ComponentGraphNode node = new ComponentGraphNode(new StubComponent());

        Assert.IsEmpty(node.Dependents);
    }

    [TestMethod]
    public void State_DefaultIsCreated()
    {
        ComponentGraphNode node = new ComponentGraphNode(new StubComponent());

        Assert.AreEqual(ComponentState.Created, node.State);
    }

    [TestMethod]
    public void ToString_ContainsNameAndState()
    {
        ComponentGraphNode node = new ComponentGraphNode(new StubComponent(), "DB");

        string result = node.ToString();

        Assert.Contains("DB", result);
        Assert.Contains("Created", result);
    }
    #endregion

    internal sealed class StubComponent : IComponent
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
}

///<summary>
///Unit tests for the <see cref="ComponentGraphNode"/> class.
///</summary>
[TestClass]
public sealed class ComponentGraphTests
{
    #region Private methods
    static ComponentGraph BuildGraph(params IComponent[] components)
    {
        ComponentGraphBuilder builder = new ComponentGraphBuilder();
        foreach(IComponent c in components)
        {
            builder.AddComponent(c);
        }

        return builder.Build();
    }
    #endregion

    #region Public methods
    [TestMethod]
    public void Contains_NullComponent_ThrowsArgumentNullException()
    {
        ComponentGraph graph = BuildGraph();

        Assert.ThrowsExactly<ArgumentNullException>(() => graph.Contains(null!));
    }

    [TestMethod]
    public void Contains_RegisteredComponent_ReturnsTrue()
    {
        ComponentGraphNodeTests.StubComponent c = new ComponentGraphNodeTests.StubComponent();
        ComponentGraph graph = BuildGraph(c);

        Assert.IsTrue(graph.Contains(c));
    }

    [TestMethod]
    public void Empty_Graph_HasNoNodes()
    {
        ComponentGraph graph = BuildGraph();

        Assert.AreEqual(0, graph.Count);
    }

    [TestMethod]
    public void GetActivationOrder_DependenciesBeforeDependents()
    {
        ComponentGraphNodeTests.StubComponent db = new ComponentGraphNodeTests.StubComponent();
        ComponentGraphNodeTests.StubComponent app = new ComponentGraphNodeTests.StubComponent();
        ComponentGraph graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();

        IReadOnlyList<ComponentGraphNode> order = graph.GetActivationOrder();

        int dbIndex = order.ToList().FindIndex(static n => n.Name == "DB");
        int appIndex = order.ToList().FindIndex(static n => n.Name == "App");
        Assert.IsLessThan(appIndex, dbIndex);
    }

    [TestMethod]
    public void GetActivationOrder_NoDependencies_ReturnsAllNodes()
    {
        ComponentGraphNodeTests.StubComponent c1 = new ComponentGraphNodeTests.StubComponent();
        ComponentGraphNodeTests.StubComponent c2 = new ComponentGraphNodeTests.StubComponent();
        ComponentGraph graph = BuildGraph(c1, c2);

        IReadOnlyList<ComponentGraphNode> order = graph.GetActivationOrder();

        Assert.HasCount(2, order);
    }

    [TestMethod]
    public void GetDeactivationOrder_DependentsBeforeDependencies()
    {
        ComponentGraphNodeTests.StubComponent db = new ComponentGraphNodeTests.StubComponent();
        ComponentGraphNodeTests.StubComponent app = new ComponentGraphNodeTests.StubComponent();
        ComponentGraph graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();

        IReadOnlyList<ComponentGraphNode> order = graph.GetDeactivationOrder();

        int dbIndex = order.ToList().FindIndex(static n => n.Name == "DB");
        int appIndex = order.ToList().FindIndex(static n => n.Name == "App");
        Assert.IsLessThan(dbIndex, appIndex);
    }

    [TestMethod]
    public void GetLeaves_ReturnsNodesWithNoDependents()
    {
        ComponentGraphNodeTests.StubComponent db = new ComponentGraphNodeTests.StubComponent();
        ComponentGraphNodeTests.StubComponent app = new ComponentGraphNodeTests.StubComponent();
        ComponentGraph graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();

        IReadOnlyList<ComponentGraphNode> leaves = graph.GetLeaves();

        Assert.HasCount(1, leaves);
        Assert.AreEqual("App", leaves[0].Name);
    }

    [TestMethod]
    public void GetNode_RegisteredComponent_ReturnsNode()
    {
        ComponentGraphNodeTests.StubComponent c = new ComponentGraphNodeTests.StubComponent();
        ComponentGraph graph = BuildGraph(c);

        ComponentGraphNode? node = graph.GetNode(c);

        Assert.IsNotNull(node);
        Assert.AreSame(c, node.Component);
    }

    [TestMethod]
    public void GetNode_UnregisteredComponent_ReturnsNull()
    {
        ComponentGraph graph = BuildGraph();

        Assert.IsNull(graph.GetNode(new ComponentGraphNodeTests.StubComponent()));
    }

    [TestMethod]
    public void GetRoots_ReturnsNodesWithNoDependencies()
    {
        ComponentGraphNodeTests.StubComponent db = new ComponentGraphNodeTests.StubComponent();
        ComponentGraphNodeTests.StubComponent app = new ComponentGraphNodeTests.StubComponent();
        ComponentGraph graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();

        IReadOnlyList<ComponentGraphNode> roots = graph.GetRoots();

        Assert.HasCount(1, roots);
        Assert.AreEqual("DB", roots[0].Name);
    }
    #endregion
}
