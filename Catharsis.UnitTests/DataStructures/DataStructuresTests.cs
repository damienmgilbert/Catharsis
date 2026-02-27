namespace Catharsis.DataStructures.UnitTests;

[TestClass]
public class TreeNodeGenericTests
{
    [TestMethod]
    public void Constructor_SetsValue()
    {
        var node = new TreeNode<int>(42);
        Assert.AreEqual(42, node.Value);
        Assert.IsTrue(node.IsRoot);
        Assert.IsTrue(node.IsLeaf);
        Assert.AreEqual(0, node.Depth);
    }

    [TestMethod]
    public void AddChild_Value_CreatesChildWithParent()
    {
        var root = new TreeNode<string>("root");
        var child = root.AddChild("child");
        Assert.AreEqual("child", child.Value);
        Assert.AreSame(root, child.Parent);
        Assert.AreEqual(1, root.Children.Count);
        Assert.IsFalse(root.IsLeaf);
        Assert.IsFalse(child.IsRoot);
    }

    [TestMethod]
    public void AddChild_Node_DetachesFromPreviousParent()
    {
        var root1 = new TreeNode<int>(1);
        var root2 = new TreeNode<int>(2);
        var child = root1.AddChild(10);

        root2.AddChild(child);

        Assert.AreEqual(0, root1.Children.Count);
        Assert.AreSame(root2, child.Parent);
    }

    [TestMethod]
    public void AddChild_NullNode_Throws()
    {
        var root = new TreeNode<int>(1);
        Assert.ThrowsExactly<ArgumentNullException>(() => root.AddChild((TreeNode<int>)null!));
    }

    [TestMethod]
    public void AddChild_AncestorAsCycle_Throws()
    {
        var root = new TreeNode<int>(1);
        var child = root.AddChild(2);
        Assert.ThrowsExactly<InvalidOperationException>(() => child.AddChild(root));
    }

    [TestMethod]
    public void RemoveChild_ExistingChild_ReturnsTrue()
    {
        var root = new TreeNode<int>(1);
        var child = root.AddChild(2);
        Assert.IsTrue(root.RemoveChild(child));
        Assert.AreEqual(0, root.Children.Count);
        Assert.IsNull(child.Parent);
    }

    [TestMethod]
    public void Depth_ReturnsCorrectDepth()
    {
        var root = new TreeNode<int>(1);
        var child = root.AddChild(2);
        var grandchild = child.AddChild(3);
        Assert.AreEqual(0, root.Depth);
        Assert.AreEqual(1, child.Depth);
        Assert.AreEqual(2, grandchild.Depth);
    }

    [TestMethod]
    public void Root_ReturnsRootNode()
    {
        var root = new TreeNode<int>(1);
        var child = root.AddChild(2);
        var grandchild = child.AddChild(3);
        Assert.AreSame(root, grandchild.Root());
    }

    [TestMethod]
    public void DepthFirst_ReturnsCorrectOrder()
    {
        var root = new TreeNode<int>(1);
        var c1 = root.AddChild(2);
        var c2 = root.AddChild(3);
        c1.AddChild(4);

        var result = root.DepthFirst().Select(n => n.Value).ToList();

        CollectionAssert.AreEqual(new[] { 1, 2, 4, 3 }, result);
    }

    [TestMethod]
    public void BreadthFirst_ReturnsCorrectOrder()
    {
        var root = new TreeNode<int>(1);
        var c1 = root.AddChild(2);
        var c2 = root.AddChild(3);
        c1.AddChild(4);

        var result = root.BreadthFirst().Select(n => n.Value).ToList();

        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, result);
    }

    [TestMethod]
    public void IsDescendantOf_ReturnsCorrectResult()
    {
        var root = new TreeNode<int>(1);
        var child = root.AddChild(2);
        var grandchild = child.AddChild(3);
        Assert.IsTrue(grandchild.IsDescendantOf(root));
        Assert.IsFalse(root.IsDescendantOf(grandchild));
    }
}

[TestClass]
public class TreeNodeNonGenericTests
{
    [TestMethod]
    public void Constructor_SetsNameAndEmptyChildren()
    {
        var node = new TreeNode("root");
        Assert.AreEqual("root", node.Name);
        Assert.AreEqual(0, node.Children.Count);
    }

    [TestMethod]
    public void AddChild_AddsToChildren()
    {
        var parent = new TreeNode("parent");
        var child = new TreeNode("child");
        parent.AddChild(child);
        Assert.AreEqual(1, parent.Children.Count);
        Assert.AreSame(child, parent.Children[0]);
    }
}

[TestClass]
public class TrieTests
{
    [TestMethod]
    public void Insert_And_Search()
    {
        var trie = new Trie();
        trie.Insert("hello");
        Assert.IsTrue(trie.Search("hello"));
        Assert.IsFalse(trie.Search("hell"));
        Assert.AreEqual(1, trie.Count);
    }

    [TestMethod]
    public void Insert_Null_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Trie().Insert(null!));
    }

    [TestMethod]
    public void Search_Null_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Trie().Search(null!));
    }

    [TestMethod]
    public void StartsWith_ReturnsCorrectResult()
    {
        var trie = new Trie();
        trie.Insert("apple");
        trie.Insert("app");
        Assert.IsTrue(trie.StartsWith("app"));
        Assert.IsFalse(trie.StartsWith("xyz"));
    }

    [TestMethod]
    public void GetWordsWithPrefix_ReturnsMatchingWords()
    {
        var trie = new Trie();
        trie.Insert("cat"); trie.Insert("car"); trie.Insert("dog");
        var result = trie.GetWordsWithPrefix("ca").OrderBy(x => x).ToList();
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Contains("cat"));
        Assert.IsTrue(result.Contains("car"));
    }

    [TestMethod]
    public void Remove_ExistingWord_ReturnsTrue()
    {
        var trie = new Trie();
        trie.Insert("hello");
        Assert.IsTrue(trie.Remove("hello"));
        Assert.IsFalse(trie.Search("hello"));
        Assert.AreEqual(0, trie.Count);
    }

    [TestMethod]
    public void Remove_NonexistentWord_ReturnsFalse()
    {
        var trie = new Trie();
        Assert.IsFalse(trie.Remove("xyz"));
    }

    [TestMethod]
    public void Clear_RemovesAllWords()
    {
        var trie = new Trie();
        trie.Insert("a"); trie.Insert("b");
        trie.Clear();
        Assert.AreEqual(0, trie.Count);
    }

    [TestMethod]
    public void Insert_DuplicateWord_DoesNotIncreaseCount()
    {
        var trie = new Trie();
        trie.Insert("hello");
        trie.Insert("hello");
        Assert.AreEqual(1, trie.Count);
    }
}

[TestClass]
public class GraphTests
{
    [TestMethod]
    public void AddVertex_And_ContainsVertex()
    {
        var g = new Graph<int>();
        Assert.IsTrue(g.AddVertex(1));
        Assert.IsTrue(g.ContainsVertex(1));
        Assert.AreEqual(1, g.VertexCount);
    }

    [TestMethod]
    public void AddVertex_Duplicate_ReturnsFalse()
    {
        var g = new Graph<int>();
        g.AddVertex(1);
        Assert.IsFalse(g.AddVertex(1));
    }

    [TestMethod]
    public void AddEdge_CreatesVerticesAndEdge()
    {
        var g = new Graph<int>();
        Assert.IsTrue(g.AddEdge(1, 2));
        Assert.IsTrue(g.HasEdge(1, 2));
        Assert.IsFalse(g.HasEdge(2, 1));
        Assert.AreEqual(2, g.VertexCount);
        Assert.AreEqual(1, g.EdgeCount);
    }

    [TestMethod]
    public void RemoveVertex_RemovesVertexAndEdges()
    {
        var g = new Graph<int>();
        g.AddEdge(1, 2); g.AddEdge(2, 3); g.AddEdge(3, 1);
        Assert.IsTrue(g.RemoveVertex(2));
        Assert.IsFalse(g.ContainsVertex(2));
        Assert.IsFalse(g.HasEdge(1, 2));
    }

    [TestMethod]
    public void RemoveEdge_RemovesEdge()
    {
        var g = new Graph<int>();
        g.AddEdge(1, 2);
        Assert.IsTrue(g.RemoveEdge(1, 2));
        Assert.IsFalse(g.HasEdge(1, 2));
    }

    [TestMethod]
    public void Neighbors_ReturnsAdjacentVertices()
    {
        var g = new Graph<int>();
        g.AddEdge(1, 2); g.AddEdge(1, 3);
        var neighbors = g.Neighbors(1).ToList();
        Assert.AreEqual(2, neighbors.Count);
        Assert.IsTrue(neighbors.Contains(2));
        Assert.IsTrue(neighbors.Contains(3));
    }

    [TestMethod]
    public void Neighbors_NonexistentVertex_Throws()
    {
        Assert.ThrowsExactly<KeyNotFoundException>(() => new Graph<int>().Neighbors(99).ToList());
    }

    [TestMethod]
    public void DepthFirst_TraversesCorrectly()
    {
        var g = new Graph<int>();
        g.AddEdge(1, 2); g.AddEdge(1, 3); g.AddEdge(2, 4);
        var result = g.DepthFirst(1).ToList();
        Assert.IsTrue(result.Contains(1));
        Assert.IsTrue(result.Contains(2));
        Assert.IsTrue(result.Contains(4));
    }

    [TestMethod]
    public void BreadthFirst_TraversesCorrectly()
    {
        var g = new Graph<int>();
        g.AddEdge(1, 2); g.AddEdge(1, 3); g.AddEdge(2, 4);
        var result = g.BreadthFirst(1).ToList();
        Assert.AreEqual(1, result[0]);
        Assert.IsTrue(result.Contains(2));
        Assert.IsTrue(result.Contains(3));
        Assert.IsTrue(result.Contains(4));
    }

    [TestMethod]
    public void Clear_RemovesAll()
    {
        var g = new Graph<int>();
        g.AddEdge(1, 2);
        g.Clear();
        Assert.AreEqual(0, g.VertexCount);
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Graph<int>(null!));
    }
}

[TestClass]
public class PriorityBucketTests
{
    [TestMethod]
    public void Enqueue_And_Dequeue_ReturnsInPriorityOrder()
    {
        var pq = new PriorityBucket<string, int>();
        pq.Enqueue("low", 3);
        pq.Enqueue("high", 1);
        pq.Enqueue("mid", 2);
        Assert.AreEqual("high", pq.Dequeue());
        Assert.AreEqual("mid", pq.Dequeue());
    }

    [TestMethod]
    public void Peek_ReturnsHighestPriority()
    {
        var pq = new PriorityBucket<string, int>();
        pq.Enqueue("a", 2);
        pq.Enqueue("b", 1);
        Assert.AreEqual("b", pq.Peek());
        Assert.AreEqual(2, pq.Count);
    }

    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        var pq = new PriorityBucket<string, int>();
        pq.Enqueue("test", 1);
        Assert.IsTrue(pq.Contains("test"));
        Assert.IsFalse(pq.Contains("other"));
    }

    [TestMethod]
    public void TryDequeue_EmptyQueue_ReturnsFalse()
    {
        var pq = new PriorityBucket<string, int>();
        Assert.IsFalse(pq.TryDequeue(out _, out _));
    }

    [TestMethod]
    public void TryPeek_EmptyQueue_ReturnsFalse()
    {
        var pq = new PriorityBucket<string, int>();
        Assert.IsFalse(pq.TryPeek(out _, out _));
    }

    [TestMethod]
    public void Clear_RemovesAllElements()
    {
        var pq = new PriorityBucket<string, int>();
        pq.Enqueue("a", 1);
        pq.Clear();
        Assert.AreEqual(0, pq.Count);
        Assert.IsTrue(pq.IsEmpty);
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new PriorityBucket<string, int>(null!));
    }
}

[TestClass]
public class LruCacheTests
{
    [TestMethod]
    public void AddOrUpdate_StoresValue()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        Assert.IsTrue(cache.TryGetValue("a", out int val));
        Assert.AreEqual(1, val);
    }

    [TestMethod]
    public void AddOrUpdate_EvictsLRU()
    {
        var cache = new LruCache<string, int>(2);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);
        cache.AddOrUpdate("c", 3);
        Assert.IsFalse(cache.ContainsKey("a"));
        Assert.IsTrue(cache.ContainsKey("b"));
        Assert.IsTrue(cache.ContainsKey("c"));
    }

    [TestMethod]
    public void TryGetValue_PromotesToMRU()
    {
        var cache = new LruCache<string, int>(2);
        cache.AddOrUpdate("a", 1);
        cache.AddOrUpdate("b", 2);
        cache.TryGetValue("a", out _); // promotes "a"
        cache.AddOrUpdate("c", 3);     // evicts "b"
        Assert.IsTrue(cache.ContainsKey("a"));
        Assert.IsFalse(cache.ContainsKey("b"));
    }

    [TestMethod]
    public void Indexer_Get_ThrowsWhenNotFound()
    {
        var cache = new LruCache<string, int>(2);
        Assert.ThrowsExactly<KeyNotFoundException>(() => _ = cache["missing"]);
    }

    [TestMethod]
    public void Indexer_Set_BehavesLikeAddOrUpdate()
    {
        var cache = new LruCache<string, int>(2);
        cache["key"] = 42;
        Assert.AreEqual(42, cache["key"]);
    }

    [TestMethod]
    public void Remove_ExistingKey_ReturnsTrue()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        Assert.IsTrue(cache.Remove("a"));
        Assert.AreEqual(0, cache.Count);
    }

    [TestMethod]
    public void Clear_RemovesAll()
    {
        var cache = new LruCache<string, int>(3);
        cache.AddOrUpdate("a", 1);
        cache.Clear();
        Assert.AreEqual(0, cache.Count);
    }

    [TestMethod]
    public void Constructor_ZeroCapacity_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LruCache<string, int>(0));
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new LruCache<string, int>(1, null!));
    }
}

[TestClass]
public class MultimapTests
{
    [TestMethod]
    public void Add_StoresValueUnderKey()
    {
        var mm = new Multimap<string, int>();
        mm.Add("key", 1);
        mm.Add("key", 2);
        Assert.AreEqual(1, mm.KeyCount);
        Assert.AreEqual(2, mm.ValueCount);
    }

    [TestMethod]
    public void AddRange_StoresMultipleValues()
    {
        var mm = new Multimap<string, int>();
        mm.AddRange("key", [1, 2, 3]);
        Assert.AreEqual(3, mm.ValueCount);
    }

    [TestMethod]
    public void AddRange_NullValues_Throws()
    {
        var mm = new Multimap<string, int>();
        Assert.ThrowsExactly<ArgumentNullException>(() => mm.AddRange("key", null!));
    }

    [TestMethod]
    public void Indexer_ReturnsValuesForKey()
    {
        var mm = new Multimap<string, int>();
        mm.Add("x", 1); mm.Add("x", 2);
        var vals = mm["x"];
        Assert.AreEqual(2, vals.Count);
    }

    [TestMethod]
    public void Remove_SpecificValue_ReturnsTrue()
    {
        var mm = new Multimap<string, int>();
        mm.Add("key", 1); mm.Add("key", 2);
        Assert.IsTrue(mm.Remove("key", 1));
        Assert.AreEqual(1, mm.ValueCount);
    }

    [TestMethod]
    public void Remove_LastValue_RemovesKey()
    {
        var mm = new Multimap<string, int>();
        mm.Add("key", 1);
        mm.Remove("key", 1);
        Assert.IsFalse(mm.ContainsKey("key"));
    }

    [TestMethod]
    public void RemoveAll_RemovesKey()
    {
        var mm = new Multimap<string, int>();
        mm.Add("key", 1); mm.Add("key", 2);
        Assert.IsTrue(mm.RemoveAll("key"));
        Assert.AreEqual(0, mm.KeyCount);
    }

    [TestMethod]
    public void TryGetValues_ReturnsCorrectResult()
    {
        var mm = new Multimap<string, int>();
        mm.Add("k", 42);
        Assert.IsTrue(mm.TryGetValues("k", out var vals));
        Assert.AreEqual(1, vals!.Count);
        Assert.IsFalse(mm.TryGetValues("missing", out _));
    }

    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        var mm = new Multimap<string, int>();
        mm.Add("k", 42);
        Assert.IsTrue(mm.Contains("k", 42));
        Assert.IsFalse(mm.Contains("k", 99));
    }

    [TestMethod]
    public void Clear_RemovesAll()
    {
        var mm = new Multimap<string, int>();
        mm.Add("a", 1);
        mm.Clear();
        Assert.AreEqual(0, mm.KeyCount);
    }

    [TestMethod]
    public void Constructor_NullComparer_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Multimap<string, int>(null!));
    }
}

[TestClass]
public class IntervalTests
{
    [TestMethod]
    public void Constructor_NormalizesOrder()
    {
        var interval = new Interval<int>(10, 5);
        Assert.AreEqual(5, interval.Start);
        Assert.AreEqual(10, interval.End);
    }

    [TestMethod]
    public void Contains_ValueInRange_ReturnsTrue()
    {
        var interval = new Interval<int>(1, 10);
        Assert.IsTrue(interval.Contains(5));
        Assert.IsTrue(interval.Contains(1));
        Assert.IsTrue(interval.Contains(10));
        Assert.IsFalse(interval.Contains(0));
        Assert.IsFalse(interval.Contains(11));
    }

    [TestMethod]
    public void Overlaps_OverlappingIntervals_ReturnsTrue()
    {
        var a = new Interval<int>(1, 5);
        var b = new Interval<int>(3, 8);
        Assert.IsTrue(a.Overlaps(b));
    }

    [TestMethod]
    public void Overlaps_NonOverlapping_ReturnsFalse()
    {
        var a = new Interval<int>(1, 3);
        var b = new Interval<int>(5, 8);
        Assert.IsFalse(a.Overlaps(b));
    }

    [TestMethod]
    public void Intersect_OverlappingIntervals_ReturnsIntersection()
    {
        var a = new Interval<int>(1, 5);
        var b = new Interval<int>(3, 8);
        var result = a.Intersect(b);
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Value.Start);
        Assert.AreEqual(5, result.Value.End);
    }

    [TestMethod]
    public void Intersect_NonOverlapping_ReturnsNull()
    {
        var a = new Interval<int>(1, 3);
        var b = new Interval<int>(5, 8);
        Assert.IsNull(a.Intersect(b));
    }

    [TestMethod]
    public void Union_ReturnsSmallestCoveringInterval()
    {
        var a = new Interval<int>(1, 5);
        var b = new Interval<int>(3, 8);
        var result = a.Union(b);
        Assert.AreEqual(1, result.Start);
        Assert.AreEqual(8, result.End);
    }

    [TestMethod]
    public void Equals_SameInterval_ReturnsTrue()
    {
        var a = new Interval<int>(1, 5);
        var b = new Interval<int>(1, 5);
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void Equals_DifferentInterval_ReturnsFalse()
    {
        var a = new Interval<int>(1, 5);
        var b = new Interval<int>(1, 6);
        Assert.IsFalse(a.Equals(b));
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void ToString_ReturnsCorrectFormat()
    {
        var interval = new Interval<int>(1, 5);
        Assert.AreEqual("[1, 5]", interval.ToString());
    }

    [TestMethod]
    public void GetHashCode_EqualIntervals_SameHash()
    {
        var a = new Interval<int>(1, 5);
        var b = new Interval<int>(1, 5);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}
