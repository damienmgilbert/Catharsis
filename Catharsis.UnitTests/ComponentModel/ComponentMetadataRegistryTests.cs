using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class ComponentMetadataRegistryTests
{
    #region Public methods
    [TestMethod]
    public void GetProperties_UnregisteredType_ReturnsEmpty()
    {
        ComponentMetadataRegistry registry = new ComponentMetadataRegistry();
        IReadOnlyList<PropertyMetadata> props = registry.GetProperties(typeof(string));
        Assert.AreEqual(0, props.Count);
    }

    [TestMethod]
    public void RegisterEvent_And_GetEvents()
    {
        ComponentMetadataRegistry registry = new ComponentMetadataRegistry();
        EventMetadata meta = new EventMetadata("Click", typeof(EventHandler), typeof(object));
        registry.RegisterEvent(typeof(object), meta);

        IReadOnlyList<EventMetadata> events = registry.GetEvents(typeof(object));
        Assert.AreEqual(1, events.Count);
        Assert.AreEqual("Click", events[0].Name);
    }

    [TestMethod]
    public void RegisterProperty_And_GetProperties()
    {
        ComponentMetadataRegistry registry = new ComponentMetadataRegistry();
        PropertyMetadata meta = new PropertyMetadata("Name", typeof(string), typeof(object));
        registry.RegisterProperty(typeof(object), meta);

        IReadOnlyList<PropertyMetadata> props = registry.GetProperties(typeof(object));
        Assert.AreEqual(1, props.Count);
        Assert.AreEqual("Name", props[0].Name);
    }

    [TestMethod]
    public void RegisterProperty_DuplicateName_Throws()
    {
        ComponentMetadataRegistry registry = new ComponentMetadataRegistry();
        PropertyMetadata meta = new PropertyMetadata("Name", typeof(string), typeof(object));
        registry.RegisterProperty(typeof(object), meta);
        Assert.ThrowsExactly<ArgumentException>(() => registry.RegisterProperty(typeof(object), new PropertyMetadata("Name", typeof(int), typeof(object))));
    }
    #endregion
}
