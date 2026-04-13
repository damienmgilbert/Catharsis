using System.ComponentModel;
using System.Globalization;
using Catharsis.ComponentModel.TypeConverter;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

///<summary>
///Unit tests for the <see cref="ConverterRegistry"/> class.
///</summary>
[TestClass]
public sealed class ConverterRegistryTests
{
    #region Public methods
    [TestMethod]
    public void Clear_RemovesAllRegistrations()
    {
        ConverterRegistry registry = new();
        registry.Register<int>(new Int32Converter());
        registry.Register<double>(new DoubleConverter());

        registry.Clear();

        Assert.AreEqual(0, registry.Count);
        Assert.IsFalse(registry.IsRegistered<int>());
        Assert.IsFalse(registry.IsRegistered<double>());
    }

    [TestMethod]
    public void ConvertFrom_NullDestinationType_ThrowsArgumentNullException()
    {
        ConverterRegistry registry = new();
        registry.Register<int>(new Int32Converter());

        Assert.ThrowsExactly<ArgumentNullException>(() => registry.ConvertFrom(42, null!, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void ConvertFrom_ValidConversion_ReturnsConverted()
    {
        ConverterRegistry registry = new();
        registry.Register<int>(convertTo: static (ctx, culture, value, dest) => value.ToString(culture));

        object? result = registry.ConvertFrom(42, typeof(string), CultureInfo.InvariantCulture);

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertTo_UnsupportedConversion_ThrowsNotSupportedException()
    {
        ConverterRegistry registry = new();
        // Register a converter that only converts from string
        registry.Register<int>(convertFrom: (ctx, culture, value) => int.Parse((string)value, culture));

        Assert.ThrowsExactly<NotSupportedException>(() => registry.ConvertTo<int>(3.14, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void ConvertTo_ValidConversion_ReturnsConverted()
    {
        ConverterRegistry registry = new();
        registry.Register<int>(convertFrom: static (ctx, culture, value) => int.Parse((string)value, culture));

        int result = registry.ConvertTo<int>("42", CultureInfo.InvariantCulture);

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Count_ReflectsRegistrations()
    {
        ConverterRegistry registry = new();
        Assert.AreEqual(0, registry.Count);

        registry.Register<int>(new Int32Converter());
        Assert.AreEqual(1, registry.Count);

        registry.Register<double>(new DoubleConverter());
        Assert.AreEqual(2, registry.Count);
    }

    [TestMethod]
    public void FluentChaining_RegisterMultipleConverters()
    {
        ConverterRegistry registry = new ConverterRegistry()
            .Register<int>(new Int32Converter())
            .Register<double>(new DoubleConverter())
            .RegisterEnum<Color>();

        Assert.AreEqual(3, registry.Count);
        Assert.IsTrue(registry.IsRegistered<int>());
        Assert.IsTrue(registry.IsRegistered<double>());
        Assert.IsTrue(registry.IsRegistered<Color>());
    }

    [TestMethod]
    public void GetConverter_NullType_ThrowsArgumentNullException()
    {
        ConverterRegistry registry = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => registry.GetConverter(null!));
    }

    [TestMethod]
    public void GetConverter_RegisteredType_ReturnsRegisteredConverter()
    {
        ConverterRegistry registry = new();
        Int32Converter converter = new();
        registry.Register<int>(converter);

        Assert.AreSame(converter, registry.GetConverter<int>());
    }

    [TestMethod]
    public void GetConverter_UnregisteredType_FallsBackToTypeDescriptor()
    {
        ConverterRegistry registry = new();

        System.ComponentModel.TypeConverter converter = registry.GetConverter<int>();

        Assert.IsNotNull(converter);
        // TypeDescriptor returns a converter for int by default
        Assert.IsTrue(converter.CanConvertFrom(typeof(string)));
    }

    [TestMethod]
    public void IsRegistered_Generic_NotRegistered_ReturnsFalse()
    {
        ConverterRegistry registry = new();

        Assert.IsFalse(registry.IsRegistered<int>());
    }

    [TestMethod]
    public void IsRegistered_Generic_RegisteredType_ReturnsTrue()
    {
        ConverterRegistry registry = new();
        registry.Register<int>(new Int32Converter());

        Assert.IsTrue(registry.IsRegistered<int>());
    }

    [TestMethod]
    public void IsRegistered_Type_NullType_ThrowsArgumentNullException()
    {
        ConverterRegistry registry = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => registry.IsRegistered(null!));
    }

    [TestMethod]
    public void Register_GenericWithConverter_NullConverter_ThrowsArgumentNullException()
    {
        ConverterRegistry registry = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => registry.Register<int>((System.ComponentModel.TypeConverter)null!));
    }

    [TestMethod]
    public void Register_GenericWithConverter_StoresAndReturnsChain()
    {
        ConverterRegistry registry = new();
        Int32Converter converter = new();

        ConverterRegistry result = registry.Register<int>(converter);

        Assert.AreSame(registry, result);
        Assert.AreSame(converter, registry.GetConverter<int>());
    }

    [TestMethod]
    public void Register_ReplacesExistingRegistration()
    {
        ConverterRegistry registry = new();
        Int32Converter first = new();
        Int32Converter second = new();

        registry.Register<int>(first);
        registry.Register<int>(second);

        Assert.AreSame(second, registry.GetConverter<int>());
    }

    [TestMethod]
    public void Register_TypeAndConverter_NullConverter_ThrowsArgumentNullException()
    {
        ConverterRegistry registry = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => registry.Register(typeof(int), null!));
    }

    [TestMethod]
    public void Register_TypeAndConverter_NullType_ThrowsArgumentNullException()
    {
        ConverterRegistry registry = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => registry.Register(null!, new Int32Converter()));
    }

    [TestMethod]
    public void Register_TypeAndConverter_StoresCorrectly()
    {
        ConverterRegistry registry = new();
        Int32Converter converter = new();

        ConverterRegistry result = registry.Register(typeof(int), converter);

        Assert.AreSame(registry, result);
        Assert.AreSame(converter, registry.GetConverter(typeof(int)));
    }

    [TestMethod]
    public void Register_WithDelegates_CreatesGenericTypeConverter()
    {
        ConverterRegistry registry = new();

        registry.Register<int>(convertFrom: static (ctx, culture, value) => int.Parse((string)value, culture));

        System.ComponentModel.TypeConverter converter = registry.GetConverter<int>();
        Assert.IsInstanceOfType<GenericTypeConverter<int>>(converter);
    }

    [TestMethod]
    public void RegisterEnum_CreatesEnumTypeConverter()
    {
        ConverterRegistry registry = new();

        registry.RegisterEnum<Color>();

        System.ComponentModel.TypeConverter converter = registry.GetConverter<Color>();
        Assert.IsInstanceOfType<EnumTypeConverter<Color>>(converter);
    }

    [TestMethod]
    public void RegisterSpanBased_CreatesSpanBasedConverter()
    {
        ConverterRegistry registry = new();

        registry.RegisterSpanBased<int>(static (span, provider, out result) => int.TryParse(span, NumberStyles.Integer, provider, out result));

        System.ComponentModel.TypeConverter converter = registry.GetConverter<int>();
        Assert.IsInstanceOfType<SpanBasedTypeConverter<int>>(converter);
    }

    [TestMethod]
    public void RegisterSpanBased_NullTryParse_ThrowsArgumentNullException()
    {
        ConverterRegistry registry = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => registry.RegisterSpanBased<int>(null!));
    }

    [TestMethod]
    public void TryConvertTo_InvalidConversion_ReturnsFalse()
    {
        ConverterRegistry registry = new();
        registry.Register<int>(convertFrom: static (ctx, culture, value) => int.Parse((string)value, culture));

        bool success = registry.TryConvertTo<int>("not_a_number", out int result, CultureInfo.InvariantCulture);

        Assert.IsFalse(success);
        Assert.AreEqual(default, result);
    }

    [TestMethod]
    public void TryConvertTo_ValidConversion_ReturnsTrueAndResult()
    {
        ConverterRegistry registry = new();
        registry.Register<int>(convertFrom: static (ctx, culture, value) => int.Parse((string)value, culture));

        bool success = registry.TryConvertTo<int>("42", out int result, CultureInfo.InvariantCulture);

        Assert.IsTrue(success);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void TryGetConverter_NotRegistered_ReturnsFalse()
    {
        ConverterRegistry registry = new();

        bool found = registry.TryGetConverter(typeof(int), out System.ComponentModel.TypeConverter? result);

        Assert.IsFalse(found);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void TryGetConverter_NullType_ThrowsArgumentNullException()
    {
        ConverterRegistry registry = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => registry.TryGetConverter(null!, out _));
    }

    [TestMethod]
    public void TryGetConverter_Registered_ReturnsTrueAndConverter()
    {
        ConverterRegistry registry = new();
        Int32Converter converter = new();
        registry.Register<int>(converter);

        bool found = registry.TryGetConverter(typeof(int), out System.ComponentModel.TypeConverter? result);

        Assert.IsTrue(found);
        Assert.AreSame(converter, result);
    }

    [TestMethod]
    public void Unregister_Generic_RegisteredType_ReturnsTrue()
    {
        ConverterRegistry registry = new();
        registry.Register<int>(new Int32Converter());

        Assert.IsTrue(registry.Unregister<int>());
    }

    [TestMethod]
    public void Unregister_Generic_UnregisteredType_ReturnsFalse()
    {
        ConverterRegistry registry = new();

        Assert.IsFalse(registry.Unregister<int>());
    }

    [TestMethod]
    public void Unregister_Type_NullType_ThrowsArgumentNullException()
    {
        ConverterRegistry registry = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => registry.Unregister(null!));
    }
    #endregion

    enum Color
    {
        Red,
        Green,
        Blue
    }
}
