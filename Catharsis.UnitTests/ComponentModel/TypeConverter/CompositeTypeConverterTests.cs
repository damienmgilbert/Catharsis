using Catharsis.ComponentModel.TypeConverter;
using System.ComponentModel;
using System.Globalization;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

[TestClass]
public sealed class CompositeTypeConverterTests
{
    [TestMethod]
    public void Constructor_NullArray_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new CompositeTypeConverter((System.ComponentModel.TypeConverter[])null!));
    }

    [TestMethod]
    public void Constructor_EmptyArray_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => new CompositeTypeConverter(Array.Empty<System.ComponentModel.TypeConverter>()));
    }

    [TestMethod]
    public void Constructor_NullEnumerable_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new CompositeTypeConverter((IEnumerable<System.ComponentModel.TypeConverter>)null!));
    }

    [TestMethod]
    public void Constructor_EmptyEnumerable_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => new CompositeTypeConverter(Enumerable.Empty<System.ComponentModel.TypeConverter>()));
    }

    [TestMethod]
    public void Count_ReturnsNumberOfConverters()
    {
        var converter = new CompositeTypeConverter(new Int32Converter(), new StringConverter());

        Assert.AreEqual(2, converter.Count);
    }

    [TestMethod]
    public void CanConvertFrom_AnyConverterSupports_ReturnsTrue()
    {
        var composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsTrue(composite.CanConvertFrom(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertFrom_NoConverterSupports_ReturnsFalse()
    {
        var composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsFalse(composite.CanConvertFrom(null, typeof(DateTime)));
    }

    [TestMethod]
    public void CanConvertTo_AnyConverterSupports_ReturnsTrue()
    {
        var composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsTrue(composite.CanConvertTo(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertTo_NullDestination_ReturnsFalse()
    {
        var composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsFalse(composite.CanConvertTo(null, null));
    }

    [TestMethod]
    public void ConvertFrom_FirstConverterSucceeds_ReturnsResult()
    {
        var composite = new CompositeTypeConverter(new Int32Converter(), new DoubleConverter());

        var result = composite.ConvertFrom(null, CultureInfo.InvariantCulture, "42");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_FirstFails_FallsToSecond()
    {
        // A converter that always throws FormatException for strings, followed by a working one
        var failing = new GenericTypeConverter<int>(
            convertFrom: (ctx, culture, value) => throw new FormatException("fail"));
        var working = new GenericTypeConverter<int>(
            convertFrom: (ctx, culture, value) => int.Parse((string)value, culture));
        var composite = new CompositeTypeConverter(failing, working);

        var result = composite.ConvertFrom(null, CultureInfo.InvariantCulture, "42");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_AllConvertersFail_ThrowsNotSupportedException()
    {
        var failing = new GenericTypeConverter<int>(
            convertFrom: (ctx, culture, value) => throw new FormatException("fail"));
        var composite = new CompositeTypeConverter(failing);

        Assert.ThrowsExactly<NotSupportedException>(
            () => composite.ConvertFrom(null, CultureInfo.InvariantCulture, "42"));
    }

    [TestMethod]
    public void ConvertTo_FirstConverterSucceeds_ReturnsResult()
    {
        var composite = new CompositeTypeConverter(new Int32Converter());

        var result = composite.ConvertTo(null, CultureInfo.InvariantCulture, 42, typeof(string));

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertTo_NullDestinationType_ThrowsArgumentNullException()
    {
        var composite = new CompositeTypeConverter(new Int32Converter());

        Assert.ThrowsExactly<ArgumentNullException>(
            () => composite.ConvertTo(null, CultureInfo.InvariantCulture, 42, null!));
    }

    [TestMethod]
    public void ConvertTo_AllConvertersFail_ThrowsNotSupportedException()
    {
        var failing = new GenericTypeConverter<int>(
            convertTo: (ctx, culture, value, dest) => throw new FormatException("fail"));
        var composite = new CompositeTypeConverter(failing);

        Assert.ThrowsExactly<NotSupportedException>(
            () => composite.ConvertTo(null, CultureInfo.InvariantCulture, 42, typeof(string)));
    }

    [TestMethod]
    public void IsValid_AnyConverterReturnsTrue_ReturnsTrue()
    {
        var composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsTrue(composite.IsValid(null, "42"));
    }

    [TestMethod]
    public void IsValid_NoConverterReturnsTrue_ReturnsFalse()
    {
        // A converter that never sees the value as valid
        var converter = new GenericTypeConverter<DateTime>();
        var composite = new CompositeTypeConverter(converter);

        Assert.IsFalse(composite.IsValid(null, "not_valid_for_datetime_converter"));
    }

    [TestMethod]
    public void GetStandardValuesSupported_SomeConverterSupports_ReturnsTrue()
    {
        var enumConverter = new EnumTypeConverter<DayOfWeek>();
        var composite = new CompositeTypeConverter(enumConverter);

        Assert.IsTrue(composite.GetStandardValuesSupported(null));
    }

    [TestMethod]
    public void GetStandardValuesSupported_NoConverterSupports_ReturnsFalse()
    {
        var composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsFalse(composite.GetStandardValuesSupported(null));
    }

    [TestMethod]
    public void GetStandardValues_DelegatesToFirstSupportingConverter()
    {
        var enumConverter = new EnumTypeConverter<DayOfWeek>();
        var composite = new CompositeTypeConverter(new Int32Converter(), enumConverter);

        var values = composite.GetStandardValues(null);

        Assert.IsNotNull(values);
        Assert.AreEqual(7, values.Count);
    }

    [TestMethod]
    public void GetStandardValues_NoConverterSupports_ReturnsNull()
    {
        var composite = new CompositeTypeConverter(new Int32Converter());

        var values = composite.GetStandardValues(null);

        Assert.IsNull(values);
    }
}
