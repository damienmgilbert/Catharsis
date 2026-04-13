using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="ComponentModelDeserializer"/> class.
///</summary>
[TestClass]
public sealed class ComponentModelDeserializerTests
{
    #region Public methods
    [TestMethod]
    public void Deserialize_CreateNewInstance()
    {
        ComponentModelDeserializer deserializer = new();
        Dictionary<string, string?> data = new() { ["Name"] = "Bob", ["Age"] = "25" };

        SimpleDto result = deserializer.Deserialize<SimpleDto>(data);

        Assert.AreEqual("Bob", result.Name);
        Assert.AreEqual(25, result.Age);
    }

    [TestMethod]
    public void Deserialize_MissingProperty_IgnoredByDefault()
    {
        ComponentModelDeserializer deserializer = new();
        Dictionary<string, string?> data = new() { ["NonExistent"] = "value" };

        SimpleDto result = deserializer.Deserialize(data, new SimpleDto());

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Deserialize_MissingProperty_StrictMode_ThrowsInvalidOperationException()
    {
        ComponentModelDeserializer deserializer = new() { IgnoreMissingProperties = false };
        Dictionary<string, string?> data = new() { ["NonExistent"] = "value" };

        Assert.ThrowsExactly<InvalidOperationException>(() => deserializer.Deserialize(data, new SimpleDto()));
    }

    [TestMethod]
    public void Deserialize_NullData_ThrowsArgumentNullException()
    {
        ComponentModelDeserializer deserializer = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => deserializer.Deserialize<SimpleDto>(null!, new SimpleDto()));
    }

    [TestMethod]
    public void Deserialize_NullTarget_ThrowsArgumentNullException()
    {
        ComponentModelDeserializer deserializer = new();
        Dictionary<string, string?> data = [];

        Assert.ThrowsExactly<ArgumentNullException>(() => deserializer.Deserialize(data, (SimpleDto)null!));
    }

    [TestMethod]
    public void Deserialize_NullValue_SetsNull()
    {
        ComponentModelDeserializer deserializer = new();
        Dictionary<string, string?> data = new() { ["Name"] = null };
        SimpleDto target = new() { Name = "Alice" };

        deserializer.Deserialize(data, target);

        Assert.IsNull(target.Name);
    }

    [TestMethod]
    public void Deserialize_PopulatesTarget()
    {
        ComponentModelDeserializer deserializer = new();
        Dictionary<string, string?> data = new() { ["Name"] = "Alice", ["Age"] = "30", ["IsActive"] = "True" };
        SimpleDto target = new();

        SimpleDto result = deserializer.Deserialize(data, target);

        Assert.AreSame(target, result);
        Assert.AreEqual("Alice", target.Name);
        Assert.AreEqual(30, target.Age);
        Assert.IsTrue(target.IsActive);
    }

    [TestMethod]
    public void DeserializeRaw_NullData_ThrowsArgumentNullException()
    {
        ComponentModelDeserializer deserializer = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => deserializer.DeserializeRaw<SimpleDto>(null!, new SimpleDto()));
    }

    [TestMethod]
    public void DeserializeRaw_PopulatesTarget()
    {
        ComponentModelDeserializer deserializer = new();
        Dictionary<string, object?> data = new() { ["Name"] = "Alice", ["Age"] = 30, ["IsActive"] = true };
        SimpleDto target = new();

        deserializer.DeserializeRaw(data, target);

        Assert.AreEqual("Alice", target.Name);
        Assert.AreEqual(30, target.Age);
        Assert.IsTrue(target.IsActive);
    }

    [TestMethod]
    public void RoundTrip_SerializeDeserialize()
    {
        ComponentModelSerializer serializer = new();
        ComponentModelDeserializer deserializer = new();
        SimpleDto original = new() { Name = "Alice", Age = 30, IsActive = true };

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
