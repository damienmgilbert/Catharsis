using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="PropertyMetadata"/> class.
///</summary>
[TestClass]
public class PropertyMetadataTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullName_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new PropertyMetadata(null!, typeof(string), typeof(object))); }
    [TestMethod]
    public void Constructor_NullType_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new PropertyMetadata("X", null!, typeof(object))); }
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        PropertyMetadata meta = new PropertyMetadata("Name", typeof(string), typeof(object));
        Assert.AreEqual("Name", meta.Name);
        Assert.AreEqual(typeof(string), meta.PropertyType);
        Assert.AreEqual(typeof(object), meta.ComponentType);
        Assert.IsFalse(meta.IsReadOnly);
        Assert.IsNull(meta.DefaultValue);
    }

    [TestMethod]
    public void Constructor_WithDefaults()
    {
        PropertyMetadata meta = new PropertyMetadata("Age", typeof(int), typeof(object), isReadOnly: true, defaultValue: 25);
        Assert.IsTrue(meta.IsReadOnly);
        Assert.AreEqual(25, meta.DefaultValue);
    }
    #endregion
}
