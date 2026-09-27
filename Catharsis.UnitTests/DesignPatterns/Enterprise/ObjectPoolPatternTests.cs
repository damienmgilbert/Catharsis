using Catharsis.DesignPatterns.Enterprise;

namespace Catharsis.UnitTests.DesignPatterns.Enterprise;

///<summary>
///Unit tests for the <see cref="ObjectPoolPattern{T}"/> class.
///</summary>
[TestClass]
public class ObjectPoolPatternTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullFactory_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ObjectPoolPattern<object>(null!)); }

    #endregion

    #region Rent / Return

    [TestMethod]
    public void Rent_EmptyPool_CreatesViaFactory()
    {
        int created = 0;
        ObjectPoolPattern<object> pool = new(() => { created++; return new object(); });

        pool.Rent();

        Assert.AreEqual(1, created);
    }

    [TestMethod]
    public void Rent_AfterReturn_ReusesItemInsteadOfFactory()
    {
        int created = 0;
        ObjectPoolPattern<object> pool = new(() => { created++; return new object(); });

        object item = pool.Rent();
        pool.Return(item);
        object rented = pool.Rent();

        Assert.AreSame(item, rented);
        Assert.AreEqual(1, created);
    }

    [TestMethod]
    public void Return_IncreasesCount()
    {
        ObjectPoolPattern<object> pool = new(static () => new object());
        pool.Return(new object());

        Assert.AreEqual(1, pool.Count);
    }

    [TestMethod]
    public void Rent_DecreasesCount()
    {
        ObjectPoolPattern<object> pool = new(static () => new object());
        pool.Return(new object());
        pool.Rent();

        Assert.AreEqual(0, pool.Count);
    }

    #endregion
}
