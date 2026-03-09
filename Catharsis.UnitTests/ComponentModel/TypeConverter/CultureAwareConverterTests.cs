using System.Globalization;
using Catharsis.ComponentModel.TypeConverter;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

///<summary>
///Unit tests for the <see cref="CultureAwareConverter"/> class.
///</summary>
[TestClass]
public sealed class CultureAwareConverterTests
{
    #region Public methods
    [TestMethod]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>();

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertFrom_UnsupportedType_ReturnsFalse()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>();

        Assert.IsFalse(converter.CanConvertFrom(null, typeof(DateTime)));
    }

    [TestMethod]
    public void CanConvertTo_String_ReturnsTrue()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>();

        Assert.IsTrue(converter.CanConvertTo(null, typeof(string)));
    }

    [TestMethod]
    public void Constructor_ExplicitContext_UsesProvided()
    {
        ConverterContext context = ConverterContext.Invariant;
        CultureAwareConverter<double> converter = new CultureAwareConverter<double>(context);

        Assert.AreSame(context, converter.Context);
    }

    [TestMethod]
    public void Constructor_NullContext_UsesDefault()
    {
        CultureAwareConverter<double> converter = new CultureAwareConverter<double>(context: null);

        Assert.AreSame(ConverterContext.Default, converter.Context);
    }

    [TestMethod]
    public void ConvertFrom_CultureParameterOverridesContext()
    {
        ConverterContext context = new ConverterContext(CultureInfo.GetCultureInfo("fr-FR"));
        CultureAwareConverter<double> converter = new CultureAwareConverter<double>(context);

        // Passing invariant culture should override the French context
        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "3.14");

        Assert.AreEqual(3.14, result);
    }

    [TestMethod]
    public void ConvertFrom_FrenchCulture_ParsesDecimalWithComma()
    {
        CultureInfo frCulture = CultureInfo.GetCultureInfo("fr-FR");
        ConverterContext context = new ConverterContext(frCulture);
        CultureAwareConverter<double> converter = new CultureAwareConverter<double>(context);

        object? result = converter.ConvertFrom(null, frCulture, "3,14");

        Assert.AreEqual(3.14, result);
    }

    [TestMethod]
    public void ConvertFrom_InvalidString_ThrowsFormatException()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.ThrowsExactly<FormatException>(() => converter.ConvertFrom(null, CultureInfo.InvariantCulture, "not_a_number"));
    }

    [TestMethod]
    public void ConvertFrom_InvariantCulture_ParsesCorrectly()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "42");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_NullCulture_UsesContextCulture()
    {
        ConverterContext context = new ConverterContext(CultureInfo.InvariantCulture);
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(context);

        object? result = converter.ConvertFrom(null, null, "99");

        Assert.AreEqual(99, result);
    }

    [TestMethod]
    public void ConvertFrom_TrimsLeadingOnly_WhenOnlyLeadingAllowed()
    {
        ConverterContext context = new ConverterContext(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: true, allowTrailingWhiteSpace: false);
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(context);

        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "  42");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_TrimsTrailingOnly_WhenOnlyTrailingAllowed()
    {
        ConverterContext context = new ConverterContext(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: false, allowTrailingWhiteSpace: true);
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(context);

        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "42  ");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertFrom_TrimsWhitespace_WhenBothAllowed()
    {
        ConverterContext context = new ConverterContext(CultureInfo.InvariantCulture, allowLeadingWhiteSpace: true, allowTrailingWhiteSpace: true);
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(context);

        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "  42  ");

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ConvertTo_DoubleWithFormatString_FormatsCorrectly()
    {
        ConverterContext context = new ConverterContext(CultureInfo.InvariantCulture, "F2");
        CultureAwareConverter<double> converter = new CultureAwareConverter<double>(context);

        object? result = converter.ConvertTo(null, CultureInfo.InvariantCulture, 3.14159, typeof(string));

        Assert.AreEqual("3.14", result);
    }

    [TestMethod]
    public void ConvertTo_IntToString_FormatsCorrectly()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        object? result = converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, typeof(string));

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ConvertTo_NullDestinationType_ThrowsArgumentNullException()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.ThrowsExactly<ArgumentNullException>(() => converter.ConvertTo(null, CultureInfo.InvariantCulture, 42, null!));
    }

    [TestMethod]
    public void IsValid_InvalidString_ReturnsFalse()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.IsFalse(converter.IsValid(null, "xyz"));
    }

    [TestMethod]
    public void IsValid_Null_ReturnsFalse()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.IsFalse(converter.IsValid(null, null));
    }

    [TestMethod]
    public void IsValid_ValidString_ReturnsTrue()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.IsTrue(converter.IsValid(null, "42"));
    }

    [TestMethod]
    public void IsValid_ValueOfTypeT_ReturnsTrue()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        Assert.IsTrue(converter.IsValid(null, 42));
    }

    [TestMethod]
    public void TryFormatSpan_InsufficientBuffer_ReturnsFalse()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);
        Span<char> buffer = stackalloc char[1];

        bool result = converter.TryFormatSpan(12345, buffer, out _);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryFormatSpan_SufficientBuffer_WritesAndReturnsTrue()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);
        Span<char> buffer = stackalloc char[16];

        bool result = converter.TryFormatSpan(42, buffer, out int charsWritten);

        Assert.IsTrue(result);
        Assert.AreEqual("42", new string(buffer[..charsWritten]));
    }

    [TestMethod]
    public void TryParseSpan_InvalidInput_ReturnsFalse()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        bool result = converter.TryParseSpan("abc".AsSpan(), out _);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryParseSpan_TrimsWhitespace()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        bool result = converter.TryParseSpan("  42  ".AsSpan(), out int value);

        Assert.IsTrue(result);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryParseSpan_ValidInput_ReturnsTrue()
    {
        CultureAwareConverter<int> converter = new CultureAwareConverter<int>(ConverterContext.Invariant);

        bool result = converter.TryParseSpan("42".AsSpan(), out int value);

        Assert.IsTrue(result);
        Assert.AreEqual(42, value);
    }
    #endregion
}
