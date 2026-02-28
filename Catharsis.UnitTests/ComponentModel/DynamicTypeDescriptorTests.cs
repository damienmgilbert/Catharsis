using Catharsis.ComponentModel;
using System.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class DynamicTypeDescriptorTests
{
    #region Public methods
    [TestMethod]
    public void AddProperty_DuplicateName_Throws()
    {
        DynamicTypeDescriptor descriptor = new DynamicTypeDescriptor();
        Dictionary<string, object?> store = new Dictionary<string, object?>();
        descriptor.AddProperty(new DictionaryPropertyDescriptor("Name", typeof(string), store));

        Assert.ThrowsExactly<ArgumentException>(() => descriptor.AddProperty(new DictionaryPropertyDescriptor("Name", typeof(string), store)));
    }

    [TestMethod]
    public void AddProperty_IncreasesPropertyCount()
    {
        DynamicTypeDescriptor descriptor = new DynamicTypeDescriptor();
        Dictionary<string, object?> store = new Dictionary<string, object?>();
        DictionaryPropertyDescriptor prop = new DictionaryPropertyDescriptor("Age", typeof(int), store);

        descriptor.AddProperty(prop);

        Assert.AreEqual(1, descriptor.PropertyCount);
    }

    [TestMethod]
    public void AddProperty_Null_Throws()
    {
        DynamicTypeDescriptor descriptor = new DynamicTypeDescriptor();

        Assert.ThrowsExactly<ArgumentNullException>(() => descriptor.AddProperty(null!));
    }

    [TestMethod]
    public void GetProperties_ReturnsDynamicProperties()
    {
        DynamicTypeDescriptor descriptor = new DynamicTypeDescriptor();
        Dictionary<string, object?> store = new Dictionary<string, object?>();
        descriptor.AddProperty(new DictionaryPropertyDescriptor("Name", typeof(string), store));
        descriptor.AddProperty(new DictionaryPropertyDescriptor("Age", typeof(int), store));

        PropertyDescriptorCollection properties = descriptor.GetProperties();

        Assert.HasCount(2, properties);
        Assert.IsNotNull(properties["Name"]);
        Assert.IsNotNull(properties["Age"]);
    }

    [TestMethod]
    public void RemoveProperty_NonExisting_ReturnsFalse()
    {
        DynamicTypeDescriptor descriptor = new DynamicTypeDescriptor();

        Assert.IsFalse(descriptor.RemoveProperty("DoesNotExist"));
    }

    [TestMethod]
    public void RemoveProperty_RemovesExistingProperty()
    {
        DynamicTypeDescriptor descriptor = new DynamicTypeDescriptor();
        Dictionary<string, object?> store = new Dictionary<string, object?>();
        descriptor.AddProperty(new DictionaryPropertyDescriptor("Name", typeof(string), store));

        bool removed = descriptor.RemoveProperty("Name");

        Assert.IsTrue(removed);
        Assert.AreEqual(0, descriptor.PropertyCount);
    }
    #endregion
}
