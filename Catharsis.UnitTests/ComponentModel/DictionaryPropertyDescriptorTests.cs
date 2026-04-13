using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="DictionaryPropertyDescriptor"/> class.
///</summary>
[TestClass]
public class DictionaryPropertyDescriptorTests
{
    [TestMethod]
    public void CanResetValue_ReturnsTrue()
    {
        Dictionary<string, object?> store = [];
        DictionaryPropertyDescriptor prop = new("Name", typeof(string), store);

        Assert.IsTrue(prop.CanResetValue(null!));
    }

    [TestMethod]
    public void GetValue_MissingKey_ReturnsNull()
    {
        Dictionary<string, object?> store = [];
        DictionaryPropertyDescriptor prop = new("Name", typeof(string), store);

        Assert.IsNull(prop.GetValue(null));
    }

    [TestMethod]
    public void GetValue_ReturnsValueFromStore()
    {
        Dictionary<string, object?> store = new() { ["Name"] = "Alice" };
        DictionaryPropertyDescriptor prop = new("Name", typeof(string), store);

        Assert.AreEqual("Alice", prop.GetValue(null));
    }

    [TestMethod]
    public void IsReadOnly_ReturnsFalse()
    {
        Dictionary<string, object?> store = [];
        DictionaryPropertyDescriptor prop = new("Name", typeof(string), store);

        Assert.IsFalse(prop.IsReadOnly);
    }

    [TestMethod]
    public void PropertyType_ReturnsConfiguredType()
    {
        Dictionary<string, object?> store = [];
        DictionaryPropertyDescriptor prop = new("Age", typeof(int), store);

        Assert.AreEqual(typeof(int), prop.PropertyType);
    }

    [TestMethod]
    public void ResetValue_RemovesFromStore()
    {
        Dictionary<string, object?> store = new() { ["Name"] = "Alice" };
        DictionaryPropertyDescriptor prop = new("Name", typeof(string), store);

        prop.ResetValue(null!);

        Assert.IsFalse(store.ContainsKey("Name"));
    }

    [TestMethod]
    public void SetValue_UpdatesStore()
    {
        Dictionary<string, object?> store = [];
        DictionaryPropertyDescriptor prop = new("Name", typeof(string), store);

        prop.SetValue(null, "Bob");

        Assert.AreEqual("Bob", store["Name"]);
    }

    [TestMethod]
    public void ShouldSerializeValue_ReturnsFalseWhenAbsent()
    {
        Dictionary<string, object?> store = [];
        DictionaryPropertyDescriptor prop = new("Name", typeof(string), store);

        Assert.IsFalse(prop.ShouldSerializeValue(null!));
    }

    [TestMethod]
    public void ShouldSerializeValue_ReturnsTrueWhenPresent()
    {
        Dictionary<string, object?> store = new() { ["Name"] = "Alice" };
        DictionaryPropertyDescriptor prop = new("Name", typeof(string), store);

        Assert.IsTrue(prop.ShouldSerializeValue(null!));
    }
}
