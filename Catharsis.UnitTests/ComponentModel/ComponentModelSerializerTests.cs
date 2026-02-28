using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public sealed class ComponentModelSerializerTests
{
    #region Public methods
    [TestMethod]
    public void Serialize_NullComponent_ThrowsArgumentNullException()
    {
        ComponentModelSerializer serializer = new ComponentModelSerializer();

        Assert.ThrowsExactly<ArgumentNullException>(() => serializer.Serialize(null!));
    }

    [TestMethod]
    public void Serialize_NullPropertyValue_ReturnsNull()
    {
        ComponentModelSerializer serializer = new ComponentModelSerializer();
        SimpleDto dto = new SimpleDto { Name = null };

        Dictionary<string, string?> result = serializer.Serialize(dto);

        Assert.IsNull(result["Name"]);
    }

    [TestMethod]
    public void Serialize_ReturnsPropertyDictionary()
    {
        ComponentModelSerializer serializer = new ComponentModelSerializer();
        SimpleDto dto = new SimpleDto { Name = "Alice", Age = 30, IsActive = true };

        Dictionary<string, string?> result = serializer.Serialize(dto);

        Assert.AreEqual("Alice", result["Name"]);
        Assert.AreEqual("30", result["Age"]);
        Assert.AreEqual("True", result["IsActive"]);
    }

    [TestMethod]
    public void Serialize_SkipDefaultValues_OmitsDefaults()
    {
        ComponentModelSerializer serializer = new ComponentModelSerializer { SkipDefaultValues = true };
        SimpleDto dto = new SimpleDto { Name = "Alice" };

        Dictionary<string, string?> result = serializer.Serialize(dto);

        Assert.IsTrue(result.ContainsKey("Name"));
    }

    [TestMethod]
    public void Serialize_WithPropertyFilter_FiltersProperties()
    {
        ComponentModelSerializer serializer = new ComponentModelSerializer { PropertyFilter = p => p.Name == "Name" };
        SimpleDto dto = new SimpleDto { Name = "Alice", Age = 30 };

        Dictionary<string, string?> result = serializer.Serialize(dto);

        Assert.IsTrue(result.ContainsKey("Name"));
        Assert.IsFalse(result.ContainsKey("Age"));
    }

    [TestMethod]
    public void SerializeRaw_NullComponent_ThrowsArgumentNullException()
    {
        ComponentModelSerializer serializer = new ComponentModelSerializer();

        Assert.ThrowsExactly<ArgumentNullException>(() => serializer.SerializeRaw(null!));
    }

    [TestMethod]
    public void SerializeRaw_ReturnsTypedValues()
    {
        ComponentModelSerializer serializer = new ComponentModelSerializer();
        SimpleDto dto = new SimpleDto { Name = "Alice", Age = 30, IsActive = true };

        Dictionary<string, object?> result = serializer.SerializeRaw(dto);

        Assert.AreEqual("Alice", result["Name"]);
        Assert.AreEqual(30, result["Age"]);
        Assert.IsTrue((bool?)result["IsActive"]);
    }
    #endregion

    sealed class SimpleDto
    {
        #region Public properties
        public int Age { get; set; }

        public bool IsActive { get; set; }

        public string? Name { get; set; }
        #endregion
    }
}
