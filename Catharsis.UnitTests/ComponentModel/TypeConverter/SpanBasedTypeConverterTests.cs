using System.Globalization;
using Catharsis.ComponentModel.TypeConverter;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

///<summary>
///Unit tests for the <see cref="SpanBasedTypeConverter"/> class.
///</summary>
[TestClass]
public sealed class SpanBasedTypeConverterTests
{
    #region Private methods
    static bool IntTryFormat(int value, Span<char> destination, IFormatProvider? provider, out int charsWritten) { return value.TryFormat(destination, out charsWritten, default, provider); }
    static bool IntTryParse(ReadOnlySpan<char> span, IFormatProvider? provider, out int result) { return int.TryParse(span, NumberStyles.Integer, provider, out result); }
    #endregion

    #region Public methods
    [TestMethod]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertFrom_UnsupportedType_ReturnsFalse()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsFalse(converter.CanConvertFrom(null, typeof(DateTime)));
    }

    [TestMethod]
    public void CanConvertTo_String_ReturnsTrue()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsTrue(converter.CanConvertTo(null, typeof(string)));
    }

    [TestMethod]
    public void Constructor_ExplicitContext_UsesProvided()
    {
        ConverterContext context = ConverterContext.Default;
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse, context: context);

        Assert.AreSame(context, converter.Context);
    }

    [TestMethod]
    public void Constructor_NullContext_UsesInvariant()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse, context: null);

        Assert.AreSame(ConverterContext.Invariant, converter.Context);
    }

    [TestMethod]
    public void Constructor_NullTryParse_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new SpanBasedTypeConverter<int>(null!)); }
    [TestMethod]
    public void ConvertFrom_CultureParameterIsUsed()
    {
        IFormatProvider? capturedProvider = null;
        SpanParseDelegate<int> tryParse = (ReadOnlySpan<char> span, IFormatProvider? provider, out int result) =>
        {
            capturedProvider = provider;
            return int.TryParse(span, NumberStyles.Integer, provider, out result);
        };
        CultureInfo frCulture = CultureInfo.GetCultureInfo("fr-FR");
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(tryParse);

        converter.ConvertFrom(null, frCulture, "42");

        Assert.AreSame(frCulture, capturedProvider);
    }

    [TestMethod]
    public void ConvertFrom_InvalidString_ThrowsFormatException()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.ThrowsExactly<FormatException>(() => converter.ConvertFrom(null, CultureInfo.InvariantCulture, "not_a_number"));
    }

    [TestMethod]
    public void ConvertFrom_NullCulture_UsesContextCulture()
    {
        IFormatProvider? capturedProvider = null;
        SpanParseDelegate<int> tryParse = (ReadOnlySpan<char> span, IFormatProvider? provider, out int result) =>
        {
            capturedProvider = provider;
            return int.TryParse(span, NumberStyles.Integer, provider, out result);
        };
        ConverterContext context = ConverterContext.Invariant;
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(tryParse, context: context);

        converter.ConvertFrom(null, null, "42");

        Assert.AreSame(CultureInfo.InvariantCulture, capturedProvider);
    }

    [TestMethod]
    public void ConvertFrom_StringWithWhitespace_TrimsWhenAllowed()
    {
        ConverterContext context = new ConverterContext(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: true, allowTrailingWhiteSpace: true);
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse, context: context);

        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "  42  ");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_ValidString_ParsesCorrectly()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "42");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertTo_NullDestinationType_ThrowsArgumentNullException()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.ThrowsExactly<ArgumentNullException>(() => converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, null!));
    }

    [TestMethod]
    public void ConvertTo_ValueNotOfTypeT_FallsBackToBase()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse, IntTryFormat);

        object? result = converter.ConvertTo(null, CultureInfo.InvariantCulture, "hello", typeof(string));

        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    public void ConvertTo_WithoutTryFormat_FallsBackToToString()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        object? result = converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, typeof(string));

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertTo_WithTryFormat_FormatsUsingSpan()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse, IntTryFormat);

        object? result = converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, typeof(string));

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void IsValid_InvalidString_ReturnsFalse()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsFalse(converter.IsValid(null, "xyz"));
    }

    [TestMethod]
    public void IsValid_Null_ReturnsFalse()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsFalse(converter.IsValid(null, null));
    }

    [TestMethod]
    public void IsValid_ValidString_ReturnsTrue()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsTrue(converter.IsValid(null, "42"));
    }

    [TestMethod]
    public void IsValid_ValueOfTypeT_ReturnsTrue()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsTrue(converter.IsValid(null, 42));
    }

    [TestMethod]
    public void TryFormatSpan_InsufficientBuffer_ReturnsFalse()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse, IntTryFormat);
        Span<char> buffer = stackalloc char[1];

        bool result = converter.TryFormatSpan(12345, buffer, out _);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryFormatSpan_WithDelegate_WritesAndReturnsTrue()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse, IntTryFormat);
        Span<char> buffer = stackalloc char[16];

        bool result = converter.TryFormatSpan(42, buffer, out int charsWritten);

        Assert.IsTrue(result);
        Assert.AreEqual("42", new string(buffer[..charsWritten]));
    }

    [TestMethod]
    public void TryFormatSpan_WithoutDelegate_ReturnsFalse()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);
        Span<char> buffer = stackalloc char[16];

        bool result = converter.TryFormatSpan(42, buffer, out int charsWritten);

        Assert.IsFalse(result);
        Assert.AreEqual(0, charsWritten);
    }

    [TestMethod]
    public void TryParseSpan_InvalidInput_ReturnsFalse()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        bool result = converter.TryParseSpan("abc".AsSpan(), out _);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryParseSpan_TrimsWhitespace()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        bool result = converter.TryParseSpan("  42  ".AsSpan(), out int value);

        Assert.IsTrue(result);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryParseSpan_ValidInput_ReturnsTrue()
    {
        SpanBasedTypeConverter<int> converter = new SpanBasedTypeConverter<int>(IntTryParse);

        bool result = converter.TryParseSpan("42".AsSpan(), out int value);

        Assert.IsTrue(result);
        Assert.AreEqual(42, value);
    }
    #endregion
}
