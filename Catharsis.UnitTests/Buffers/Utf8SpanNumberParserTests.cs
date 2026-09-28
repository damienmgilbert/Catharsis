using Catharsis.Buffers;
using System.Text;

namespace Catharsis.UnitTests.Buffers;

///<summary>
///Unit tests for the <see cref="Utf8SpanNumberParser"/> class.
///</summary>
[TestClass]
public class Utf8SpanNumberParserTests
{
    #region TryParseBoolean

    [TestMethod]
    public void TryParseBoolean_True_ReturnsTrueAndValue()
    {
        bool parsed = Utf8SpanNumberParser.TryParseBoolean(Encoding.UTF8.GetBytes("True"), out bool value, out int consumed);

        Assert.IsTrue(parsed);
        Assert.IsTrue(value);
        Assert.AreEqual(4, consumed);
    }

    [TestMethod]
    public void TryParseBoolean_Invalid_ReturnsFalse()
    {
        bool parsed = Utf8SpanNumberParser.TryParseBoolean(Encoding.UTF8.GetBytes("nope"), out _, out _);
        Assert.IsFalse(parsed);
    }

    #endregion

    #region TryParseInt32 / TryParseInt64

    [TestMethod]
    public void TryParseInt32_ValidDigits_ReturnsTrueAndValue()
    {
        bool parsed = Utf8SpanNumberParser.TryParseInt32(Encoding.UTF8.GetBytes("42"), out int value, out int consumed);

        Assert.IsTrue(parsed);
        Assert.AreEqual(42, value);
        Assert.AreEqual(2, consumed);
    }

    [TestMethod]
    public void TryParseInt32_NonNumeric_ReturnsFalse()
    {
        bool parsed = Utf8SpanNumberParser.TryParseInt32(Encoding.UTF8.GetBytes("abc"), out _, out _);
        Assert.IsFalse(parsed);
    }

    [TestMethod]
    public void TryParseInt64_ValidDigits_ReturnsTrueAndValue()
    {
        bool parsed = Utf8SpanNumberParser.TryParseInt64(Encoding.UTF8.GetBytes("9000000000"), out long value, out _);

        Assert.IsTrue(parsed);
        Assert.AreEqual(9000000000L, value);
    }

    #endregion

    #region TryParseDouble / TryParseDecimal

    [TestMethod]
    public void TryParseDouble_ValidNumber_ReturnsTrueAndValue()
    {
        bool parsed = Utf8SpanNumberParser.TryParseDouble(Encoding.UTF8.GetBytes("3.5"), out double value, out _);

        Assert.IsTrue(parsed);
        Assert.AreEqual(3.5, value);
    }

    [TestMethod]
    public void TryParseDecimal_ValidNumber_ReturnsTrueAndValue()
    {
        bool parsed = Utf8SpanNumberParser.TryParseDecimal(Encoding.UTF8.GetBytes("3.50"), out decimal value, out _);

        Assert.IsTrue(parsed);
        Assert.AreEqual(3.50m, value);
    }

    #endregion

    #region TryParseGuid / TryParseDateTime

    [TestMethod]
    public void TryParseGuid_ValidGuid_ReturnsTrueAndValue()
    {
        Guid expected = Guid.NewGuid();
        bool parsed = Utf8SpanNumberParser.TryParseGuid(Encoding.UTF8.GetBytes(expected.ToString("D")), out Guid value, out _, 'D');

        Assert.IsTrue(parsed);
        Assert.AreEqual(expected, value);
    }

    [TestMethod]
    public void TryParseDateTime_ValidRoundTripFormat_ReturnsTrueAndValue()
    {
        DateTime expected = new(2024, 3, 14, 12, 30, 0, DateTimeKind.Utc);
        bool parsed = Utf8SpanNumberParser.TryParseDateTime(Encoding.UTF8.GetBytes(expected.ToString("O")), out DateTime value, out _, 'O');

        Assert.IsTrue(parsed);
        Assert.AreEqual(expected, value);
    }

    #endregion
}
