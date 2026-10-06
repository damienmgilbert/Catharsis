using Catharsis.DataStructures;
using Catharsis.Patterns.Composed;

namespace Catharsis.UnitTests.Patterns.Composed;

///<summary>
///Unit tests for <see cref="TreeIterator"/>.
///</summary>
[TestClass]
public class TreeIteratorTests
{
    //        1
    //      / | \
    //     2  3  4
    //    / \    |
    //   5   6   7
    static TreeNode<int> Build()
    {
        TreeNode<int> root = new(1);
        TreeNode<int> two = root.AddChild(2);
        root.AddChild(3);
        TreeNode<int> four = root.AddChild(4);
        two.AddChild(5);
        two.AddChild(6);
        four.AddChild(7);

        return root;
    }

    [TestMethod]
    public void Leaves_ReturnsChildlessNodesInDepthFirstOrder() { CollectionAssert.AreEqual(new[] { 5, 6, 3, 7 }, TreeIterator.Leaves(Build()).Select(static n => n.Value).ToArray()); }

    [TestMethod]
    public void Leaves_SingleNode_IsItself() { CollectionAssert.AreEqual(new[] { 9 }, TreeIterator.Leaves(new TreeNode<int>(9)).Select(static n => n.Value).ToArray()); }

    [TestMethod]
    public void Ancestors_WalkFromParentToRoot()
    {
        TreeNode<int> six = TreeIterator.Leaves(Build()).First(static n => n.Value == 6);

        CollectionAssert.AreEqual(new[] { 2, 1 }, TreeIterator.Ancestors(six).Select(static n => n.Value).ToArray());
    }

    [TestMethod]
    public void Ancestors_Root_IsEmpty() { Assert.AreEqual(0, TreeIterator.Ancestors(Build()).Count()); }

    [TestMethod]
    public void Levels_GroupNodesByDepth()
    {
        int[][] levels = [.. TreeIterator.Levels(Build()).Select(static level => level.Select(static n => n.Value).ToArray())];

        Assert.AreEqual(3, levels.Length);
        CollectionAssert.AreEqual(new[] { 1 }, levels[0]);
        CollectionAssert.AreEqual(new[] { 2, 3, 4 }, levels[1]);
        CollectionAssert.AreEqual(new[] { 5, 6, 7 }, levels[2]);
    }

    [TestMethod]
    public void Paths_ListEveryRootToLeafRoute()
    {
        string[] paths = [.. TreeIterator.Paths(Build()).Select(static p => string.Join("-", p))];

        CollectionAssert.AreEqual(new[] { "1-2-5", "1-2-6", "1-3", "1-4-7" }, paths);
    }

    [TestMethod]
    public void Paths_DeepChain_DoesNotOverflowTheStack()
    {
        TreeNode<int> node = new(0);
        TreeNode<int> root = node;

        for (int i = 1; i <= 20_000; i++)
        {
            node = node.AddChild(i);
        }

        IReadOnlyList<int> only = TreeIterator.Paths(root).Single();

        Assert.AreEqual(20_001, only.Count);
    }

    [TestMethod]
    public void NullArguments_Throw_Eagerly()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => TreeIterator.Leaves<int>(null!));
        Assert.ThrowsExactly<ArgumentNullException>(static () => TreeIterator.Ancestors<int>(null!));
        Assert.ThrowsExactly<ArgumentNullException>(static () => TreeIterator.Levels<int>(null!));
        Assert.ThrowsExactly<ArgumentNullException>(static () => TreeIterator.Paths<int>(null!));
    }
}
