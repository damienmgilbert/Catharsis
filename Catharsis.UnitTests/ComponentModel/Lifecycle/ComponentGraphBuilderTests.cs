using System.ComponentModel;
using Catharsis.ComponentModel.Lifecycle;

namespace Catharsis.UnitTests.ComponentModel.Lifecycle;

///<summary>
///Unit tests for the <see cref="ComponentGraphBuilder"/> class.
///</summary>
[TestClass]
public sealed class ComponentGraphBuilderTests
{
    #region Public methods
    [TestMethod]
    public void AddComponent_DuplicateComponent_ThrowsInvalidOperationException()
    {
        ComponentGraphBuilder builder = new();
        StubComponent c = new();
        builder.AddComponent(c);

        Assert.ThrowsExactly<InvalidOperationException>(() => builder.AddComponent(c));
    }

    [TestMethod]
    public void AddComponent_NullComponent_ThrowsArgumentNullException()
    {
        ComponentGraphBuilder builder = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => builder.AddComponent(null!));
    }

    [TestMethod]
    public void AddComponent_ReturnsSelfForChaining()
    {
        ComponentGraphBuilder builder = new();

        ComponentGraphBuilder result = builder.AddComponent(new StubComponent());

        Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void AddDependency_NullDependency_ThrowsArgumentNullException()
    {
        ComponentGraphBuilder builder = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => builder.AddDependency(new StubComponent(), null!));
    }

    [TestMethod]
    public void AddDependency_NullDependent_ThrowsArgumentNullException()
    {
        ComponentGraphBuilder builder = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => builder.AddDependency(null!, new StubComponent()));
    }

    [TestMethod]
    public void AddDependency_ReturnsSelfForChaining()
    {
        ComponentGraphBuilder builder = new();
        StubComponent c1 = new();
        StubComponent c2 = new();
        builder.AddComponent(c1);
        builder.AddComponent(c2);

        ComponentGraphBuilder result = builder.AddDependency(c1, c2);

        Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void Build_EmptyGraph_Succeeds()
    {
        ComponentGraphBuilder builder = new();

        ComponentGraph graph = builder.Build();

        Assert.AreEqual(0, graph.Count);
    }

    [TestMethod]
    public void Build_UnregisteredDependency_ThrowsInvalidOperationException()
    {
        ComponentGraphBuilder builder = new();
        StubComponent registered = new();
        StubComponent unregistered = new();
        builder.AddComponent(registered);
        builder.AddDependency(registered, unregistered);

        Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
    }

    [TestMethod]
    public void Build_UnregisteredDependent_ThrowsInvalidOperationException()
    {
        ComponentGraphBuilder builder = new();
        StubComponent registered = new();
        StubComponent unregistered = new();
        builder.AddComponent(registered);
        builder.AddDependency(unregistered, registered);

        Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
    }

    [TestMethod]
    public void Build_WithComponents_ReturnsGraph()
    {
        StubComponent c1 = new();
        StubComponent c2 = new();
        ComponentGraph graph = new ComponentGraphBuilder()
            .AddComponent(c1, "A")
            .AddComponent(c2, "B")
            .Build();

        Assert.AreEqual(2, graph.Count);
        Assert.IsTrue(graph.Contains(c1));
        Assert.IsTrue(graph.Contains(c2));
    }

    [TestMethod]
    public void Build_WithDependencies_WiresNodes()
    {
        StubComponent db = new();
        StubComponent app = new();
        ComponentGraph graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();

        ComponentGraphNode? appNode = graph.GetNode(app);
        Assert.IsNotNull(appNode);
        Assert.HasCount(1, appNode.Dependencies);
        Assert.AreEqual("DB", appNode.Dependencies[0].Name);
    }

    [TestMethod]
    public void Clear_ResetsBuilder()
    {
        ComponentGraphBuilder builder = new();
        builder.AddComponent(new StubComponent());

        ComponentGraphBuilder result = builder.Clear();

        Assert.AreSame(builder, result);

        ComponentGraph graph = builder.Build();
        Assert.AreEqual(0, graph.Count);
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
}
