using System.ComponentModel;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class TypeConversionExtensionsTests
{
    #region Public methods
    [TestMethod]
    public void ConvertTo_IntToString_Converts()
    {
        object value = 42;

        string? result = value.ConvertTo<string>();

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertTo_Null_ReturnsDefault()
    {
        object? value = null;

        int result = value.ConvertTo<int>();

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void ConvertTo_SameType_ReturnsSameValue()
    {
        string value = "hello";

        string? result = value.ConvertTo<string>();

        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    public void ConvertTo_StringToInt_Converts()
    {
        object value = "42";

        int result = value.ConvertTo<int>();

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void GetBrowsableProperties_Null_Throws()
    {
        object? obj = null;

        Assert.ThrowsExactly<ArgumentNullException>(() => obj!.GetBrowsableProperties());
    }

    [TestMethod]
    public void GetBrowsableProperties_ReturnsProperties()
    {
        var obj = new { Name = "Test", Value = 42 };

        List<PropertyDescriptor> properties = obj.GetBrowsableProperties().ToList();

        Assert.IsNotEmpty(properties);
    }

    [TestMethod]
    public void GetComponentCategory_WithAttribute_ReturnsCategory()
    {
        DecoratedComponent component = new DecoratedComponent();

        string category = component.GetComponentCategory();

        Assert.AreEqual("Testing", category);
    }

    [TestMethod]
    public void GetComponentCategory_WithoutAttribute_ReturnsEmpty()
    {
        PlainComponent component = new PlainComponent();

        string category = component.GetComponentCategory();

        Assert.AreEqual(string.Empty, category);
    }

    [TestMethod]
    public void GetComponentDescription_WithAttribute_ReturnsDescription()
    {
        DecoratedComponent component = new DecoratedComponent();

        string description = component.GetComponentDescription();

        Assert.AreEqual("A widget for testing", description);
    }

    [TestMethod]
    public void GetComponentDescription_WithoutAttribute_ReturnsEmpty()
    {
        PlainComponent component = new PlainComponent();

        string description = component.GetComponentDescription();

        Assert.AreEqual(string.Empty, description);
    }

    [TestMethod]
    public void GetComponentDisplayName_WithAttribute_ReturnsDisplayName()
    {
        DecoratedComponent component = new DecoratedComponent();

        string name = component.GetComponentDisplayName();

        Assert.AreEqual("Test Widget", name);
    }

    [TestMethod]
    public void GetComponentDisplayName_WithoutAttribute_ReturnsTypeName()
    {
        PlainComponent component = new PlainComponent();

        string name = component.GetComponentDisplayName();

        Assert.AreEqual(nameof(PlainComponent), name);
    }

    [TestMethod]
    public void GetPropertyDescriptor_ExistingProperty_ReturnsDescriptor()
    {
        var obj = new { Name = "Test" };

        PropertyDescriptor? descriptor = obj.GetPropertyDescriptor("Name");

        Assert.IsNotNull(descriptor);
        Assert.AreEqual("Name", descriptor.Name);
    }

    [TestMethod]
    public void GetPropertyDescriptor_NonExistingProperty_ReturnsNull()
    {
        var obj = new { Name = "Test" };

        PropertyDescriptor? descriptor = obj.GetPropertyDescriptor("DoesNotExist");

        Assert.IsNull(descriptor);
    }

    [TestMethod]
    public void GetTypeConverter_Null_Throws()
    {
        Type? type = null;

        Assert.ThrowsExactly<ArgumentNullException>(() => type!.GetTypeConverter());
    }

    [TestMethod]
    public void GetTypeConverter_ReturnsConverter()
    {
        System.ComponentModel.TypeConverter converter = typeof(int).GetTypeConverter();

        Assert.IsNotNull(converter);
        Assert.IsTrue(converter.CanConvertFrom(typeof(string)));
    }

    [TestMethod]
    public void TryConvertTo_InvalidConversion_ReturnsFalse()
    {
        object value = "not_a_number";

        bool success = value.TryConvertTo<int>(out int result);

        Assert.IsFalse(success);
        Assert.AreEqual(default, result);
    }

    [TestMethod]
    public void TryConvertTo_ValidConversion_ReturnsTrue()
    {
        object value = "123";

        bool success = value.TryConvertTo<int>(out int result);

        Assert.IsTrue(success);
        Assert.AreEqual(123, result);
    }
    #endregion

    [DisplayName("Test Widget")]
    [System.ComponentModel.Description("A widget for testing")]
    [Category("Testing")]
    sealed class DecoratedComponent
    {
    }

    sealed class PlainComponent
    {
    }
}
