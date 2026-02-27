using Catharsis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catharsis.UnitTests.Services;

[TestClass]
public class PooledObjectFactoryTests
{
    #region Public methods
    [TestMethod]
    public void Dispose_DisposesPooledObjects()
    {
        ILogger<PooledObjectFactory<TestObj>> logger = NullLoggerFactory.Instance.CreateLogger<PooledObjectFactory<TestObj>>();
        PooledObjectFactory<TestObj> factory = new PooledObjectFactory<TestObj>(logger);
        TestObj obj = factory.Rent();
        factory.Return(obj);

        factory.Dispose();

        Assert.IsTrue(obj.Disposed);
    }

    [TestMethod]
    public void Rent_AfterDispose_Throws()
    {
        ILogger<PooledObjectFactory<TestObj>> logger = NullLoggerFactory.Instance.CreateLogger<PooledObjectFactory<TestObj>>();
        PooledObjectFactory<TestObj> factory = new PooledObjectFactory<TestObj>(logger);
        factory.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => factory.Rent());
    }

    [TestMethod]
    public void Rent_CreatesNewObject()
    {
        ILogger<PooledObjectFactory<TestObj>> logger = NullLoggerFactory.Instance.CreateLogger<PooledObjectFactory<TestObj>>();
        using PooledObjectFactory<TestObj> factory = new PooledObjectFactory<TestObj>(logger);

        TestObj obj = factory.Rent();

        Assert.IsNotNull(obj);
        Assert.AreEqual(1, factory.TotalCreated);
    }

    [TestMethod]
    public void Return_And_Rent_ReusesObject()
    {
        ILogger<PooledObjectFactory<TestObj>> logger = NullLoggerFactory.Instance.CreateLogger<PooledObjectFactory<TestObj>>();
        using PooledObjectFactory<TestObj> factory = new PooledObjectFactory<TestObj>(logger);

        TestObj obj = factory.Rent();
        factory.Return(obj);
        Assert.AreEqual(1, factory.AvailableCount);

        TestObj obj2 = factory.Rent();
        Assert.AreSame(obj, obj2);
    }
    #endregion

    sealed class TestObj : IDisposable
    {
        #region Public methods
        public void Dispose() { Disposed = true; }
        #endregion

        #region Public properties
        public bool Disposed { get; private set; }
        #endregion
    }
}
