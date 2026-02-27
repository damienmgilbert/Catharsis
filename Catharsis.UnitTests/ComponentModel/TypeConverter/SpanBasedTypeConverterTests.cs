using System.Globalization;

using Catharsis.ComponentModel.TypeConverter;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

[TestClass]
public sealed class SpanBasedTypeConverterTests
{
    private static bool IntTryParse(ReadOnlySpan<char> span, IFormatProvider? provider, out int result) =>
        int.TryParse(span, NumberStyles.Integer, provider, out result);

    private static bool IntTryFormat(int value, Span<char> destination, IFormatProvider? provider, out int charsWritten) =>
        value.TryFormat(destination, out charsWritten, default, provider);

    [TestMethod]
    public void Constructor_NullTryParse_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new SpanBasedTypeConverter<int>(null!));
    }

    [TestMethod]
    public void Constructor_NullContext_UsesInvariant()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse, context: null);

        Assert.AreSame(ConverterContext.Invariant, converter.Context);
    }

    [TestMethod]
    public void Constructor_ExplicitContext_UsesProvided()
    {
        var context = ConverterContext.Default;
        var converter = new SpanBasedTypeConverter<int>(IntTryParse, context: context);

        Assert.AreSame(context, converter.Context);
    }

    [TestMethod]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertFrom_UnsupportedType_ReturnsFalse()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsFalse(converter.CanConvertFrom(null, typeof(DateTime)));
    }

    [TestMethod]
    public void CanConvertTo_String_ReturnsTrue()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsTrue(converter.CanConvertTo(null, typeof(string)));
    }

    [TestMethod]
    public void ConvertFrom_ValidString_ParsesCorrectly()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "42");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_StringWithWhitespace_TrimsWhenAllowed()
    {
        var context = new ConverterContext(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: true, allowTrailingWhiteSpace: true);
        var converter = new SpanBasedTypeConverter<int>(IntTryParse, context: context);

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "  42  ");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_InvalidString_ThrowsFormatException()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.ThrowsExactly<FormatException>(
            () => converter.ConvertFrom(null, CultureInfo.InvariantCulture, "not_a_number"));
    }

    [TestMethod]
    public void ConvertFrom_CultureParameterIsUsed()
    {
        IFormatProvider? capturedProvider = null;
        SpanParseDelegate<int> tryParse = (ReadOnlySpan<char> span, IFormatProvider? provider, out int result) =>
        {
            capturedProvider = provider;
            return int.TryParse(span, NumberStyles.Integer, provider, out result);
        };
        var frCulture = CultureInfo.GetCultureInfo("fr-FR");
        var converter = new SpanBasedTypeConverter<int>(tryParse);

        converter.ConvertFrom(null, frCulture, "42");

        Assert.AreSame(frCulture, capturedProvider);
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
        var context = ConverterContext.Invariant;
        var converter = new SpanBasedTypeConverter<int>(tryParse, context: context);

        converter.ConvertFrom(null, null, "42");

        Assert.AreSame(CultureInfo.InvariantCulture, capturedProvider);
    }

    [TestMethod]
    public void ConvertTo_WithTryFormat_FormatsUsingSpan()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse, IntTryFormat);

        var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, typeof(string));

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertTo_WithoutTryFormat_FallsBackToToString()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, typeof(string));

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertTo_NullDestinationType_ThrowsArgumentNullException()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.ThrowsExactly<ArgumentNullException>(
            () => converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, null!));
    }

    [TestMethod]
    public void ConvertTo_ValueNotOfTypeT_FallsBackToBase()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse, IntTryFormat);

        var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, "hello", typeof(string));

        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    public void IsValid_ValueOfTypeT_ReturnsTrue()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsTrue(converter.IsValid(null, 42));
    }

    [TestMethod]
    public void IsValid_ValidString_ReturnsTrue()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsTrue(converter.IsValid(null, "42"));
    }

    [TestMethod]
    public void IsValid_InvalidString_ReturnsFalse()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsFalse(converter.IsValid(null, "xyz"));
    }

    [TestMethod]
    public void IsValid_Null_ReturnsFalse()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        Assert.IsFalse(converter.IsValid(null, null));
    }

    [TestMethod]
    public void TryParseSpan_ValidInput_ReturnsTrue()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        var result = converter.TryParseSpan("42".AsSpan(), out var value);

        Assert.IsTrue(result);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryParseSpan_InvalidInput_ReturnsFalse()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        var result = converter.TryParseSpan("abc".AsSpan(), out _);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryParseSpan_TrimsWhitespace()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);

        var result = converter.TryParseSpan("  42  ".AsSpan(), out var value);

        Assert.IsTrue(result);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryFormatSpan_WithDelegate_WritesAndReturnsTrue()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse, IntTryFormat);
        Span<char> buffer = stackalloc char[16];

        var result = converter.TryFormatSpan(42, buffer, out var charsWritten);

        Assert.IsTrue(result);
        Assert.AreEqual("42", new string(buffer[..charsWritten]));
    }

    [TestMethod]
    public void TryFormatSpan_WithoutDelegate_ReturnsFalse()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse);
        Span<char> buffer = stackalloc char[16];

        var result = converter.TryFormatSpan(42, buffer, out var charsWritten);

        Assert.IsFalse(result);
        Assert.AreEqual(0, charsWritten);
    }

    [TestMethod]
    public void TryFormatSpan_InsufficientBuffer_ReturnsFalse()
    {
        var converter = new SpanBasedTypeConverter<int>(IntTryParse, IntTryFormat);
        Span<char> buffer = stackalloc char[1];

        var result = converter.TryFormatSpan(12345, buffer, out _);

        Assert.IsFalse(result);
    }
}
