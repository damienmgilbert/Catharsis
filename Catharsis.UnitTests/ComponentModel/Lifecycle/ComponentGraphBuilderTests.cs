using System.ComponentModel;

using Catharsis.ComponentModel.Lifecycle;

namespace Catharsis.UnitTests.ComponentModel.Lifecycle;

[TestClass]
public sealed class ComponentGraphBuilderTests
{
    [TestMethod]
    public void AddComponent_NullComponent_ThrowsArgumentNullException()
    {
        var builder = new ComponentGraphBuilder();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => builder.AddComponent(null!));
    }

    [TestMethod]
    public void AddComponent_DuplicateComponent_ThrowsInvalidOperationException()
    {
        var builder = new ComponentGraphBuilder();
        var c = new StubComponent();
        builder.AddComponent(c);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => builder.AddComponent(c));
    }

    [TestMethod]
    public void AddComponent_ReturnsSelfForChaining()
    {
        var builder = new ComponentGraphBuilder();

        var result = builder.AddComponent(new StubComponent());

        Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void AddDependency_NullDependent_ThrowsArgumentNullException()
    {
        var builder = new ComponentGraphBuilder();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => builder.AddDependency(null!, new StubComponent()));
    }

    [TestMethod]
    public void AddDependency_NullDependency_ThrowsArgumentNullException()
    {
        var builder = new ComponentGraphBuilder();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => builder.AddDependency(new StubComponent(), null!));
    }

    [TestMethod]
    public void AddDependency_ReturnsSelfForChaining()
    {
        var builder = new ComponentGraphBuilder();
        var c1 = new StubComponent();
        var c2 = new StubComponent();
        builder.AddComponent(c1);
        builder.AddComponent(c2);

        var result = builder.AddDependency(c1, c2);

        Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void Build_EmptyGraph_Succeeds()
    {
        var builder = new ComponentGraphBuilder();

        var graph = builder.Build();

        Assert.AreEqual(0, graph.Count);
    }

    [TestMethod]
    public void Build_WithComponents_ReturnsGraph()
    {
        var c1 = new StubComponent();
        var c2 = new StubComponent();
        var graph = new ComponentGraphBuilder()
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
        var db = new StubComponent();
        var app = new StubComponent();
        var graph = new ComponentGraphBuilder()
            .AddComponent(db, "DB")
            .AddComponent(app, "App")
            .AddDependency(app, db)
            .Build();

        var appNode = graph.GetNode(app);
        Assert.IsNotNull(appNode);
        Assert.AreEqual(1, appNode.Dependencies.Count);
        Assert.AreEqual("DB", appNode.Dependencies[0].Name);
    }

    [TestMethod]
    public void Build_UnregisteredDependent_ThrowsInvalidOperationException()
    {
        var builder = new ComponentGraphBuilder();
        var registered = new StubComponent();
        var unregistered = new StubComponent();
        builder.AddComponent(registered);
        builder.AddDependency(unregistered, registered);

        Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
    }

    [TestMethod]
    public void Build_UnregisteredDependency_ThrowsInvalidOperationException()
    {
        var builder = new ComponentGraphBuilder();
        var registered = new StubComponent();
        var unregistered = new StubComponent();
        builder.AddComponent(registered);
        builder.AddDependency(registered, unregistered);

        Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
    }

    [TestMethod]
    public void Clear_ResetsBuilder()
    {
        var builder = new ComponentGraphBuilder();
        builder.AddComponent(new StubComponent());

        var result = builder.Clear();

        Assert.AreSame(builder, result);

        var graph = builder.Build();
        Assert.AreEqual(0, graph.Count);
    }

    private sealed class StubComponent : IComponent
    {
        public ISite? Site { get; set; }
        public event EventHandler? Disposed;
        public void Dispose() => Disposed?.Invoke(this, EventArgs.Empty);
    }
}
