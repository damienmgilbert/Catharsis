using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

///<summary>
///Unit tests for the <see cref="ValidatingCollection{T}"/> class.
///</summary>
[TestClass]
public class ValidatingCollectionTests
{
    [TestMethod]
    public void Add_ValidItem_Succeeds()
    {
        ValidatingCollection<int> collection = new(static x => x > 0)
        {
            1,
            2
        };
        Assert.HasCount(2, collection);
    }

    [TestMethod]
    public void Add_InvalidItem_Throws()
    {
        ValidatingCollection<int> collection = new(static x => x > 0);
        Assert.ThrowsExactly<ArgumentException>(() => collection.Add(-1));
    }

    [TestMethod]
    public void Insert_ValidItem_Succeeds()
    {
        ValidatingCollection<int> collection = new(static x => x > 0)
        {
            1,
            3
        };
        collection.Insert(1, 2);
        Assert.AreEqual(2, collection[1]);
    }

    [TestMethod]
    public void Insert_InvalidItem_Throws()
    {
        ValidatingCollection<int> collection = new(static x => x > 0)
        {
            1
        };
        Assert.ThrowsExactly<ArgumentException>(() => collection.Insert(0, 0));
    }

    [TestMethod]
    public void SetItem_ValidReplacement_Succeeds()
    {
        ValidatingCollection<int> collection = new(static x => x > 0)
        {
            1
        };
        collection[0] = 5;
        Assert.AreEqual(5, collection[0]);
    }

    [TestMethod]
    public void SetItem_InvalidReplacement_Throws()
    {
        ValidatingCollection<int> collection = new(static x => x > 0)
        {
            1
        };
        Assert.ThrowsExactly<ArgumentException>(() => collection[0] = -1);
    }

    [TestMethod]
    public void Constructor_NullValidator_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => new ValidatingCollection<int>(null!));
    }

    [TestMethod]
    public void Clear_RemovesAllItems()
    {
        ValidatingCollection<string> collection = new(static s => !string.IsNullOrEmpty(s))
        {
            "a",
            "b"
        };
        collection.Clear();
        Assert.IsEmpty(collection);
    }

    [TestMethod]
    public void Remove_ExistingItem_ReturnsTrue()
    {
        ValidatingCollection<int> collection = new(static x => x > 0)
        {
            1
        };
        Assert.IsTrue(collection.Remove(1));
        Assert.IsEmpty(collection);
    }

    [TestMethod]
    public void Contains_ExistingItem_ReturnsTrue()
    {
        ValidatingCollection<int> collection = new(static x => x > 0)
        {
            42
        };
        Assert.Contains(42, collection);
    }

    [TestMethod]
    public void StringValidation_RejectsEmptyStrings()
    {
        ValidatingCollection<string> collection = new(static s => !string.IsNullOrWhiteSpace(s))
        {
            "hello"
        };
        Assert.ThrowsExactly<ArgumentException>(() => collection.Add(""));
        Assert.ThrowsExactly<ArgumentException>(() => collection.Add("   "));
    }
}
