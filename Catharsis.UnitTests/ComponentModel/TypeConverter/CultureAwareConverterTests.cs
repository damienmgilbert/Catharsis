using System.Globalization;

using Catharsis.ComponentModel.TypeConverter;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

[TestClass]
public sealed class CultureAwareConverterTests
{
    [TestMethod]
    public void Constructor_NullContext_UsesDefault()
    {
        var converter = new CultureAwareConverter<double>(context: null);

        Assert.AreSame(ConverterContext.Default, converter.Context);
    }

    [TestMethod]
    public void Constructor_ExplicitContext_UsesProvided()
    {
        var context = ConverterContext.Invariant;
        var converter = new CultureAwareConverter<double>(context);

        Assert.AreSame(context, converter.Context);
    }

    [TestMethod]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        var converter = new CultureAwareConverter<int>();

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertFrom_UnsupportedType_ReturnsFalse()
    {
        var converter = new CultureAwareConverter<int>();

        Assert.IsFalse(converter.CanConvertFrom(null, typeof(DateTime)));
    }

    [TestMethod]
    public void CanConvertTo_String_ReturnsTrue()
    {
        var converter = new CultureAwareConverter<int>();

        Assert.IsTrue(converter.CanConvertTo(null, typeof(string)));
    }

    [TestMethod]
    public void ConvertFrom_InvariantCulture_ParsesCorrectly()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "42");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_FrenchCulture_ParsesDecimalWithComma()
    {
        var frCulture = CultureInfo.GetCultureInfo("fr-FR");
        var context = new ConverterContext(frCulture);
        var converter = new CultureAwareConverter<double>(context);

        var result = converter.ConvertFrom(null, frCulture, "3,14");

        Assert.AreEqual(3.14, result);
    }

    [TestMethod]
    public void ConvertFrom_CultureParameterOverridesContext()
    {
        var context = new ConverterContext(CultureInfo.GetCultureInfo("fr-FR"));
        var converter = new CultureAwareConverter<double>(context);

        // Passing invariant culture should override the French context
        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "3.14");

        Assert.AreEqual(3.14, result);
    }

    [TestMethod]
    public void ConvertFrom_NullCulture_UsesContextCulture()
    {
        var context = new ConverterContext(CultureInfo.InvariantCulture);
        var converter = new CultureAwareConverter<int>(context);

        var result = converter.ConvertFrom(null, null, "99");

        Assert.AreEqual(99, result);
    }

    [TestMethod]
    public void ConvertFrom_TrimsWhitespace_WhenBothAllowed()
    {
        var context = new ConverterContext(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: true, allowTrailingWhiteSpace: true);
        var converter = new CultureAwareConverter<int>(context);

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "  42  ");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_TrimsLeadingOnly_WhenOnlyLeadingAllowed()
    {
        var context = new ConverterContext(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: true, allowTrailingWhiteSpace: false);
        var converter = new CultureAwareConverter<int>(context);

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "  42");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_TrimsTrailingOnly_WhenOnlyTrailingAllowed()
    {
        var context = new ConverterContext(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: false, allowTrailingWhiteSpace: true);
        var converter = new CultureAwareConverter<int>(context);

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "42  ");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_InvalidString_ThrowsFormatException()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.ThrowsExactly<FormatException>(
            () => converter.ConvertFrom(null, CultureInfo.InvariantCulture, "not_a_number"));
    }

    [TestMethod]
    public void ConvertTo_IntToString_FormatsCorrectly()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, typeof(string));

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertTo_DoubleWithFormatString_FormatsCorrectly()
    {
        var context = new ConverterContext(CultureInfo.InvariantCulture, "F2");
        var converter = new CultureAwareConverter<double>(context);

        var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, 3.14159, typeof(string));

        Assert.AreEqual("3.14", result);
    }

    [TestMethod]
    public void ConvertTo_NullDestinationType_ThrowsArgumentNullException()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.ThrowsExactly<ArgumentNullException>(
            () => converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, null!));
    }

    [TestMethod]
    public void IsValid_ValueOfTypeT_ReturnsTrue()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.IsTrue(converter.IsValid(null, 42));
    }

    [TestMethod]
    public void IsValid_ValidString_ReturnsTrue()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.IsTrue(converter.IsValid(null, "42"));
    }

    [TestMethod]
    public void IsValid_InvalidString_ReturnsFalse()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.IsFalse(converter.IsValid(null, "xyz"));
    }

    [TestMethod]
    public void IsValid_Null_ReturnsFalse()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.IsFalse(converter.IsValid(null, null));
    }

    [TestMethod]
    public void TryParseSpan_ValidInput_ReturnsTrue()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        var result = converter.TryParseSpan("42".AsSpan(), out var value);

        Assert.IsTrue(result);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryParseSpan_InvalidInput_ReturnsFalse()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        var result = converter.TryParseSpan("abc".AsSpan(), out _);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryParseSpan_TrimsWhitespace()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        var result = converter.TryParseSpan("  42  ".AsSpan(), out var value);

        Assert.IsTrue(result);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryFormatSpan_SufficientBuffer_WritesAndReturnsTrue()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);
        Span<char> buffer = stackalloc char[16];

        var result = converter.TryFormatSpan(42, buffer, out var charsWritten);

        Assert.IsTrue(result);
        Assert.AreEqual("42", new string(buffer[..charsWritten]));
    }

    [TestMethod]
    public void TryFormatSpan_InsufficientBuffer_ReturnsFalse()
    {
        var converter = new CultureAwareConverter<int>(ConverterContext.Invariant);
        Span<char> buffer = stackalloc char[1];

        var result = converter.TryFormatSpan(12345, buffer, out _);

        Assert.IsFalse(result);
    }
}
