using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public sealed class ComponentModelDeserializerTests
{
    private sealed class SimpleDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    [TestMethod]
    public void Deserialize_NullData_ThrowsArgumentNullException()
    {
        var deserializer = new ComponentModelDeserializer();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => deserializer.Deserialize<SimpleDto>(null!, new SimpleDto()));
    }

    [TestMethod]
    public void Deserialize_NullTarget_ThrowsArgumentNullException()
    {
        var deserializer = new ComponentModelDeserializer();
        var data = new Dictionary<string, string?>();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => deserializer.Deserialize(data, (SimpleDto)null!));
    }

    [TestMethod]
    public void Deserialize_PopulatesTarget()
    {
        var deserializer = new ComponentModelDeserializer();
        var data = new Dictionary<string, string?>
        {
            ["Name"] = "Alice",
            ["Age"] = "30",
            ["IsActive"] = "True"
        };
        var target = new SimpleDto();

        var result = deserializer.Deserialize(data, target);

        Assert.AreSame(target, result);
        Assert.AreEqual("Alice", target.Name);
        Assert.AreEqual(30, target.Age);
        Assert.IsTrue(target.IsActive);
    }

    [TestMethod]
    public void Deserialize_NullValue_SetsNull()
    {
        var deserializer = new ComponentModelDeserializer();
        var data = new Dictionary<string, string?> { ["Name"] = null };
        var target = new SimpleDto { Name = "Alice" };

        deserializer.Deserialize(data, target);

        Assert.IsNull(target.Name);
    }

    [TestMethod]
    public void Deserialize_MissingProperty_IgnoredByDefault()
    {
        var deserializer = new ComponentModelDeserializer();
        var data = new Dictionary<string, string?> { ["NonExistent"] = "value" };

        var result = deserializer.Deserialize(data, new SimpleDto());

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Deserialize_MissingProperty_StrictMode_ThrowsInvalidOperationException()
    {
        var deserializer = new ComponentModelDeserializer { IgnoreMissingProperties = false };
        var data = new Dictionary<string, string?> { ["NonExistent"] = "value" };

        Assert.ThrowsExactly<InvalidOperationException>(
            () => deserializer.Deserialize(data, new SimpleDto()));
    }

    [TestMethod]
    public void Deserialize_CreateNewInstance()
    {
        var deserializer = new ComponentModelDeserializer();
        var data = new Dictionary<string, string?> { ["Name"] = "Bob", ["Age"] = "25" };

        var result = deserializer.Deserialize<SimpleDto>(data);

        Assert.AreEqual("Bob", result.Name);
        Assert.AreEqual(25, result.Age);
    }

    [TestMethod]
    public void DeserializeRaw_PopulatesTarget()
    {
        var deserializer = new ComponentModelDeserializer();
        var data = new Dictionary<string, object?>
        {
            ["Name"] = "Alice",
            ["Age"] = 30,
            ["IsActive"] = true
        };
        var target = new SimpleDto();

        deserializer.DeserializeRaw(data, target);

        Assert.AreEqual("Alice", target.Name);
        Assert.AreEqual(30, target.Age);
        Assert.IsTrue(target.IsActive);
    }

    [TestMethod]
    public void DeserializeRaw_NullData_ThrowsArgumentNullException()
    {
        var deserializer = new ComponentModelDeserializer();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => deserializer.DeserializeRaw<SimpleDto>(null!, new SimpleDto()));
    }

    [TestMethod]
    public void RoundTrip_SerializeDeserialize()
    {
        var serializer = new ComponentModelSerializer();
        var deserializer = new ComponentModelDeserializer();
        var original = new SimpleDto { Name = "Alice", Age = 30, IsActive = true };

        var data = serializer.Serialize(original);
        var restored = deserializer.Deserialize<SimpleDto>(data);

        Assert.AreEqual(original.Name, restored.Name);
        Assert.AreEqual(original.Age, restored.Age);
        Assert.AreEqual(original.IsActive, restored.IsActive);
    }
}
