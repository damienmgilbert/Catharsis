using Catharsis.DataStructures;
using Catharsis.DesignPatterns.Structural;

namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class CompositePatternTests
{
    /// <summary>
    /// Tests that Composite processes a single node with no children correctly.
    /// </summary>
    [TestMethod]
    public void Composite_SingleNodeWithNoChildren_CallsActionOnce()
    {
        // Arrange
        var root = new TreeNode("root");
        var visitedNodes = new List<string>();
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        var result = new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(1, visitedNodes.Count);
        Assert.AreEqual("root", visitedNodes[0]);
        Assert.AreSame(root, result);
    }

    /// <summary>
    /// Tests that Composite returns the original object after processing.
    /// </summary>
    [TestMethod]
    public void Composite_AnyValidInput_ReturnsOriginalObject()
    {
        // Arrange
        var root = new TreeNode("root");
        root.AddChild(new TreeNode("child1"));
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n =>
        {
        };
        // Act
        var result = new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreSame(root, result);
    }

    /// <summary>
    /// Tests that Composite traverses a flat tree (root with multiple direct children) in pre-order.
    /// </summary>
    [TestMethod]
    public void Composite_FlatTreeWithMultipleChildren_VisitsAllNodesInPreOrder()
    {
        // Arrange
        var root = new TreeNode("root");
        root.AddChild(new TreeNode("child1"));
        root.AddChild(new TreeNode("child2"));
        root.AddChild(new TreeNode("child3"));
        var visitedNodes = new List<string>();
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(4, visitedNodes.Count);
        Assert.AreEqual("root", visitedNodes[0]);
        Assert.AreEqual("child1", visitedNodes[1]);
        Assert.AreEqual("child2", visitedNodes[2]);
        Assert.AreEqual("child3", visitedNodes[3]);
    }

    /// <summary>
    /// Tests that Composite traverses a deep nested tree in pre-order depth-first manner.
    /// </summary>
    [TestMethod]
    public void Composite_DeepNestedTree_VisitsAllNodesInPreOrderDepthFirst()
    {
        // Arrange
        var root = new TreeNode("root");
        var child1 = new TreeNode("child1");
        var child2 = new TreeNode("child2");
        var grandchild1 = new TreeNode("grandchild1");
        var grandchild2 = new TreeNode("grandchild2");
        root.AddChild(child1);
        root.AddChild(child2);
        child1.AddChild(grandchild1);
        child1.AddChild(grandchild2);
        var visitedNodes = new List<string>();
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(5, visitedNodes.Count);
        Assert.AreEqual("root", visitedNodes[0]);
        Assert.AreEqual("child1", visitedNodes[1]);
        Assert.AreEqual("grandchild1", visitedNodes[2]);
        Assert.AreEqual("grandchild2", visitedNodes[3]);
        Assert.AreEqual("child2", visitedNodes[4]);
    }

    /// <summary>
    /// Tests that Composite handles a complex tree with multiple levels and branches correctly.
    /// </summary>
    [TestMethod]
    public void Composite_ComplexTreeStructure_VisitsAllNodesInCorrectOrder()
    {
        // Arrange
        var root = new TreeNode("A");
        var b = new TreeNode("B");
        var c = new TreeNode("C");
        var d = new TreeNode("D");
        var e = new TreeNode("E");
        var f = new TreeNode("F");
        var g = new TreeNode("G");
        root.AddChild(b);
        root.AddChild(c);
        b.AddChild(d);
        b.AddChild(e);
        c.AddChild(f);
        f.AddChild(g);
        var visitedNodes = new List<string>();
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(7, visitedNodes.Count);
        Assert.AreEqual("A", visitedNodes[0]);
        Assert.AreEqual("B", visitedNodes[1]);
        Assert.AreEqual("D", visitedNodes[2]);
        Assert.AreEqual("E", visitedNodes[3]);
        Assert.AreEqual("C", visitedNodes[4]);
        Assert.AreEqual("F", visitedNodes[5]);
        Assert.AreEqual("G", visitedNodes[6]);
    }

    /// <summary>
    /// Tests that Composite works correctly when the root object is null (for nullable reference types).
    /// </summary>
    [TestMethod]
    public void Composite_NullRootObject_CallsActionWithNull()
    {
        // Arrange
        TreeNode? root = null;
        var actionCalled = false;
        TreeNode? receivedNode = new TreeNode("dummy");
        Func<TreeNode?, IEnumerable<TreeNode?>> getChildren = n => Array.Empty<TreeNode?>();
        Action<TreeNode?> action = n =>
        {
            actionCalled = true;
            receivedNode = n;
        };
        // Act
        var result = new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.IsTrue(actionCalled);
        Assert.IsNull(receivedNode);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that Composite allows the action to modify the nodes during traversal.
    /// </summary>
    [TestMethod]
    public void Composite_ActionModifiesNodes_NodesAreModified()
    {
        // Arrange
        var root = new TreeNode("root");
        var child1 = new TreeNode("child1");
        var child2 = new TreeNode("child2");
        root.AddChild(child1);
        root.AddChild(child2);
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => n.Name = n.Name.ToUpper();
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual("ROOT", root.Name);
        Assert.AreEqual("CHILD1", child1.Name);
        Assert.AreEqual("CHILD2", child2.Name);
    }

    /// <summary>
    /// Tests that Composite handles empty children collections correctly.
    /// </summary>
    [TestMethod]
    public void Composite_GetChildrenReturnsEmptyCollection_ProcessesOnlyRoot()
    {
        // Arrange
        var root = new TreeNode("root");
        var visitCount = 0;
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => new List<TreeNode>();
        Action<TreeNode> action = n => visitCount++;
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(1, visitCount);
    }

    /// <summary>
    /// Tests that Composite handles nodes with varying numbers of children correctly.
    /// </summary>
    [TestMethod]
    public void Composite_MixedTreeSomeNodesHaveChildrenSomeDont_VisitsAllNodes()
    {
        // Arrange
        var root = new TreeNode("root");
        var child1 = new TreeNode("child1");
        var child2 = new TreeNode("child2");
        var grandchild = new TreeNode("grandchild");
        root.AddChild(child1);
        root.AddChild(child2);
        child1.AddChild(grandchild);
        // child2 has no children
        var visitedNodes = new List<string>();
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(4, visitedNodes.Count);
        CollectionAssert.AreEqual(new[] { "root", "child1", "grandchild", "child2" }, visitedNodes);
    }

    /// <summary>
    /// Tests that Composite works with value types.
    /// </summary>
    [TestMethod]
    public void Composite_ValueTypeAsNode_WorksCorrectly()
    {
        // Arrange
        var root = 1;
        var visitedValues = new List<int>();
        Func<int, IEnumerable<int>> getChildren = n => n < 3 ? new[]
        {
            n + 1
        }

        : Array.Empty<int>();
        Action<int> action = n => visitedValues.Add(n);
        // Act
        var result = new CompositePattern().Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(1, result);
        Assert.AreEqual(3, visitedValues.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, visitedValues);
    }
}
