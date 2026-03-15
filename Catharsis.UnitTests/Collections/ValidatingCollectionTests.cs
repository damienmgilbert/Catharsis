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
        ValidatingCollection<int> collection = new ValidatingCollection<int>(static x => x > 0);
        collection.Add(1);
        collection.Add(2);
        Assert.HasCount(2, collection);
    }

    [TestMethod]
    public void Add_InvalidItem_Throws()
    {
        ValidatingCollection<int> collection = new ValidatingCollection<int>(static x => x > 0);
        Assert.ThrowsExactly<ArgumentException>(() => collection.Add(-1));
    }

    [TestMethod]
    public void Insert_ValidItem_Succeeds()
    {
        ValidatingCollection<int> collection = new ValidatingCollection<int>(static x => x > 0);
        collection.Add(1);
        collection.Add(3);
        collection.Insert(1, 2);
        Assert.AreEqual(2, collection[1]);
    }

    [TestMethod]
    public void Insert_InvalidItem_Throws()
    {
        ValidatingCollection<int> collection = new ValidatingCollection<int>(static x => x > 0);
        collection.Add(1);
        Assert.ThrowsExactly<ArgumentException>(() => collection.Insert(0, 0));
    }

    [TestMethod]
    public void SetItem_ValidReplacement_Succeeds()
    {
        ValidatingCollection<int> collection = new ValidatingCollection<int>(static x => x > 0);
        collection.Add(1);
        collection[0] = 5;
        Assert.AreEqual(5, collection[0]);
    }

    [TestMethod]
    public void SetItem_InvalidReplacement_Throws()
    {
        ValidatingCollection<int> collection = new ValidatingCollection<int>(static x => x > 0);
        collection.Add(1);
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
        ValidatingCollection<string> collection = new ValidatingCollection<string>(static s => !string.IsNullOrEmpty(s));
        collection.Add("a");
        collection.Add("b");
        collection.Clear();
        Assert.IsEmpty(collection);
    }

    [TestMethod]
    public void Remove_ExistingItem_ReturnsTrue()
    {
        ValidatingCollection<int> collection = new ValidatingCollection<int>(static x => x > 0);
        collection.Add(1);
        Assert.IsTrue(collection.Remove(1));
        Assert.IsEmpty(collection);
    }

    [TestMethod]
    public void Contains_ExistingItem_ReturnsTrue()
    {
        ValidatingCollection<int> collection = new ValidatingCollection<int>(static x => x > 0);
        collection.Add(42);
        Assert.IsTrue(collection.Contains(42));
    }

    [TestMethod]
    public void StringValidation_RejectsEmptyStrings()
    {
        ValidatingCollection<string> collection = new ValidatingCollection<string>(static s => !string.IsNullOrWhiteSpace(s));
        collection.Add("hello");
        Assert.ThrowsExactly<ArgumentException>(() => collection.Add(""));
        Assert.ThrowsExactly<ArgumentException>(() => collection.Add("   "));
    }
}
