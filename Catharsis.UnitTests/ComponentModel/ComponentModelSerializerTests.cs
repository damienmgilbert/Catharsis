using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public sealed class ComponentModelSerializerTests
{
    private sealed class SimpleDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    [TestMethod]
    public void Serialize_NullComponent_ThrowsArgumentNullException()
    {
        var serializer = new ComponentModelSerializer();

        Assert.ThrowsExactly<ArgumentNullException>(() => serializer.Serialize(null!));
    }

    [TestMethod]
    public void Serialize_ReturnsPropertyDictionary()
    {
        var serializer = new ComponentModelSerializer();
        var dto = new SimpleDto { Name = "Alice", Age = 30, IsActive = true };

        var result = serializer.Serialize(dto);

        Assert.AreEqual("Alice", result["Name"]);
        Assert.AreEqual("30", result["Age"]);
        Assert.AreEqual("True", result["IsActive"]);
    }

    [TestMethod]
    public void Serialize_NullPropertyValue_ReturnsNull()
    {
        var serializer = new ComponentModelSerializer();
        var dto = new SimpleDto { Name = null };

        var result = serializer.Serialize(dto);

        Assert.IsNull(result["Name"]);
    }

    [TestMethod]
    public void Serialize_WithPropertyFilter_FiltersProperties()
    {
        var serializer = new ComponentModelSerializer
        {
            PropertyFilter = p => p.Name == "Name"
        };
        var dto = new SimpleDto { Name = "Alice", Age = 30 };

        var result = serializer.Serialize(dto);

        Assert.IsTrue(result.ContainsKey("Name"));
        Assert.IsFalse(result.ContainsKey("Age"));
    }

    [TestMethod]
    public void SerializeRaw_NullComponent_ThrowsArgumentNullException()
    {
        var serializer = new ComponentModelSerializer();

        Assert.ThrowsExactly<ArgumentNullException>(() => serializer.SerializeRaw(null!));
    }

    [TestMethod]
    public void SerializeRaw_ReturnsTypedValues()
    {
        var serializer = new ComponentModelSerializer();
        var dto = new SimpleDto { Name = "Alice", Age = 30, IsActive = true };

        var result = serializer.SerializeRaw(dto);

        Assert.AreEqual("Alice", result["Name"]);
        Assert.AreEqual(30, result["Age"]);
        Assert.AreEqual(true, result["IsActive"]);
    }

    [TestMethod]
    public void Serialize_SkipDefaultValues_OmitsDefaults()
    {
        var serializer = new ComponentModelSerializer { SkipDefaultValues = true };
        var dto = new SimpleDto { Name = "Alice" };

        var result = serializer.Serialize(dto);

        Assert.IsTrue(result.ContainsKey("Name"));
    }
}
