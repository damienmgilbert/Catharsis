using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="SequenceFlattener"/> class.
///</summary>
[TestClass]
public class SequenceFlattenerTests
{
    private sealed record TreeNode(string Name, TreeNode[]? Children = null);

    [TestMethod]
    public void Flatten_SequenceOfSequences_FlattensAll()
    {
        int[][] source = [[1, 2], [3], [4, 5]];
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, source.Flatten().ToList());
    }

    [TestMethod]
    public void Flatten_Arrays_FlattensAll()
    {
        int[][] source = [[1, 2], [3, 4]];
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, source.Flatten().ToList());
    }

    [TestMethod]
    public void Flatten_Lists_FlattensAll()
    {
        List<List<int>> source = [new() { 1, 2 }, new() { 3 }];
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, source.Flatten().ToList());
    }

    [TestMethod]
    public void Flatten_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<IEnumerable<int>>)null!).Flatten().ToList());
    }

    [TestMethod]
    public void FlatSelect_ProjectsAndFlattensWithIndex()
    {
        List<string> result = new[] { "ab", "cd" }
            .FlatSelect((s, i) => s.Select(c => $"{i}:{c}"))
            .ToList();
        CollectionAssert.AreEqual(new[] { "0:a", "0:b", "1:c", "1:d" }, result);
    }

    [TestMethod]
    public void FlattenRecursive_BreadthFirst_TraversesCorrectly()
    {
        TreeNode tree = new("root", [
            new("a", [new("a1"), new("a2")]),
            new("b")
        ]);

        List<string> result = new[] { tree }
            .FlattenRecursive(static n => n.Children)
            .Select(static n => n.Name)
            .ToList();

        // BFS: root, a, b, a1, a2
        CollectionAssert.AreEqual(new[] { "root", "a", "b", "a1", "a2" }, result);
    }

    [TestMethod]
    public void FlattenRecursiveDepthFirst_TraversesCorrectly()
    {
        TreeNode tree = new("root", [
            new("a", [new("a1"), new("a2")]),
            new("b")
        ]);

        List<string> result = new[] { tree }
            .FlattenRecursiveDepthFirst(static n => n.Children)
            .Select(static n => n.Name)
            .ToList();

        // DFS: root, a, a1, a2, b
        CollectionAssert.AreEqual(new[] { "root", "a", "a1", "a2", "b" }, result);
    }

    [TestMethod]
    public void FlattenWithDepth_ReturnsElementsWithDepth()
    {
        TreeNode tree = new("root", [new("child", [new("grandchild")])]);

        List<(TreeNode, int)> result = new[] { tree }
            .FlattenWithDepth(static n => n.Children)
            .ToList();

        Assert.AreEqual(("root", 0), (result[0].Item1.Name, result[0].Item2));
        Assert.AreEqual(("child", 1), (result[1].Item1.Name, result[1].Item2));
        Assert.AreEqual(("grandchild", 2), (result[2].Item1.Name, result[2].Item2));
    }

    [TestMethod]
    public void FlattenRecursive_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<TreeNode>)null!).FlattenRecursive(static n => n.Children).ToList());
    }

    [TestMethod]
    public void Ungroup_DiscardsKeysAndFlattens()
    {
        IGrouping<string, int>[] groups =
        [
            SequenceFactory.Grouping("a", 1, 2),
            SequenceFactory.Grouping("b", 3)
        ];

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, groups.AsEnumerable().Ungroup().ToList());
    }

    [TestMethod]
    public void Ungroup_WithSelector_ProjectsKeyAndElement()
    {
        IGrouping<string, int>[] groups = [SequenceFactory.Grouping("x", 1, 2)];

        List<string> result = groups.AsEnumerable().Ungroup(static (key, val) => $"{key}:{val}").ToList();
        CollectionAssert.AreEqual(new[] { "x:1", "x:2" }, result);
    }

    [TestMethod]
    public void UngroupToTuples_ReturnsKeyElementTuples()
    {
        IGrouping<string, int>[] groups = [SequenceFactory.Grouping("k", 1, 2)];

        List<(string, int)> result = groups.AsEnumerable().UngroupToTuples().ToList();
        Assert.AreEqual(("k", 1), result[0]);
        Assert.AreEqual(("k", 2), result[1]);
    }
}
