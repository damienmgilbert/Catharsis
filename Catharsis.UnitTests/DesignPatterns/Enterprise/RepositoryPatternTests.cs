using Catharsis.DesignPatterns.Enterprise;

namespace Catharsis.UnitTests.DesignPatterns.Enterprise;

///<summary>
///Unit tests for the <see cref="RepositoryPattern{TEntity, TKey}"/> class.
///</summary>
[TestClass]
public class RepositoryPatternTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullKeySelector_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new RepositoryPattern<string, int>(null!)); }

    #endregion

    #region Add / TryGet / Remove

    [TestMethod]
    public void Add_NullEntity_Throws()
    {
        RepositoryPattern<string, int> repository = new(static s => s.Length);
        Assert.ThrowsExactly<ArgumentNullException>(() => repository.Add(null!));
    }

    [TestMethod]
    public void Add_ThenTryGet_ReturnsEntity()
    {
        RepositoryPattern<string, int> repository = new(static s => s.Length);
        repository.Add("abc");

        Assert.IsTrue(repository.TryGet(3, out string entity));
        Assert.AreEqual("abc", entity);
    }

    [TestMethod]
    public void Add_SameKeyTwice_ReplacesEntity()
    {
        RepositoryPattern<string, int> repository = new(static s => s.Length);
        repository.Add("abc");
        repository.Add("xyz");

        Assert.AreEqual(1, repository.Count);
        Assert.IsTrue(repository.TryGet(3, out string entity));
        Assert.AreEqual("xyz", entity);
    }

    [TestMethod]
    public void TryGet_MissingKey_ReturnsFalse()
    {
        RepositoryPattern<string, int> repository = new(static s => s.Length);
        Assert.IsFalse(repository.TryGet(99, out _));
    }

    [TestMethod]
    public void Remove_ExistingKey_ReturnsTrueAndRemoves()
    {
        RepositoryPattern<string, int> repository = new(static s => s.Length);
        repository.Add("abc");

        Assert.IsTrue(repository.Remove(3));
        Assert.IsFalse(repository.TryGet(3, out _));
    }

    [TestMethod]
    public void Remove_MissingKey_ReturnsFalse()
    {
        RepositoryPattern<string, int> repository = new(static s => s.Length);
        Assert.IsFalse(repository.Remove(99));
    }

    #endregion

    #region GetAll / Count

    [TestMethod]
    public void GetAll_ReturnsAllEntities()
    {
        RepositoryPattern<string, int> repository = new(static s => s.GetHashCode());
        repository.Add("abc");
        repository.Add("defg");

        IReadOnlyCollection<string> all = repository.GetAll();

        Assert.HasCount(2, all);
        CollectionAssert.Contains(all.ToList(), "abc");
        CollectionAssert.Contains(all.ToList(), "defg");
    }

    [TestMethod]
    public void Count_ReflectsNumberOfEntities()
    {
        RepositoryPattern<string, int> repository = new(static s => s.GetHashCode());
        repository.Add("abc");
        repository.Add("defg");

        Assert.AreEqual(2, repository.Count);
    }

    #endregion
}
