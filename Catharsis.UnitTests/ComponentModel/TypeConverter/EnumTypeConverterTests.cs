using System.Globalization;
using Catharsis.ComponentModel.TypeConverter;

namespace Catharsis.UnitTests.ComponentModel.TypeConverter;

///<summary>
///Unit tests for the <see cref="EnumTypeConverter"/> class.
///</summary>
[TestClass]
public sealed class EnumTypeConverterTests
{
    #region Public methods
    [TestMethod]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertFrom_UnderlyingType_ReturnsTrue()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(int)));
    }

    [TestMethod]
    public void CanConvertFrom_UnsupportedType_ReturnsFalse()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.IsFalse(converter.CanConvertFrom(null, typeof(DateTime)));
    }

    [TestMethod]
    public void CanConvertTo_String_ReturnsTrue()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.IsTrue(converter.CanConvertTo(null, typeof(string)));
    }

    [TestMethod]
    public void CanConvertTo_UnderlyingType_ReturnsTrue()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.IsTrue(converter.CanConvertTo(null, typeof(int)));
    }

    [TestMethod]
    public void ConvertFrom_CaseInsensitiveString_ReturnsEnumValue()
    {
        EnumTypeConverter<Color> converter = new();

        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "green");

        Assert.AreEqual(Color.Green, result);
    }

    [TestMethod]
    public void ConvertFrom_EmptyString_ThrowsFormatException()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.ThrowsExactly<FormatException>(() => converter.ConvertFrom(null, CultureInfo.InvariantCulture, string.Empty));
    }

    [TestMethod]
    public void ConvertFrom_FlagsCommaString_ReturnsCompositeValue()
    {
        EnumTypeConverter<Permissions> converter = new();

        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "Read, Write");

        Assert.AreEqual(Permissions.Read | Permissions.Write, result);
    }

    [TestMethod]
    public void ConvertFrom_InvalidString_ThrowsFormatException()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.ThrowsExactly<FormatException>(() => converter.ConvertFrom(null, CultureInfo.InvariantCulture, "Purple"));
    }

    [TestMethod]
    public void ConvertFrom_StringWithWhitespace_ReturnsEnumValue()
    {
        EnumTypeConverter<Color> converter = new();

        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "  Blue  ");

        Assert.AreEqual(Color.Blue, result);
    }

    [TestMethod]
    public void ConvertFrom_UnderlyingIntValue_ReturnsEnumValue()
    {
        EnumTypeConverter<Color> converter = new();

        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, 1);

        Assert.AreEqual(Color.Green, result);
    }

    [TestMethod]
    public void ConvertFrom_ValidString_ReturnsEnumValue()
    {
        EnumTypeConverter<Color> converter = new();

        object? result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "Green");

        Assert.AreEqual(Color.Green, result);
    }

    [TestMethod]
    public void ConvertFrom_WhitespaceOnlyString_ThrowsFormatException()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.ThrowsExactly<FormatException>(() => converter.ConvertFrom(null, CultureInfo.InvariantCulture, "   "));
    }

    [TestMethod]
    public void ConvertTo_EnumToString_ReturnsName()
    {
        EnumTypeConverter<Color> converter = new();

        object? result = converter.ConvertTo(null, CultureInfo.InvariantCulture, Color.Blue, typeof(string));

        Assert.AreEqual("Blue", result);
    }

    [TestMethod]
    public void ConvertTo_EnumToUnderlyingType_ReturnsIntValue()
    {
        EnumTypeConverter<Color> converter = new();

        object? result = converter.ConvertTo(null, CultureInfo.InvariantCulture, Color.Green, typeof(int));

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void ConvertTo_FlagsComposite_ReturnsCommaString()
    {
        EnumTypeConverter<Permissions> converter = new();

        object? result = converter.ConvertTo(null, CultureInfo.InvariantCulture, Permissions.Read | Permissions.Execute, typeof(string));

        Assert.AreEqual("Read, Execute", result);
    }

    [TestMethod]
    public void ConvertTo_NullDestinationType_ThrowsArgumentNullException()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => converter.ConvertTo(null, CultureInfo.InvariantCulture, Color.Red, null!));
    }

    [TestMethod]
    public void FormatEnum_ReturnsName()
    {
        string result = EnumTypeConverter<Color>.FormatEnum(Color.Red);

        Assert.AreEqual("Red", result);
    }

    [TestMethod]
    public void GetStandardValues_ReturnsAllEnumValues()
    {
        EnumTypeConverter<Color> converter = new();

        System.ComponentModel.TypeConverter.StandardValuesCollection values = converter.GetStandardValues(null);

        Assert.IsNotNull(values);
        Assert.HasCount(3, values);
    }

    [TestMethod]
    public void GetStandardValuesExclusive_Flags_ReturnsFalse()
    {
        EnumTypeConverter<Permissions> converter = new();

        Assert.IsFalse(converter.GetStandardValuesExclusive(null));
    }

    [TestMethod]
    public void GetStandardValuesExclusive_NonFlags_ReturnsTrue()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.IsTrue(converter.GetStandardValuesExclusive(null));
    }

    [TestMethod]
    public void GetStandardValuesSupported_ReturnsTrue()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.IsTrue(converter.GetStandardValuesSupported(null));
    }

    [TestMethod]
    public void IsFlags_FlagsEnum_ReturnsTrue() { Assert.IsTrue(EnumTypeConverter<Permissions>.IsFlags); }
    [TestMethod]
    public void IsFlags_NonFlagsEnum_ReturnsFalse() { Assert.IsFalse(EnumTypeConverter<Color>.IsFlags); }
    [TestMethod]
    public void IsValid_InvalidString_ReturnsFalse()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.IsFalse(converter.IsValid(null, "Purple"));
    }

    [TestMethod]
    public void IsValid_Null_ReturnsFalse()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.IsFalse(converter.IsValid(null, null));
    }

    [TestMethod]
    public void IsValid_ValidEnumValue_ReturnsTrue()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.IsTrue(converter.IsValid(null, Color.Red));
    }

    [TestMethod]
    public void IsValid_ValidString_ReturnsTrue()
    {
        EnumTypeConverter<Color> converter = new();

        Assert.IsTrue(converter.IsValid(null, "Green"));
    }

    [TestMethod]
    public void ParseSpan_InvalidSpan_ThrowsFormatException() { Assert.ThrowsExactly<FormatException>(static () => EnumTypeConverter<Color>.ParseSpan("Invalid".AsSpan())); }
    [TestMethod]
    public void ParseSpan_ValidSpan_ReturnsValue()
    {
        Color result = EnumTypeConverter<Color>.ParseSpan("Blue".AsSpan());

        Assert.AreEqual(Color.Blue, result);
    }

    [TestMethod]
    public void TryFormatSpan_InsufficientBuffer_ReturnsFalse()
    {
        Span<char> buffer = stackalloc char[2];

        bool result = EnumTypeConverter<Color>.TryFormatSpan(Color.Green, buffer, out _);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryFormatSpan_SufficientBuffer_WritesAndReturnsTrue()
    {
        Span<char> buffer = stackalloc char[16];

        bool result = EnumTypeConverter<Color>.TryFormatSpan(Color.Green, buffer, out int charsWritten);

        Assert.IsTrue(result);
        Assert.AreEqual("Green", new string(buffer[..charsWritten]));
    }

    [TestMethod]
    public void TryParseSpan_InvalidSpan_ReturnsFalse()
    {
        bool result = EnumTypeConverter<Color>.TryParseSpan("Purple".AsSpan(), out _);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryParseSpan_ValidSpan_ReturnsTrue()
    {
        bool result = EnumTypeConverter<Color>.TryParseSpan("Green".AsSpan(), out Color value);

        Assert.IsTrue(result);
        Assert.AreEqual(Color.Green, value);
    }
    #endregion

    enum Color
    {
        Red,
        Green,
        Blue
    }

    [Flags]
    enum Permissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4
    }
}
