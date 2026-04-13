using System.ComponentModel;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ComponentReflectionCache"/> class.
///</summary>
[TestClass]
public class ComponentReflectionCacheTests
{
    #region Public methods
    [TestMethod]
    public void GetEvents_ReturnsCachedCollection()
    {
        ComponentReflectionCache cache = new();
        EventDescriptorCollection events1 = cache.GetEvents(typeof(Component));
        EventDescriptorCollection events2 = cache.GetEvents(typeof(Component));
        Assert.AreSame(events1, events2);
    }

    [TestMethod]
    public void GetProperties_NullType_Throws()
    {
        ComponentReflectionCache cache = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => cache.GetProperties(null!));
    }

    [TestMethod]
    public void GetProperties_ReturnsCachedCollection()
    {
        ComponentReflectionCache cache = new();
        PropertyDescriptorCollection props1 = cache.GetProperties(typeof(string));
        PropertyDescriptorCollection props2 = cache.GetProperties(typeof(string));
        Assert.AreSame(props1, props2);
    }
    #endregion
}
