using System.Buffers.Text;

namespace Catharsis.Buffers;

///<summary>
///Static helpers over <see cref="Utf8Parser"/> for parsing UTF-8-encoded numeric and related text directly from a
///<see cref="ReadOnlySpan{Byte}"/>, without first decoding to a <see cref="string"/> or <see cref="char"/> span —
///the read-side complement to <see cref="Utf8SpanNumberFormatter"/>, and distinct from the binary-primitive-focused
///<see cref="SpanReader"/>/<see cref="SpanWriter"/>.
///</summary>
public static class Utf8SpanNumberParser
{
    #region Public methods
    ///<summary>
    ///Attempts to parse a <see cref="bool"/> from the start of <paramref name="utf8Text"/>.
    ///</summary>
    public static bool TryParseBoolean(ReadOnlySpan<byte> utf8Text, out bool value, out int bytesConsumed) => Utf8Parser.TryParse(utf8Text, out value, out bytesConsumed);

    ///<summary>
    ///Attempts to parse a <see cref="DateTime"/> from the start of <paramref name="utf8Text"/>.
    ///</summary>
    public static bool TryParseDateTime(ReadOnlySpan<byte> utf8Text, out DateTime value, out int bytesConsumed, char standardFormat = default) => Utf8Parser.TryParse(utf8Text, out value, out bytesConsumed, standardFormat);

    ///<summary>
    ///Attempts to parse a <see cref="decimal"/> from the start of <paramref name="utf8Text"/>.
    ///</summary>
    public static bool TryParseDecimal(ReadOnlySpan<byte> utf8Text, out decimal value, out int bytesConsumed, char standardFormat = default) => Utf8Parser.TryParse(utf8Text, out value, out bytesConsumed, standardFormat);

    ///<summary>
    ///Attempts to parse a <see cref="double"/> from the start of <paramref name="utf8Text"/>.
    ///</summary>
    public static bool TryParseDouble(ReadOnlySpan<byte> utf8Text, out double value, out int bytesConsumed, char standardFormat = default) => Utf8Parser.TryParse(utf8Text, out value, out bytesConsumed, standardFormat);

    ///<summary>
    ///Attempts to parse a <see cref="Guid"/> from the start of <paramref name="utf8Text"/>.
    ///</summary>
    public static bool TryParseGuid(ReadOnlySpan<byte> utf8Text, out Guid value, out int bytesConsumed, char standardFormat = default) => Utf8Parser.TryParse(utf8Text, out value, out bytesConsumed, standardFormat);

    ///<summary>
    ///Attempts to parse an <see cref="int"/> from the start of <paramref name="utf8Text"/>.
    ///</summary>
    public static bool TryParseInt32(ReadOnlySpan<byte> utf8Text, out int value, out int bytesConsumed, char standardFormat = default) => Utf8Parser.TryParse(utf8Text, out value, out bytesConsumed, standardFormat);

    ///<summary>
    ///Attempts to parse a <see cref="long"/> from the start of <paramref name="utf8Text"/>.
    ///</summary>
    public static bool TryParseInt64(ReadOnlySpan<byte> utf8Text, out long value, out int bytesConsumed, char standardFormat = default) => Utf8Parser.TryParse(utf8Text, out value, out bytesConsumed, standardFormat);
    #endregion
}
