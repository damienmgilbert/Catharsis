using System.ComponentModel;
using System.Globalization;
using Catharsis.ComponentModel.TypeConverter;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

///<summary>
///Unit tests for the <see cref="CompositeTypeConverter"/> class.
///</summary>
[TestClass]
public sealed class CompositeTypeConverterTests
{
    #region Public methods
    [TestMethod]
    public void CanConvertFrom_AnyConverterSupports_ReturnsTrue()
    {
        CompositeTypeConverter composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsTrue(composite.CanConvertFrom(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertFrom_NoConverterSupports_ReturnsFalse()
    {
        CompositeTypeConverter composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsFalse(composite.CanConvertFrom(null, typeof(DateTime)));
    }

    [TestMethod]
    public void CanConvertTo_AnyConverterSupports_ReturnsTrue()
    {
        CompositeTypeConverter composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsTrue(composite.CanConvertTo(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertTo_NullDestination_ReturnsFalse()
    {
        CompositeTypeConverter composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsFalse(composite.CanConvertTo(null, null));
    }

    [TestMethod]
    public void Constructor_EmptyArray_ThrowsArgumentException() { Assert.ThrowsExactly<ArgumentException>(static () => new CompositeTypeConverter(Array.Empty<System.ComponentModel.TypeConverter>())); }
    [TestMethod]
    public void Constructor_EmptyEnumerable_ThrowsArgumentException() { Assert.ThrowsExactly<ArgumentException>(static () => new CompositeTypeConverter(Enumerable.Empty<System.ComponentModel.TypeConverter>())); }
    [TestMethod]
    public void Constructor_NullArray_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new CompositeTypeConverter((System.ComponentModel.TypeConverter[])null!)); }
    [TestMethod]
    public void Constructor_NullEnumerable_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new CompositeTypeConverter((IEnumerable<System.ComponentModel.TypeConverter>)null!)); }
    [TestMethod]
    public void ConvertFrom_AllConvertersFail_ThrowsNotSupportedException()
    {
        GenericTypeConverter<int> failing = new GenericTypeConverter<int>(convertFrom: (ctx, culture, value) => throw new FormatException("fail"));
        CompositeTypeConverter composite = new CompositeTypeConverter(failing);

        Assert.ThrowsExactly<NotSupportedException>(() => composite.ConvertFrom(null, CultureInfo.InvariantCulture, "42"));
    }

    [TestMethod]
    public void ConvertFrom_FirstConverterSucceeds_ReturnsResult()
    {
        CompositeTypeConverter composite = new CompositeTypeConverter(new Int32Converter(), new DoubleConverter());

        object? result = composite.ConvertFrom(null, CultureInfo.InvariantCulture, "42");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_FirstFails_FallsToSecond()
    {
        // A converter that always throws FormatException for strings, followed by a working one
        GenericTypeConverter<int> failing = new GenericTypeConverter<int>(convertFrom: static (ctx, culture, value) => throw new FormatException("fail"));
        GenericTypeConverter<int> working = new GenericTypeConverter<int>(convertFrom: static (ctx, culture, value) => int.Parse((string)value, culture));
        CompositeTypeConverter composite = new CompositeTypeConverter(failing, working);

        object? result = composite.ConvertFrom(null, CultureInfo.InvariantCulture, "42");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertTo_AllConvertersFail_ThrowsNotSupportedException()
    {
        GenericTypeConverter<int> failing = new GenericTypeConverter<int>(convertTo: (ctx, culture, value, dest) => throw new FormatException("fail"));
        CompositeTypeConverter composite = new CompositeTypeConverter(failing);

        Assert.ThrowsExactly<NotSupportedException>(() => composite.ConvertTo(null, CultureInfo.InvariantCulture, 42, typeof(string)));
    }

    [TestMethod]
    public void ConvertTo_FirstConverterSucceeds_ReturnsResult()
    {
        CompositeTypeConverter composite = new CompositeTypeConverter(new Int32Converter());

        object? result = composite.ConvertTo(null, CultureInfo.InvariantCulture, 42, typeof(string));

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertTo_NullDestinationType_ThrowsArgumentNullException()
    {
        CompositeTypeConverter composite = new CompositeTypeConverter(new Int32Converter());

        Assert.ThrowsExactly<ArgumentNullException>(() => composite.ConvertTo(null, CultureInfo.InvariantCulture, 42, null!));
    }

    [TestMethod]
    public void Count_ReturnsNumberOfConverters()
    {
        CompositeTypeConverter converter = new CompositeTypeConverter(new Int32Converter(), new StringConverter());

        Assert.AreEqual(2, converter.Count);
    }

    [TestMethod]
    public void GetStandardValues_DelegatesToFirstSupportingConverter()
    {
        EnumTypeConverter<DayOfWeek> enumConverter = new EnumTypeConverter<DayOfWeek>();
        CompositeTypeConverter composite = new CompositeTypeConverter(new Int32Converter(), enumConverter);

        System.ComponentModel.TypeConverter.StandardValuesCollection? values = composite.GetStandardValues(null);

        Assert.IsNotNull(values);
        Assert.HasCount(7, values);
    }

    [TestMethod]
    public void GetStandardValues_NoConverterSupports_ReturnsNull()
    {
        CompositeTypeConverter composite = new CompositeTypeConverter(new Int32Converter());

        System.ComponentModel.TypeConverter.StandardValuesCollection? values = composite.GetStandardValues(null);

        Assert.IsNull(values);
    }

    [TestMethod]
    public void GetStandardValuesSupported_NoConverterSupports_ReturnsFalse()
    {
        CompositeTypeConverter composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsFalse(composite.GetStandardValuesSupported(null));
    }

    [TestMethod]
    public void GetStandardValuesSupported_SomeConverterSupports_ReturnsTrue()
    {
        EnumTypeConverter<DayOfWeek> enumConverter = new EnumTypeConverter<DayOfWeek>();
        CompositeTypeConverter composite = new CompositeTypeConverter(enumConverter);

        Assert.IsTrue(composite.GetStandardValuesSupported(null));
    }

    [TestMethod]
    public void IsValid_AnyConverterReturnsTrue_ReturnsTrue()
    {
        CompositeTypeConverter composite = new CompositeTypeConverter(new Int32Converter());

        Assert.IsTrue(composite.IsValid(null, "42"));
    }

    [TestMethod]
    public void IsValid_NoConverterReturnsTrue_ReturnsFalse()
    {
        // A converter that never sees the value as valid
        GenericTypeConverter<DateTime> converter = new GenericTypeConverter<DateTime>();
        CompositeTypeConverter composite = new CompositeTypeConverter(converter);

        Assert.IsFalse(composite.IsValid(null, "not_valid_for_datetime_converter"));
    }
    #endregion
}
