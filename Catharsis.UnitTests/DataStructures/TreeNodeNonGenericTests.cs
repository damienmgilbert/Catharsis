using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

[TestClass]
public class TreeNodeNonGenericTests
{
    #region Public methods
    [TestMethod]
    public void AddChild_AddsToChildren()
    {
        TreeNode parent = new TreeNode("parent");
        TreeNode child = new TreeNode("child");
        parent.AddChild(child);
        Assert.AreEqual(1, parent.Children.Count);
        Assert.AreSame(child, parent.Children[0]);
    }

    [TestMethod]
    public void Constructor_SetsNameAndEmptyChildren()
    {
        TreeNode node = new TreeNode("root");
        Assert.AreEqual("root", node.Name);
        Assert.AreEqual(0, node.Children.Count);
    }
    #endregion
}
