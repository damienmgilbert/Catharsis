using Catharsis.ComponentModel;
using System.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class TypeConversionExtensionsTests
{
    [DisplayName("Test Widget")]
    [System.ComponentModel.Description("A widget for testing")]
    [Category("Testing")]
    private sealed class DecoratedComponent { }

    private sealed class PlainComponent { }

    [TestMethod]
    public void ConvertTo_StringToInt_Converts()
    {
        object value = "42";

        var result = value.ConvertTo<int>();

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertTo_IntToString_Converts()
    {
        object value = 42;

        var result = value.ConvertTo<string>();

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertTo_SameType_ReturnsSameValue()
    {
        var value = "hello";

        var result = value.ConvertTo<string>();

        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    public void ConvertTo_Null_ReturnsDefault()
    {
        object? value = null;

        var result = value.ConvertTo<int>();

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void TryConvertTo_ValidConversion_ReturnsTrue()
    {
        object value = "123";

        var success = value.TryConvertTo<int>(out var result);

        Assert.IsTrue(success);
        Assert.AreEqual(123, result);
    }

    [TestMethod]
    public void TryConvertTo_InvalidConversion_ReturnsFalse()
    {
        object value = "not_a_number";

        var success = value.TryConvertTo<int>(out var result);

        Assert.IsFalse(success);
        Assert.AreEqual(default, result);
    }

    [TestMethod]
    public void GetTypeConverter_ReturnsConverter()
    {
        var converter = typeof(int).GetTypeConverter();

        Assert.IsNotNull(converter);
        Assert.IsTrue(converter.CanConvertFrom(typeof(string)));
    }

    [TestMethod]
    public void GetTypeConverter_Null_Throws()
    {
        Type? type = null;

        Assert.ThrowsExactly<ArgumentNullException>(() => type!.GetTypeConverter());
    }

    [TestMethod]
    public void GetBrowsableProperties_ReturnsProperties()
    {
        var obj = new { Name = "Test", Value = 42 };

        var properties = obj.GetBrowsableProperties().ToList();

        Assert.IsTrue(properties.Count > 0);
    }

    [TestMethod]
    public void GetBrowsableProperties_Null_Throws()
    {
        object? obj = null;

        Assert.ThrowsExactly<ArgumentNullException>(() => obj!.GetBrowsableProperties());
    }

    [TestMethod]
    public void GetPropertyDescriptor_ExistingProperty_ReturnsDescriptor()
    {
        var obj = new { Name = "Test" };

        var descriptor = obj.GetPropertyDescriptor("Name");

        Assert.IsNotNull(descriptor);
        Assert.AreEqual("Name", descriptor.Name);
    }

    [TestMethod]
    public void GetPropertyDescriptor_NonExistingProperty_ReturnsNull()
    {
        var obj = new { Name = "Test" };

        var descriptor = obj.GetPropertyDescriptor("DoesNotExist");

        Assert.IsNull(descriptor);
    }

    [TestMethod]
    public void GetComponentDisplayName_WithAttribute_ReturnsDisplayName()
    {
        var component = new DecoratedComponent();

        var name = component.GetComponentDisplayName();

        Assert.AreEqual("Test Widget", name);
    }

    [TestMethod]
    public void GetComponentDisplayName_WithoutAttribute_ReturnsTypeName()
    {
        var component = new PlainComponent();

        var name = component.GetComponentDisplayName();

        Assert.AreEqual(nameof(PlainComponent), name);
    }

    [TestMethod]
    public void GetComponentDescription_WithAttribute_ReturnsDescription()
    {
        var component = new DecoratedComponent();

        var description = component.GetComponentDescription();

        Assert.AreEqual("A widget for testing", description);
    }

    [TestMethod]
    public void GetComponentDescription_WithoutAttribute_ReturnsEmpty()
    {
        var component = new PlainComponent();

        var description = component.GetComponentDescription();

        Assert.AreEqual(string.Empty, description);
    }

    [TestMethod]
    public void GetComponentCategory_WithAttribute_ReturnsCategory()
    {
        var component = new DecoratedComponent();

        var category = component.GetComponentCategory();

        Assert.AreEqual("Testing", category);
    }

    [TestMethod]
    public void GetComponentCategory_WithoutAttribute_ReturnsEmpty()
    {
        var component = new PlainComponent();

        var category = component.GetComponentCategory();

        Assert.AreEqual(string.Empty, category);
    }
}
