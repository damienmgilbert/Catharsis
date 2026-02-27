using System.Globalization;

using Catharsis.ComponentModel.TypeConverter;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

[TestClass]
public sealed class EnumTypeConverterTests
{
    private enum Color { Red, Green, Blue }

    [Flags]
    private enum Permissions { None = 0, Read = 1, Write = 2, Execute = 4 }

    [TestMethod]
    public void IsFlags_NonFlagsEnum_ReturnsFalse()
    {
        Assert.IsFalse(EnumTypeConverter<Color>.IsFlags);
    }

    [TestMethod]
    public void IsFlags_FlagsEnum_ReturnsTrue()
    {
        Assert.IsTrue(EnumTypeConverter<Permissions>.IsFlags);
    }

    [TestMethod]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertFrom_UnderlyingType_ReturnsTrue()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(int)));
    }

    [TestMethod]
    public void CanConvertFrom_UnsupportedType_ReturnsFalse()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.IsFalse(converter.CanConvertFrom(null, typeof(DateTime)));
    }

    [TestMethod]
    public void CanConvertTo_String_ReturnsTrue()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.IsTrue(converter.CanConvertTo(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertTo_UnderlyingType_ReturnsTrue()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.IsTrue(converter.CanConvertTo(null, typeof(int)));
    }

    [TestMethod]
    public void ConvertFrom_ValidString_ReturnsEnumValue()
    {
        var converter = new EnumTypeConverter<Color>();

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "Green");

        Assert.AreEqual(Color.Green, result);
    }

    [TestMethod]
    public void ConvertFrom_CaseInsensitiveString_ReturnsEnumValue()
    {
        var converter = new EnumTypeConverter<Color>();

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "green");

        Assert.AreEqual(Color.Green, result);
    }

    [TestMethod]
    public void ConvertFrom_StringWithWhitespace_ReturnsEnumValue()
    {
        var converter = new EnumTypeConverter<Color>();

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "  Blue  ");

        Assert.AreEqual(Color.Blue, result);
    }

    [TestMethod]
    public void ConvertFrom_EmptyString_ThrowsFormatException()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.ThrowsExactly<FormatException>(
            () => converter.ConvertFrom(null, CultureInfo.InvariantCulture, ""));
    }

    [TestMethod]
    public void ConvertFrom_WhitespaceOnlyString_ThrowsFormatException()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.ThrowsExactly<FormatException>(
            () => converter.ConvertFrom(null, CultureInfo.InvariantCulture, "   "));
    }

    [TestMethod]
    public void ConvertFrom_InvalidString_ThrowsFormatException()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.ThrowsExactly<FormatException>(
            () => converter.ConvertFrom(null, CultureInfo.InvariantCulture, "Purple"));
    }

    [TestMethod]
    public void ConvertFrom_UnderlyingIntValue_ReturnsEnumValue()
    {
        var converter = new EnumTypeConverter<Color>();

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, 1);

        Assert.AreEqual(Color.Green, result);
    }

    [TestMethod]
    public void ConvertTo_EnumToString_ReturnsName()
    {
        var converter = new EnumTypeConverter<Color>();

        var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, Color.Blue, typeof(string));

        Assert.AreEqual("Blue", result);
    }

    [TestMethod]
    public void ConvertTo_EnumToUnderlyingType_ReturnsIntValue()
    {
        var converter = new EnumTypeConverter<Color>();

        var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, Color.Green, typeof(int));

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void ConvertTo_NullDestinationType_ThrowsArgumentNullException()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => converter.ConvertTo(null, CultureInfo.InvariantCulture, Color.Red, null!));
    }

    [TestMethod]
    public void ConvertFrom_FlagsCommaString_ReturnsCompositeValue()
    {
        var converter = new EnumTypeConverter<Permissions>();

        var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "Read, Write");

        Assert.AreEqual(Permissions.Read | Permissions.Write, result);
    }

    [TestMethod]
    public void ConvertTo_FlagsComposite_ReturnsCommaString()
    {
        var converter = new EnumTypeConverter<Permissions>();

        var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, Permissions.Read | Permissions.Execute, typeof(string));

        Assert.AreEqual("Read, Execute", result);
    }

    [TestMethod]
    public void IsValid_ValidEnumValue_ReturnsTrue()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.IsTrue(converter.IsValid(null, Color.Red));
    }

    [TestMethod]
    public void IsValid_ValidString_ReturnsTrue()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.IsTrue(converter.IsValid(null, "Green"));
    }

    [TestMethod]
    public void IsValid_InvalidString_ReturnsFalse()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.IsFalse(converter.IsValid(null, "Purple"));
    }

    [TestMethod]
    public void IsValid_Null_ReturnsFalse()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.IsFalse(converter.IsValid(null, null));
    }

    [TestMethod]
    public void GetStandardValues_ReturnsAllEnumValues()
    {
        var converter = new EnumTypeConverter<Color>();

        var values = converter.GetStandardValues(null);

        Assert.IsNotNull(values);
        Assert.AreEqual(3, values.Count);
    }

    [TestMethod]
    public void GetStandardValuesSupported_ReturnsTrue()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.IsTrue(converter.GetStandardValuesSupported(null));
    }

    [TestMethod]
    public void GetStandardValuesExclusive_NonFlags_ReturnsTrue()
    {
        var converter = new EnumTypeConverter<Color>();

        Assert.IsTrue(converter.GetStandardValuesExclusive(null));
    }

    [TestMethod]
    public void GetStandardValuesExclusive_Flags_ReturnsFalse()
    {
        var converter = new EnumTypeConverter<Permissions>();

        Assert.IsFalse(converter.GetStandardValuesExclusive(null));
    }

    [TestMethod]
    public void TryParseSpan_ValidSpan_ReturnsTrue()
    {
        var result = EnumTypeConverter<Color>.TryParseSpan("Green".AsSpan(), out var value);

        Assert.IsTrue(result);
        Assert.AreEqual(Color.Green, value);
    }

    [TestMethod]
    public void TryParseSpan_InvalidSpan_ReturnsFalse()
    {
        var result = EnumTypeConverter<Color>.TryParseSpan("Purple".AsSpan(), out _);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ParseSpan_ValidSpan_ReturnsValue()
    {
        var result = EnumTypeConverter<Color>.ParseSpan("Blue".AsSpan());

        Assert.AreEqual(Color.Blue, result);
    }

    [TestMethod]
    public void ParseSpan_InvalidSpan_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(
            () => EnumTypeConverter<Color>.ParseSpan("Invalid".AsSpan()));
    }

    [TestMethod]
    public void FormatEnum_ReturnsName()
    {
        var result = EnumTypeConverter<Color>.FormatEnum(Color.Red);

        Assert.AreEqual("Red", result);
    }

    [TestMethod]
    public void TryFormatSpan_SufficientBuffer_WritesAndReturnsTrue()
    {
        Span<char> buffer = stackalloc char[16];

        var result = EnumTypeConverter<Color>.TryFormatSpan(Color.Green, buffer, out var charsWritten);

        Assert.IsTrue(result);
        Assert.AreEqual("Green", new string(buffer[..charsWritten]));
    }

    [TestMethod]
    public void TryFormatSpan_InsufficientBuffer_ReturnsFalse()
    {
        Span<char> buffer = stackalloc char[2];

        var result = EnumTypeConverter<Color>.TryFormatSpan(Color.Green, buffer, out _);

        Assert.IsFalse(result);
    }
}
