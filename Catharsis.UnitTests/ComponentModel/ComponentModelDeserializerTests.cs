using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public sealed class ComponentModelDeserializerTests
{
    #region Public methods
    [TestMethod]
    public void Deserialize_CreateNewInstance()
    {
        ComponentModelDeserializer deserializer = new ComponentModelDeserializer();
        Dictionary<string, string?> data = new Dictionary<string, string?> { ["Name"] = "Bob", ["Age"] = "25" };

        SimpleDto result = deserializer.Deserialize<SimpleDto>(data);

        Assert.AreEqual("Bob", result.Name);
        Assert.AreEqual(25, result.Age);
    }

    [TestMethod]
    public void Deserialize_MissingProperty_IgnoredByDefault()
    {
        ComponentModelDeserializer deserializer = new ComponentModelDeserializer();
        Dictionary<string, string?> data = new Dictionary<string, string?> { ["NonExistent"] = "value" };

        SimpleDto result = deserializer.Deserialize(data, new SimpleDto());

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Deserialize_MissingProperty_StrictMode_ThrowsInvalidOperationException()
    {
        ComponentModelDeserializer deserializer = new ComponentModelDeserializer { IgnoreMissingProperties = false };
        Dictionary<string, string?> data = new Dictionary<string, string?> { ["NonExistent"] = "value" };

        Assert.ThrowsExactly<InvalidOperationException>(() => deserializer.Deserialize(data, new SimpleDto()));
    }

    [TestMethod]
    public void Deserialize_NullData_ThrowsArgumentNullException()
    {
        ComponentModelDeserializer deserializer = new ComponentModelDeserializer();

        Assert.ThrowsExactly<ArgumentNullException>(() => deserializer.Deserialize<SimpleDto>(null!, new SimpleDto()));
    }

    [TestMethod]
    public void Deserialize_NullTarget_ThrowsArgumentNullException()
    {
        ComponentModelDeserializer deserializer = new ComponentModelDeserializer();
        Dictionary<string, string?> data = new Dictionary<string, string?>();

        Assert.ThrowsExactly<ArgumentNullException>(() => deserializer.Deserialize(data, (SimpleDto)null!));
    }

    [TestMethod]
    public void Deserialize_NullValue_SetsNull()
    {
        ComponentModelDeserializer deserializer = new ComponentModelDeserializer();
        Dictionary<string, string?> data = new Dictionary<string, string?> { ["Name"] = null };
        SimpleDto target = new SimpleDto { Name = "Alice" };

        deserializer.Deserialize(data, target);

        Assert.IsNull(target.Name);
    }

    [TestMethod]
    public void Deserialize_PopulatesTarget()
    {
        ComponentModelDeserializer deserializer = new ComponentModelDeserializer();
        Dictionary<string, string?> data = new Dictionary<string, string?> { ["Name"] = "Alice", ["Age"] = "30", ["IsActive"] = "True" };
        SimpleDto target = new SimpleDto();

        SimpleDto result = deserializer.Deserialize(data, target);

        Assert.AreSame(target, result);
        Assert.AreEqual("Alice", target.Name);
        Assert.AreEqual(30, target.Age);
        Assert.IsTrue(target.IsActive);
    }

    [TestMethod]
    public void DeserializeRaw_NullData_ThrowsArgumentNullException()
    {
        ComponentModelDeserializer deserializer = new ComponentModelDeserializer();

        Assert.ThrowsExactly<ArgumentNullException>(() => deserializer.DeserializeRaw<SimpleDto>(null!, new SimpleDto()));
    }

    [TestMethod]
    public void DeserializeRaw_PopulatesTarget()
    {
        ComponentModelDeserializer deserializer = new ComponentModelDeserializer();
        Dictionary<string, object?> data = new Dictionary<string, object?> { ["Name"] = "Alice", ["Age"] = 30, ["IsActive"] = true };
        SimpleDto target = new SimpleDto();

        deserializer.DeserializeRaw(data, target);

        Assert.AreEqual("Alice", target.Name);
        Assert.AreEqual(30, target.Age);
        Assert.IsTrue(target.IsActive);
    }

    [TestMethod]
    public void RoundTrip_SerializeDeserialize()
    {
        ComponentModelSerializer serializer = new ComponentModelSerializer();
        ComponentModelDeserializer deserializer = new ComponentModelDeserializer();
        SimpleDto original = new SimpleDto { Name = "Alice", Age = 30, IsActive = true };

        Dictionary<string, string?> data = serializer.Serialize(original);
        SimpleDto restored = deserializer.Deserialize<SimpleDto>(data);

        Assert.AreEqual(original.Name, restored.Name);
        Assert.AreEqual(original.Age, restored.Age);
        Assert.AreEqual(original.IsActive, restored.IsActive);
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
