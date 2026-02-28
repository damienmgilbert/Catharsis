using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

[TestClass]
public class TreeNodeGenericTests
{
    #region Public methods
    [TestMethod]
    public void AddChild_AncestorAsCycle_Throws()
    {
        TreeNode<int> root = new TreeNode<int>(1);
        TreeNode<int> child = root.AddChild(2);
        Assert.ThrowsExactly<InvalidOperationException>(() => child.AddChild(root));
    }

    [TestMethod]
    public void AddChild_Node_DetachesFromPreviousParent()
    {
        TreeNode<int> root1 = new TreeNode<int>(1);
        TreeNode<int> root2 = new TreeNode<int>(2);
        TreeNode<int> child = root1.AddChild(10);

        root2.AddChild(child);

        Assert.IsEmpty(root1.Children);
        Assert.AreSame(root2, child.Parent);
    }

    [TestMethod]
    public void AddChild_NullNode_Throws()
    {
        TreeNode<int> root = new TreeNode<int>(1);
        Assert.ThrowsExactly<ArgumentNullException>(() => root.AddChild((TreeNode<int>)null!));
    }

    [TestMethod]
    public void AddChild_Value_CreatesChildWithParent()
    {
        TreeNode<string> root = new TreeNode<string>("root");
        TreeNode<string> child = root.AddChild("child");
        Assert.AreEqual("child", child.Value);
        Assert.AreSame(root, child.Parent);
        Assert.HasCount(1, root.Children);
        Assert.IsFalse(root.IsLeaf);
        Assert.IsFalse(child.IsRoot);
    }

    [TestMethod]
    public void BreadthFirst_ReturnsCorrectOrder()
    {
        TreeNode<int> root = new TreeNode<int>(1);
        TreeNode<int> c1 = root.AddChild(2);
        TreeNode<int> c2 = root.AddChild(3);
        c1.AddChild(4);

        List<int> result = root.BreadthFirst().Select(n => n.Value).ToList();

        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, result);
    }

    [TestMethod]
    public void Constructor_SetsValue()
    {
        TreeNode<int> node = new TreeNode<int>(42);
        Assert.AreEqual(42, node.Value);
        Assert.IsTrue(node.IsRoot);
        Assert.IsTrue(node.IsLeaf);
        Assert.AreEqual(0, node.Depth);
    }

    [TestMethod]
    public void Depth_ReturnsCorrectDepth()
    {
        TreeNode<int> root = new TreeNode<int>(1);
        TreeNode<int> child = root.AddChild(2);
        TreeNode<int> grandchild = child.AddChild(3);
        Assert.AreEqual(0, root.Depth);
        Assert.AreEqual(1, child.Depth);
        Assert.AreEqual(2, grandchild.Depth);
    }

    [TestMethod]
    public void DepthFirst_ReturnsCorrectOrder()
    {
        TreeNode<int> root = new TreeNode<int>(1);
        TreeNode<int> c1 = root.AddChild(2);
        TreeNode<int> c2 = root.AddChild(3);
        c1.AddChild(4);

        List<int> result = root.DepthFirst().Select(n => n.Value).ToList();

        CollectionAssert.AreEqual(new[] { 1, 2, 4, 3 }, result);
    }

    [TestMethod]
    public void IsDescendantOf_ReturnsCorrectResult()
    {
        TreeNode<int> root = new TreeNode<int>(1);
        TreeNode<int> child = root.AddChild(2);
        TreeNode<int> grandchild = child.AddChild(3);
        Assert.IsTrue(grandchild.IsDescendantOf(root));
        Assert.IsFalse(root.IsDescendantOf(grandchild));
    }

    [TestMethod]
    public void RemoveChild_ExistingChild_ReturnsTrue()
    {
        TreeNode<int> root = new TreeNode<int>(1);
        TreeNode<int> child = root.AddChild(2);
        Assert.IsTrue(root.RemoveChild(child));
        Assert.IsEmpty(root.Children);
        Assert.IsNull(child.Parent);
    }

    [TestMethod]
    public void Root_ReturnsRootNode()
    {
        TreeNode<int> root = new TreeNode<int>(1);
        TreeNode<int> child = root.AddChild(2);
        TreeNode<int> grandchild = child.AddChild(3);
        Assert.AreSame(root, grandchild.Root());
    }
    #endregion
}
