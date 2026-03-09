using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

///<summary>
///Unit tests for the <see cref="TreeNodeNonGeneric"/> class.
///</summary>
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
        Assert.HasCount(1, parent.Children);
        Assert.AreSame(child, parent.Children[0]);
    }

    [TestMethod]
    public void Constructor_SetsNameAndEmptyChildren()
    {
        TreeNode node = new TreeNode("root");
        Assert.AreEqual("root", node.Name);
        Assert.IsEmpty(node.Children);
    }
    #endregion
}
