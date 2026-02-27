using Catharsis.ComponentModel.TypeConverter;
using System.Globalization;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

[TestClass]
public sealed class GenericTypeConverterTests
{
    [TestMethod]
    public void CanConvertFrom_WithConvertFromDelegate_StringReturnsTrue()
    {
        var converter = new GenericTypeConverter<int>(
            convertFrom: (ctx, culture, value) => int.Parse((string)value, culture));

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertFrom_NoConvertFromDelegate_StringReturnsFalse()
    {
        var converter = new GenericTypeConverter<int>();

        Assert.IsFalse(converter.CanConvertFrom(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertFrom_UnsupportedType_ReturnsFalse()
    {
        var converter = new GenericTypeConverter<int>(
            convertFrom: (ctx, culture, value) => 0);

        Assert.IsFalse(converter.CanConvertFrom(null, typeof(DateTime)));
    }

    [TestMethod]
    public void CanConvertTo_WithConvertToDelegate_StringReturnsTrue()
    {
        var converter = new GenericTypeConverter<int>(
            convertTo: (ctx, culture, value, destType) => value.ToString(culture));

        Assert.IsTrue(converter.CanConvertTo(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertTo_NoConvertToDelegate_FallsBackToBase()
    {
        var converter = new GenericTypeConverter<int>();

        Assert.IsTrue(converter.CanConvertTo(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertTo_NullDestinationType_ReturnsFalse()
    {
        var converter = new GenericTypeConverter<int>(
            convertTo: (ctx, culture, value, destType) => value.ToString(culture));

        Assert.IsFalse(converter.CanConvertTo(null, null));
    }

    [TestMethod]
    public void ConvertFrom_StringToInt_ConvertsCorrectly()
    {
        var converter = new GenericTypeConverter<int>(
            convertFrom: (ctx, culture, value) => int.Parse((string)value, culture));

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "42");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_UsesCultureParameter()
    {
        CultureInfo? capturedCulture = null;
        var converter = new GenericTypeConverter<double>(
            convertFrom: (ctx, culture, value) =>
            {
                capturedCulture = culture;
                return double.Parse((string)value, culture!);
            });

        var frCulture = CultureInfo.GetCultureInfo("fr-FR");
        converter.ConvertFrom(null, frCulture, "3,14");

        Assert.AreSame(frCulture, capturedCulture);
    }

    [TestMethod]
    public void ConvertFrom_NullCulture_DefaultsToCurrentCulture()
    {
        CultureInfo? capturedCulture = null;
        var converter = new GenericTypeConverter<string>(
            convertFrom: (ctx, culture, value) =>
            {
                capturedCulture = culture;
                return value.ToString();
            });

        converter.ConvertFrom(null, null, "test");

        Assert.AreSame(CultureInfo.CurrentCulture, capturedCulture);
    }

    [TestMethod]
    public void ConvertFrom_NoDelegate_ThrowsNotSupportedException()
    {
        var converter = new GenericTypeConverter<int>();

        Assert.ThrowsExactly<NotSupportedException>(
            () => converter.ConvertFrom(null, CultureInfo.InvariantCulture, "42"));
    }

    [TestMethod]
    public void ConvertTo_IntToString_ConvertsCorrectly()
    {
        var converter = new GenericTypeConverter<int>(
            convertTo: (ctx, culture, value, destType) => value.ToString(culture));

        var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, typeof(string));

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertTo_NullDestinationType_ThrowsArgumentNullException()
    {
        var converter = new GenericTypeConverter<int>(
            convertTo: (ctx, culture, value, destType) => value.ToString(culture));

        Assert.ThrowsExactly<ArgumentNullException>(
            () => converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, null!));
    }

    [TestMethod]
    public void ConvertTo_ValueNotOfTypeT_FallsBackToBase()
    {
        var converter = new GenericTypeConverter<int>(
            convertTo: (ctx, culture, value, destType) => value.ToString(culture));

        // Passing a string instead of int should fall back to base
        var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, "hello", typeof(string));

        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    public void ConvertTo_NullCulture_DefaultsToCurrentCulture()
    {
        CultureInfo? capturedCulture = null;
        var converter = new GenericTypeConverter<int>(
            convertTo: (ctx, culture, value, destType) =>
            {
                capturedCulture = culture;
                return value.ToString(culture);
            });

        converter.ConvertTo(null, null, 42, typeof(string));

        Assert.AreSame(CultureInfo.CurrentCulture, capturedCulture);
    }

    [TestMethod]
    public void IsValid_ValueOfTypeT_ReturnsTrue()
    {
        var converter = new GenericTypeConverter<int>();

        Assert.IsTrue(converter.IsValid(null, 42));
    }

    [TestMethod]
    public void IsValid_NullValue_ReturnsFalse()
    {
        var converter = new GenericTypeConverter<int>();

        Assert.IsFalse(converter.IsValid(null, null));
    }

    [TestMethod]
    public void IsValid_ConvertibleString_ReturnsTrue()
    {
        var converter = new GenericTypeConverter<int>(
            convertFrom: (ctx, culture, value) => int.Parse((string)value, culture));

        Assert.IsTrue(converter.IsValid(null, "42"));
    }

    [TestMethod]
    public void IsValid_NonConvertibleString_ReturnsFalse()
    {
        var converter = new GenericTypeConverter<int>(
            convertFrom: (ctx, culture, value) => int.Parse((string)value, culture));

        Assert.IsFalse(converter.IsValid(null, "not_a_number"));
    }
}
