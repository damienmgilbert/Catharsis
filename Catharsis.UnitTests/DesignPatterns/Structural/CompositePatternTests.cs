using Catharsis.DataStructures;
using Catharsis.DesignPatterns.Structural;

namespace Catharsis.UnitTests.DesignPatterns.Structural;

[TestClass]
public class CompositePatternTests
{
    #region Public methods

    ///<summary>
    ///Tests that Composite allows the action to modify the nodes during traversal.
    ///</summary>
    [TestMethod]
    public void Composite_ActionModifiesNodes_NodesAreModified()
    {
        // Arrange
        TreeNode root = new("root");
        TreeNode child1 = new("child1");
        TreeNode child2 = new("child2");
        root.AddChild(child1);
        root.AddChild(child2);
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = static n => n.Children;
        Action<TreeNode> action = static n => n.Name = n.Name.ToUpper();
        // Act
        CompositePattern.Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual("ROOT", root.Name);
        Assert.AreEqual("CHILD1", child1.Name);
        Assert.AreEqual("CHILD2", child2.Name);
    }

    ///<summary>
    ///Tests that Composite returns the original object after processing.
    ///</summary>
    [TestMethod]
    public void Composite_AnyValidInput_ReturnsOriginalObject()
    {
        // Arrange
        TreeNode root = new("root");
        root.AddChild(new TreeNode("child1"));
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = static n => n.Children;
        Action<TreeNode> action = static n =>
        {
        };
        // Act
        TreeNode result = CompositePattern.Composite(root, getChildren, action);
        // Assert
        Assert.AreSame(root, result);
    }

    ///<summary>
    ///Tests that Composite handles a complex tree with multiple levels and branches correctly.
    ///</summary>
    [TestMethod]
    public void Composite_ComplexTreeStructure_VisitsAllNodesInCorrectOrder()
    {
        // Arrange
        TreeNode root = new("A");
        TreeNode b = new("B");
        TreeNode c = new("C");
        TreeNode d = new("D");
        TreeNode e = new("E");
        TreeNode f = new("F");
        TreeNode g = new("G");
        root.AddChild(b);
        root.AddChild(c);
        b.AddChild(d);
        b.AddChild(e);
        c.AddChild(f);
        f.AddChild(g);
        List<string> visitedNodes = [];
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        CompositePattern.Composite(root, getChildren, action);
        // Assert
        Assert.HasCount(7, visitedNodes);
        Assert.AreEqual("A", visitedNodes[0]);
        Assert.AreEqual("B", visitedNodes[1]);
        Assert.AreEqual("D", visitedNodes[2]);
        Assert.AreEqual("E", visitedNodes[3]);
        Assert.AreEqual("C", visitedNodes[4]);
        Assert.AreEqual("F", visitedNodes[5]);
        Assert.AreEqual("G", visitedNodes[6]);
    }

    ///<summary>
    ///Tests that Composite traverses a deep nested tree in pre-order depth-first manner.
    ///</summary>
    [TestMethod]
    public void Composite_DeepNestedTree_VisitsAllNodesInPreOrderDepthFirst()
    {
        // Arrange
        TreeNode root = new("root");
        TreeNode child1 = new("child1");
        TreeNode child2 = new("child2");
        TreeNode grandchild1 = new("grandchild1");
        TreeNode grandchild2 = new("grandchild2");
        root.AddChild(child1);
        root.AddChild(child2);
        child1.AddChild(grandchild1);
        child1.AddChild(grandchild2);
        List<string> visitedNodes = [];
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        CompositePattern.Composite(root, getChildren, action);
        // Assert
        Assert.HasCount(5, visitedNodes);
        Assert.AreEqual("root", visitedNodes[0]);
        Assert.AreEqual("child1", visitedNodes[1]);
        Assert.AreEqual("grandchild1", visitedNodes[2]);
        Assert.AreEqual("grandchild2", visitedNodes[3]);
        Assert.AreEqual("child2", visitedNodes[4]);
    }

    ///<summary>
    ///Tests that Composite traverses a flat tree (root with multiple direct children) in pre-order.
    ///</summary>
    [TestMethod]
    public void Composite_FlatTreeWithMultipleChildren_VisitsAllNodesInPreOrder()
    {
        // Arrange
        TreeNode root = new("root");
        root.AddChild(new TreeNode("child1"));
        root.AddChild(new TreeNode("child2"));
        root.AddChild(new TreeNode("child3"));
        List<string> visitedNodes = [];
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        CompositePattern.Composite(root, getChildren, action);
        // Assert
        Assert.HasCount(4, visitedNodes);
        Assert.AreEqual("root", visitedNodes[0]);
        Assert.AreEqual("child1", visitedNodes[1]);
        Assert.AreEqual("child2", visitedNodes[2]);
        Assert.AreEqual("child3", visitedNodes[3]);
    }

    ///<summary>
    ///Tests that Composite handles empty children collections correctly.
    ///</summary>
    [TestMethod]
    public void Composite_GetChildrenReturnsEmptyCollection_ProcessesOnlyRoot()
    {
        // Arrange
        TreeNode root = new("root");
        int visitCount = 0;
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => new List<TreeNode>();
        Action<TreeNode> action = n => visitCount++;
        // Act
        CompositePattern.Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(1, visitCount);
    }

    ///<summary>
    ///Tests that Composite handles nodes with varying numbers of children correctly.
    ///</summary>
    [TestMethod]
    public void Composite_MixedTreeSomeNodesHaveChildrenSomeDont_VisitsAllNodes()
    {
        // Arrange
        TreeNode root = new("root");
        TreeNode child1 = new("child1");
        TreeNode child2 = new("child2");
        TreeNode grandchild = new("grandchild");
        root.AddChild(child1);
        root.AddChild(child2);
        child1.AddChild(grandchild);
        // child2 has no children
        List<string> visitedNodes = [];
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        CompositePattern.Composite(root, getChildren, action);
        // Assert
        Assert.HasCount(4, visitedNodes);
        CollectionAssert.AreEqual(new[] { "root", "child1", "grandchild", "child2" }, visitedNodes);
    }

    ///<summary>
    ///Tests that Composite works correctly when the root object is null (for nullable reference types).
    ///</summary>
    [TestMethod]
    public void Composite_NullRootObject_CallsActionWithNull()
    {
        // Arrange
        TreeNode? root = null;
        bool actionCalled = false;
        TreeNode? receivedNode = new("dummy");
        Func<TreeNode?, IEnumerable<TreeNode?>> getChildren = n => Array.Empty<TreeNode?>();
        Action<TreeNode?> action = n =>
        {
            actionCalled = true;
            receivedNode = n;
        };
        // Act
        TreeNode? result = CompositePattern.Composite(root, getChildren, action);
        // Assert
        Assert.IsTrue(actionCalled);
        Assert.IsNull(receivedNode);
        Assert.IsNull(result);
    }

    ///<summary>
    ///Tests that Composite processes a single node with no children correctly.
    ///</summary>
    [TestMethod]
    public void Composite_SingleNodeWithNoChildren_CallsActionOnce()
    {
        // Arrange
        TreeNode root = new("root");
        List<string> visitedNodes = [];
        Func<TreeNode, IEnumerable<TreeNode>> getChildren = n => n.Children;
        Action<TreeNode> action = n => visitedNodes.Add(n.Name);
        // Act
        TreeNode result = CompositePattern.Composite(root, getChildren, action);
        // Assert
        Assert.HasCount(1, visitedNodes);
        Assert.AreEqual("root", visitedNodes[0]);
        Assert.AreSame(root, result);
    }

    ///<summary>
    ///Tests that Composite works with value types.
    ///</summary>
    [TestMethod]
    public void Composite_ValueTypeAsNode_WorksCorrectly()
    {
        // Arrange
        int root = 1;
        List<int> visitedValues = [];
        Func<int, IEnumerable<int>> getChildren = n => (n < 3) ? ([n + 1]) : Array.Empty<int>();
        Action<int> action = n => visitedValues.Add(n);
        // Act
        int result = CompositePattern.Composite(root, getChildren, action);
        // Assert
        Assert.AreEqual(1, result);
        Assert.HasCount(3, visitedValues);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, visitedValues);
    }
    #endregion
}
