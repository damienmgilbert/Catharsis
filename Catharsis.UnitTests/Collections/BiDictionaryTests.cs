using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="BiDictionary{TLeft, TRight}"/> class.
///</summary>
[TestClass]
public class BiDictionaryTests
{
    #region Add

    [TestMethod]
    public void Add_NullLeftOrRight_Throws()
    {
        BiDictionary<string, string> dictionary = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => dictionary.Add(null!, "a"));
        Assert.ThrowsExactly<ArgumentNullException>(() => dictionary.Add("a", null!));
    }

    [TestMethod]
    public void Add_DuplicateLeft_Throws()
    {
        BiDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        Assert.ThrowsExactly<ArgumentException>(() => dictionary.Add("a", 2));
    }

    [TestMethod]
    public void Add_DuplicateRight_Throws()
    {
        BiDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        Assert.ThrowsExactly<ArgumentException>(() => dictionary.Add("b", 1));
    }

    [TestMethod]
    public void Add_IncrementsCount()
    {
        BiDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        dictionary.Add("b", 2);
        Assert.AreEqual(2, dictionary.Count);
    }

    #endregion

    #region TryGetByLeft / TryGetByRight

    [TestMethod]
    public void TryGetByLeft_ExistingValue_ReturnsTrueWithRight()
    {
        BiDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        Assert.IsTrue(dictionary.TryGetByLeft("a", out int right));
        Assert.AreEqual(1, right);
    }

    [TestMethod]
    public void TryGetByRight_ExistingValue_ReturnsTrueWithLeft()
    {
        BiDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        Assert.IsTrue(dictionary.TryGetByRight(1, out string? left));
        Assert.AreEqual("a", left);
    }

    [TestMethod]
    public void TryGetByLeft_MissingValue_ReturnsFalse()
    {
        BiDictionary<string, int> dictionary = new();
        Assert.IsFalse(dictionary.TryGetByLeft("missing", out _));
    }

    #endregion

    #region ContainsLeft / ContainsRight

    [TestMethod]
    public void ContainsLeft_And_ContainsRight_ReflectPresence()
    {
        BiDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        Assert.IsTrue(dictionary.ContainsLeft("a"));
        Assert.IsTrue(dictionary.ContainsRight(1));
        Assert.IsFalse(dictionary.ContainsLeft("b"));
        Assert.IsFalse(dictionary.ContainsRight(2));
    }

    #endregion

    #region RemoveByLeft / RemoveByRight

    [TestMethod]
    public void RemoveByLeft_ExistingValue_RemovesBothDirections()
    {
        BiDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);

        Assert.IsTrue(dictionary.RemoveByLeft("a"));
        Assert.IsFalse(dictionary.ContainsLeft("a"));
        Assert.IsFalse(dictionary.ContainsRight(1));
    }

    [TestMethod]
    public void RemoveByRight_ExistingValue_RemovesBothDirections()
    {
        BiDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);

        Assert.IsTrue(dictionary.RemoveByRight(1));
        Assert.IsFalse(dictionary.ContainsLeft("a"));
        Assert.IsFalse(dictionary.ContainsRight(1));
    }

    [TestMethod]
    public void RemoveByLeft_MissingValue_ReturnsFalse()
    {
        BiDictionary<string, int> dictionary = new();
        Assert.IsFalse(dictionary.RemoveByLeft("missing"));
    }

    #endregion

    #region Clear

    [TestMethod]
    public void Clear_RemovesEverything()
    {
        BiDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        dictionary.Add("b", 2);
        dictionary.Clear();

        Assert.AreEqual(0, dictionary.Count);
        Assert.IsFalse(dictionary.ContainsLeft("a"));
    }

    #endregion

    #region Reuse after removal

    [TestMethod]
    public void RemoveByLeft_ThenAddWithSameValues_Succeeds()
    {
        BiDictionary<string, int> dictionary = new();
        dictionary.Add("a", 1);
        dictionary.RemoveByLeft("a");
        dictionary.Add("a", 1);

        Assert.IsTrue(dictionary.TryGetByLeft("a", out int right));
        Assert.AreEqual(1, right);
    }

    #endregion
}
