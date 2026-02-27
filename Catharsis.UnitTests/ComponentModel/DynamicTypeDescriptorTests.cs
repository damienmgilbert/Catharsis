using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class DynamicTypeDescriptorTests
{
    [TestMethod]
    public void AddProperty_IncreasesPropertyCount()
    {
        var descriptor = new DynamicTypeDescriptor();
        var store = new Dictionary<string, object?>();
        var prop = new DictionaryPropertyDescriptor("Age", typeof(int), store);

        descriptor.AddProperty(prop);

        Assert.AreEqual(1, descriptor.PropertyCount);
    }

    [TestMethod]
    public void GetProperties_ReturnsDynamicProperties()
    {
        var descriptor = new DynamicTypeDescriptor();
        var store = new Dictionary<string, object?>();
        descriptor.AddProperty(new DictionaryPropertyDescriptor("Name", typeof(string), store));
        descriptor.AddProperty(new DictionaryPropertyDescriptor("Age", typeof(int), store));

        var properties = descriptor.GetProperties();

        Assert.AreEqual(2, properties.Count);
        Assert.IsNotNull(properties["Name"]);
        Assert.IsNotNull(properties["Age"]);
    }

    [TestMethod]
    public void RemoveProperty_RemovesExistingProperty()
    {
        var descriptor = new DynamicTypeDescriptor();
        var store = new Dictionary<string, object?>();
        descriptor.AddProperty(new DictionaryPropertyDescriptor("Name", typeof(string), store));

        var removed = descriptor.RemoveProperty("Name");

        Assert.IsTrue(removed);
        Assert.AreEqual(0, descriptor.PropertyCount);
    }

    [TestMethod]
    public void RemoveProperty_NonExisting_ReturnsFalse()
    {
        var descriptor = new DynamicTypeDescriptor();

        Assert.IsFalse(descriptor.RemoveProperty("DoesNotExist"));
    }

    [TestMethod]
    public void AddProperty_DuplicateName_Throws()
    {
        var descriptor = new DynamicTypeDescriptor();
        var store = new Dictionary<string, object?>();
        descriptor.AddProperty(new DictionaryPropertyDescriptor("Name", typeof(string), store));

        Assert.ThrowsExactly<ArgumentException>(() =>
            descriptor.AddProperty(new DictionaryPropertyDescriptor("Name", typeof(string), store)));
    }

    [TestMethod]
    public void AddProperty_Null_Throws()
    {
        var descriptor = new DynamicTypeDescriptor();

        Assert.ThrowsExactly<ArgumentNullException>(() => descriptor.AddProperty(null!));
    }
}

[TestClass]
public class DictionaryPropertyDescriptorTests
{
    [TestMethod]
    public void GetValue_ReturnsValueFromStore()
    {
        var store = new Dictionary<string, object?> { ["Name"] = "Alice" };
        var prop = new DictionaryPropertyDescriptor("Name", typeof(string), store);

        Assert.AreEqual("Alice", prop.GetValue(null));
    }

    [TestMethod]
    public void SetValue_UpdatesStore()
    {
        var store = new Dictionary<string, object?>();
        var prop = new DictionaryPropertyDescriptor("Name", typeof(string), store);

        prop.SetValue(null, "Bob");

        Assert.AreEqual("Bob", store["Name"]);
    }

    [TestMethod]
    public void ResetValue_RemovesFromStore()
    {
        var store = new Dictionary<string, object?> { ["Name"] = "Alice" };
        var prop = new DictionaryPropertyDescriptor("Name", typeof(string), store);

        prop.ResetValue(null!);

        Assert.IsFalse(store.ContainsKey("Name"));
    }

    [TestMethod]
    public void ShouldSerializeValue_ReturnsTrueWhenPresent()
    {
        var store = new Dictionary<string, object?> { ["Name"] = "Alice" };
        var prop = new DictionaryPropertyDescriptor("Name", typeof(string), store);

        Assert.IsTrue(prop.ShouldSerializeValue(null!));
    }

    [TestMethod]
    public void ShouldSerializeValue_ReturnsFalseWhenAbsent()
    {
        var store = new Dictionary<string, object?>();
        var prop = new DictionaryPropertyDescriptor("Name", typeof(string), store);

        Assert.IsFalse(prop.ShouldSerializeValue(null!));
    }

    [TestMethod]
    public void PropertyType_ReturnsConfiguredType()
    {
        var store = new Dictionary<string, object?>();
        var prop = new DictionaryPropertyDescriptor("Age", typeof(int), store);

        Assert.AreEqual(typeof(int), prop.PropertyType);
    }

    [TestMethod]
    public void GetValue_MissingKey_ReturnsNull()
    {
        var store = new Dictionary<string, object?>();
        var prop = new DictionaryPropertyDescriptor("Name", typeof(string), store);

        Assert.IsNull(prop.GetValue(null));
    }

    [TestMethod]
    public void CanResetValue_ReturnsTrue()
    {
        var store = new Dictionary<string, object?>();
        var prop = new DictionaryPropertyDescriptor("Name", typeof(string), store);

        Assert.IsTrue(prop.CanResetValue(null!));
    }

    [TestMethod]
    public void IsReadOnly_ReturnsFalse()
    {
        var store = new Dictionary<string, object?>();
        var prop = new DictionaryPropertyDescriptor("Name", typeof(string), store);

        Assert.IsFalse(prop.IsReadOnly);
    }
}
