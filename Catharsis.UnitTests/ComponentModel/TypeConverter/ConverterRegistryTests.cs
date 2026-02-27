using System.ComponentModel;
using System.Globalization;

using Catharsis.ComponentModel.TypeConverter;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

[TestClass]
public sealed class ConverterRegistryTests
{
    private enum Color { Red, Green, Blue }

    [TestMethod]
    public void Register_GenericWithConverter_StoresAndReturnsChain()
    {
        var registry = new ConverterRegistry();
        var converter = new Int32Converter();

        var result = registry.Register<int>(converter);

        Assert.AreSame(registry, result);
        Assert.AreSame(converter, registry.GetConverter<int>());
    }

    [TestMethod]
    public void Register_GenericWithConverter_NullConverter_ThrowsArgumentNullException()
    {
        var registry = new ConverterRegistry();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => registry.Register<int>((System.ComponentModel.TypeConverter)null!));
    }

    [TestMethod]
    public void Register_TypeAndConverter_StoresCorrectly()
    {
        var registry = new ConverterRegistry();
        var converter = new Int32Converter();

        var result = registry.Register(typeof(int), converter);

        Assert.AreSame(registry, result);
        Assert.AreSame(converter, registry.GetConverter(typeof(int)));
    }

    [TestMethod]
    public void Register_TypeAndConverter_NullType_ThrowsArgumentNullException()
    {
        var registry = new ConverterRegistry();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => registry.Register(null!, new Int32Converter()));
    }

    [TestMethod]
    public void Register_TypeAndConverter_NullConverter_ThrowsArgumentNullException()
    {
        var registry = new ConverterRegistry();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => registry.Register(typeof(int), null!));
    }

    [TestMethod]
    public void Register_WithDelegates_CreatesGenericTypeConverter()
    {
        var registry = new ConverterRegistry();

        registry.Register<int>(
            convertFrom: (ctx, culture, value) => int.Parse((string)value, culture));

        var converter = registry.GetConverter<int>();
        Assert.IsInstanceOfType<GenericTypeConverter<int>>(converter);
    }

    [TestMethod]
    public void RegisterSpanBased_CreatesSpanBasedConverter()
    {
        var registry = new ConverterRegistry();

        registry.RegisterSpanBased<int>(
            (ReadOnlySpan<char> span, IFormatProvider? provider, out int result) =>
                int.TryParse(span, NumberStyles.Integer, provider, out result));

        var converter = registry.GetConverter<int>();
        Assert.IsInstanceOfType<SpanBasedTypeConverter<int>>(converter);
    }

    [TestMethod]
    public void RegisterSpanBased_NullTryParse_ThrowsArgumentNullException()
    {
        var registry = new ConverterRegistry();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => registry.RegisterSpanBased<int>(null!));
    }

    [TestMethod]
    public void RegisterEnum_CreatesEnumTypeConverter()
    {
        var registry = new ConverterRegistry();

        registry.RegisterEnum<Color>();

        var converter = registry.GetConverter<Color>();
        Assert.IsInstanceOfType<EnumTypeConverter<Color>>(converter);
    }

    [TestMethod]
    public void Register_ReplacesExistingRegistration()
    {
        var registry = new ConverterRegistry();
        var first = new Int32Converter();
        var second = new Int32Converter();

        registry.Register<int>(first);
        registry.Register<int>(second);

        Assert.AreSame(second, registry.GetConverter<int>());
    }

    [TestMethod]
    public void Unregister_Generic_RegisteredType_ReturnsTrue()
    {
        var registry = new ConverterRegistry();
        registry.Register<int>(new Int32Converter());

        Assert.IsTrue(registry.Unregister<int>());
    }

    [TestMethod]
    public void Unregister_Generic_UnregisteredType_ReturnsFalse()
    {
        var registry = new ConverterRegistry();

        Assert.IsFalse(registry.Unregister<int>());
    }

    [TestMethod]
    public void Unregister_Type_NullType_ThrowsArgumentNullException()
    {
        var registry = new ConverterRegistry();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => registry.Unregister(null!));
    }

    [TestMethod]
    public void GetConverter_RegisteredType_ReturnsRegisteredConverter()
    {
        var registry = new ConverterRegistry();
        var converter = new Int32Converter();
        registry.Register<int>(converter);

        Assert.AreSame(converter, registry.GetConverter<int>());
    }

    [TestMethod]
    public void GetConverter_UnregisteredType_FallsBackToTypeDescriptor()
    {
        var registry = new ConverterRegistry();

        var converter = registry.GetConverter<int>();

        Assert.IsNotNull(converter);
        // TypeDescriptor returns a converter for int by default
        Assert.IsTrue(converter.CanConvertFrom(typeof(string)));
    }

    [TestMethod]
    public void GetConverter_NullType_ThrowsArgumentNullException()
    {
        var registry = new ConverterRegistry();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => registry.GetConverter(null!));
    }

    [TestMethod]
    public void TryGetConverter_Registered_ReturnsTrueAndConverter()
    {
        var registry = new ConverterRegistry();
        var converter = new Int32Converter();
        registry.Register<int>(converter);

        var found = registry.TryGetConverter(typeof(int), out var result);

        Assert.IsTrue(found);
        Assert.AreSame(converter, result);
    }

    [TestMethod]
    public void TryGetConverter_NotRegistered_ReturnsFalse()
    {
        var registry = new ConverterRegistry();

        var found = registry.TryGetConverter(typeof(int), out var result);

        Assert.IsFalse(found);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void TryGetConverter_NullType_ThrowsArgumentNullException()
    {
        var registry = new ConverterRegistry();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => registry.TryGetConverter(null!, out _));
    }

    [TestMethod]
    public void IsRegistered_Generic_RegisteredType_ReturnsTrue()
    {
        var registry = new ConverterRegistry();
        registry.Register<int>(new Int32Converter());

        Assert.IsTrue(registry.IsRegistered<int>());
    }

    [TestMethod]
    public void IsRegistered_Generic_NotRegistered_ReturnsFalse()
    {
        var registry = new ConverterRegistry();

        Assert.IsFalse(registry.IsRegistered<int>());
    }

    [TestMethod]
    public void IsRegistered_Type_NullType_ThrowsArgumentNullException()
    {
        var registry = new ConverterRegistry();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => registry.IsRegistered(null!));
    }

    [TestMethod]
    public void Count_ReflectsRegistrations()
    {
        var registry = new ConverterRegistry();
        Assert.AreEqual(0, registry.Count);

        registry.Register<int>(new Int32Converter());
        Assert.AreEqual(1, registry.Count);

        registry.Register<double>(new DoubleConverter());
        Assert.AreEqual(2, registry.Count);
    }

    [TestMethod]
    public void Clear_RemovesAllRegistrations()
    {
        var registry = new ConverterRegistry();
        registry.Register<int>(new Int32Converter());
        registry.Register<double>(new DoubleConverter());

        registry.Clear();

        Assert.AreEqual(0, registry.Count);
        Assert.IsFalse(registry.IsRegistered<int>());
        Assert.IsFalse(registry.IsRegistered<double>());
    }

    [TestMethod]
    public void ConvertTo_ValidConversion_ReturnsConverted()
    {
        var registry = new ConverterRegistry();
        registry.Register<int>(
            convertFrom: (ctx, culture, value) => int.Parse((string)value, culture));

        var result = registry.ConvertTo<int>("42", CultureInfo.InvariantCulture);

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertTo_UnsupportedConversion_ThrowsNotSupportedException()
    {
        var registry = new ConverterRegistry();
        // Register a converter that only converts from string
        registry.Register<int>(
            convertFrom: (ctx, culture, value) => int.Parse((string)value, culture));

        Assert.ThrowsExactly<NotSupportedException>(
            () => registry.ConvertTo<int>(3.14, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void ConvertFrom_ValidConversion_ReturnsConverted()
    {
        var registry = new ConverterRegistry();
        registry.Register<int>(
            convertTo: (ctx, culture, value, dest) => value.ToString(culture));

        var result = registry.ConvertFrom(42, typeof(string), CultureInfo.InvariantCulture);

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertFrom_NullDestinationType_ThrowsArgumentNullException()
    {
        var registry = new ConverterRegistry();
        registry.Register<int>(new Int32Converter());

        Assert.ThrowsExactly<ArgumentNullException>(
            () => registry.ConvertFrom(42, null!, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void TryConvertTo_ValidConversion_ReturnsTrueAndResult()
    {
        var registry = new ConverterRegistry();
        registry.Register<int>(
            convertFrom: (ctx, culture, value) => int.Parse((string)value, culture));

        var success = registry.TryConvertTo<int>("42", out var result, CultureInfo.InvariantCulture);

        Assert.IsTrue(success);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void TryConvertTo_InvalidConversion_ReturnsFalse()
    {
        var registry = new ConverterRegistry();
        registry.Register<int>(
            convertFrom: (ctx, culture, value) => int.Parse((string)value, culture));

        var success = registry.TryConvertTo<int>("not_a_number", out var result, CultureInfo.InvariantCulture);

        Assert.IsFalse(success);
        Assert.AreEqual(default, result);
    }

    [TestMethod]
    public void FluentChaining_RegisterMultipleConverters()
    {
        var registry = new ConverterRegistry()
            .Register<int>(new Int32Converter())
            .Register<double>(new DoubleConverter())
            .RegisterEnum<Color>();

        Assert.AreEqual(3, registry.Count);
        Assert.IsTrue(registry.IsRegistered<int>());
        Assert.IsTrue(registry.IsRegistered<double>());
        Assert.IsTrue(registry.IsRegistered<Color>());
    }
}
