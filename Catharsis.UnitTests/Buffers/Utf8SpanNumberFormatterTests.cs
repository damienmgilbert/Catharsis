using Catharsis.Buffers;
using System.Buffers;
using System.Text;

namespace Catharsis.UnitTests.Buffers;

///<summary>
///Unit tests for the <see cref="Utf8SpanNumberFormatter"/> class.
///</summary>
[TestClass]
public class Utf8SpanNumberFormatterTests
{
    #region Format(bool)

    [TestMethod]
    public void Format_Boolean_WritesExpectedBytes()
    {
        ArrayBufferWriter<byte> destination = new();
        int written = Utf8SpanNumberFormatter.Format(destination, true);

        Assert.AreEqual("True", Encoding.UTF8.GetString(destination.WrittenSpan));
        Assert.AreEqual(destination.WrittenCount, written);
    }

    [TestMethod]
    public void Format_Boolean_NullDestination_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => Utf8SpanNumberFormatter.Format(null!, true)); }

    #endregion

    #region Format(int) / Format(long)

    [TestMethod]
    public void Format_Int32_WritesExpectedBytes()
    {
        ArrayBufferWriter<byte> destination = new();
        Utf8SpanNumberFormatter.Format(destination, 42);

        Assert.AreEqual("42", Encoding.UTF8.GetString(destination.WrittenSpan));
    }

    [TestMethod]
    public void Format_Int64_LargeValue_GrowsBufferAsNeeded()
    {
        ArrayBufferWriter<byte> destination = new();
        long value = 123456789012345L;

        Utf8SpanNumberFormatter.Format(destination, value);

        Assert.AreEqual(value.ToString(System.Globalization.CultureInfo.InvariantCulture), Encoding.UTF8.GetString(destination.WrittenSpan));
    }

    #endregion

    #region Format(double) / Format(decimal)

    [TestMethod]
    public void Format_Double_WritesExpectedBytes()
    {
        ArrayBufferWriter<byte> destination = new();
        Utf8SpanNumberFormatter.Format(destination, 3.5);

        Assert.AreEqual("3.5", Encoding.UTF8.GetString(destination.WrittenSpan));
    }

    [TestMethod]
    public void Format_Decimal_WritesExpectedBytes()
    {
        ArrayBufferWriter<byte> destination = new();
        Utf8SpanNumberFormatter.Format(destination, 3.50m);

        Assert.AreEqual("3.50", Encoding.UTF8.GetString(destination.WrittenSpan));
    }

    #endregion

    #region Format(Guid) / Format(DateTime)

    [TestMethod]
    public void Format_Guid_RoundTripsThroughParser()
    {
        Guid value = Guid.NewGuid();
        ArrayBufferWriter<byte> destination = new();

        Utf8SpanNumberFormatter.Format(destination, value, 'D');

        bool parsed = Utf8SpanNumberParser.TryParseGuid(destination.WrittenSpan, out Guid roundTripped, out _, 'D');

        Assert.IsTrue(parsed);
        Assert.AreEqual(value, roundTripped);
    }

    [TestMethod]
    public void Format_DateTime_RoundTripsThroughParser()
    {
        DateTime value = new(2024, 3, 14, 12, 30, 0, DateTimeKind.Utc);
        ArrayBufferWriter<byte> destination = new();

        Utf8SpanNumberFormatter.Format(destination, value, 'O');

        bool parsed = Utf8SpanNumberParser.TryParseDateTime(destination.WrittenSpan, out DateTime roundTripped, out _, 'O');

        Assert.IsTrue(parsed);
        Assert.AreEqual(value, roundTripped);
    }

    #endregion
}
