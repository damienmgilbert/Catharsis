using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class EventMetadataTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullEventType_Throws() { Assert.ThrowsExactly<ArgumentNullException>(() => new EventMetadata("E", null!, typeof(object))); }
    [TestMethod]
    public void Constructor_NullName_Throws() { Assert.ThrowsExactly<ArgumentNullException>(() => new EventMetadata(null!, typeof(EventHandler), typeof(object))); }
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        EventMetadata meta = new EventMetadata("Click", typeof(EventHandler), typeof(object));
        Assert.AreEqual("Click", meta.Name);
        Assert.AreEqual(typeof(EventHandler), meta.EventType);
        Assert.AreEqual(typeof(object), meta.ComponentType);
        Assert.IsTrue(meta.IsMulticast);
    }

    [TestMethod]
    public void ToString_ReturnsNameAndType()
    {
        EventMetadata meta = new EventMetadata("Click", typeof(EventHandler), typeof(object));
        Assert.AreEqual("Click (EventHandler)", meta.ToString());
    }
    #endregion
}
