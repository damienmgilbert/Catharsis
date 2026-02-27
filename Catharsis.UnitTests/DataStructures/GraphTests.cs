using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

[TestClass]
public class GraphTests
{
    #region Public methods
    [TestMethod]
    public void AddEdge_CreatesVerticesAndEdge()
    {
        Graph<int> g = new Graph<int>();
        Assert.IsTrue(g.AddEdge(1, 2));
        Assert.IsTrue(g.HasEdge(1, 2));
        Assert.IsFalse(g.HasEdge(2, 1));
        Assert.AreEqual(2, g.VertexCount);
        Assert.AreEqual(1, g.EdgeCount);
    }

    [TestMethod]
    public void AddVertex_And_ContainsVertex()
    {
        Graph<int> g = new Graph<int>();
        Assert.IsTrue(g.AddVertex(1));
        Assert.IsTrue(g.ContainsVertex(1));
        Assert.AreEqual(1, g.VertexCount);
    }

    [TestMethod]
    public void AddVertex_Duplicate_ReturnsFalse()
    {
        Graph<int> g = new Graph<int>();
        g.AddVertex(1);
        Assert.IsFalse(g.AddVertex(1));
    }

    [TestMethod]
    public void BreadthFirst_TraversesCorrectly()
    {
        Graph<int> g = new Graph<int>();
        g.AddEdge(1, 2);
        g.AddEdge(1, 3);
        g.AddEdge(2, 4);
        List<int> result = g.BreadthFirst(1).ToList();
        Assert.AreEqual(1, result[0]);
        Assert.IsTrue(result.Contains(2));
        Assert.IsTrue(result.Contains(3));
        Assert.IsTrue(result.Contains(4));
    }

    [TestMethod]
    public void Clear_RemovesAll()
    {
        Graph<int> g = new Graph<int>();
        g.AddEdge(1, 2);
        g.Clear();
        Assert.AreEqual(0, g.VertexCount);
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws() { Assert.ThrowsExactly<ArgumentNullException>(() => new Graph<int>(null!)); }
    [TestMethod]
    public void DepthFirst_TraversesCorrectly()
    {
        Graph<int> g = new Graph<int>();
        g.AddEdge(1, 2);
        g.AddEdge(1, 3);
        g.AddEdge(2, 4);
        List<int> result = g.DepthFirst(1).ToList();
        Assert.IsTrue(result.Contains(1));
        Assert.IsTrue(result.Contains(2));
        Assert.IsTrue(result.Contains(4));
    }

    [TestMethod]
    public void Neighbors_NonexistentVertex_Throws() { Assert.ThrowsExactly<KeyNotFoundException>(() => new Graph<int>().Neighbors(99).ToList()); }
    [TestMethod]
    public void Neighbors_ReturnsAdjacentVertices()
    {
        Graph<int> g = new Graph<int>();
        g.AddEdge(1, 2);
        g.AddEdge(1, 3);
        List<int> neighbors = g.Neighbors(1).ToList();
        Assert.AreEqual(2, neighbors.Count);
        Assert.IsTrue(neighbors.Contains(2));
        Assert.IsTrue(neighbors.Contains(3));
    }

    [TestMethod]
    public void RemoveEdge_RemovesEdge()
    {
        Graph<int> g = new Graph<int>();
        g.AddEdge(1, 2);
        Assert.IsTrue(g.RemoveEdge(1, 2));
        Assert.IsFalse(g.HasEdge(1, 2));
    }

    [TestMethod]
    public void RemoveVertex_RemovesVertexAndEdges()
    {
        Graph<int> g = new Graph<int>();
        g.AddEdge(1, 2);
        g.AddEdge(2, 3);
        g.AddEdge(3, 1);
        Assert.IsTrue(g.RemoveVertex(2));
        Assert.IsFalse(g.ContainsVertex(2));
        Assert.IsFalse(g.HasEdge(1, 2));
    }
    #endregion
}
