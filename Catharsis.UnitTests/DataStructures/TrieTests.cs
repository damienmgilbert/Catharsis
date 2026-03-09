using Catharsis.DataStructures;

namespace Catharsis.UnitTests.DataStructures;

///<summary>
///Unit tests for the <see cref="Trie"/> class.
///</summary>
[TestClass]
public class TrieTests
{
    #region Public methods
    [TestMethod]
    public void Clear_RemovesAllWords()
    {
        Trie trie = new Trie();
        trie.Insert("a");
        trie.Insert("b");
        trie.Clear();
        Assert.AreEqual(0, trie.Count);
    }

    [TestMethod]
    public void GetWordsWithPrefix_ReturnsMatchingWords()
    {
        Trie trie = new Trie();
        trie.Insert("cat");
        trie.Insert("car");
        trie.Insert("dog");
        List<string> result = [.. trie.GetWordsWithPrefix("ca").OrderBy(static x => x)];
        Assert.HasCount(2, result);
        Assert.Contains("cat", result);
        Assert.Contains("car", result);
    }

    [TestMethod]
    public void Insert_And_Search()
    {
        Trie trie = new Trie();
        trie.Insert("hello");
        Assert.IsTrue(trie.Search("hello"));
        Assert.IsFalse(trie.Search("hell"));
        Assert.AreEqual(1, trie.Count);
    }

    [TestMethod]
    public void Insert_DuplicateWord_DoesNotIncreaseCount()
    {
        Trie trie = new Trie();
        trie.Insert("hello");
        trie.Insert("hello");
        Assert.AreEqual(1, trie.Count);
    }

    [TestMethod]
    public void Insert_Null_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new Trie().Insert(null!)); }
    [TestMethod]
    public void Remove_ExistingWord_ReturnsTrue()
    {
        Trie trie = new Trie();
        trie.Insert("hello");
        Assert.IsTrue(trie.Remove("hello"));
        Assert.IsFalse(trie.Search("hello"));
        Assert.AreEqual(0, trie.Count);
    }

    [TestMethod]
    public void Remove_NonexistentWord_ReturnsFalse()
    {
        Trie trie = new Trie();
        Assert.IsFalse(trie.Remove("xyz"));
    }

    [TestMethod]
    public void Search_Null_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new Trie().Search(null!)); }
    [TestMethod]
    public void StartsWith_ReturnsCorrectResult()
    {
        Trie trie = new Trie();
        trie.Insert("apple");
        trie.Insert("app");
        Assert.IsTrue(trie.StartsWith("app"));
        Assert.IsFalse(trie.StartsWith("xyz"));
    }
    #endregion
}
