using System.Buffers;
using System.Buffers.Text;

namespace Catharsis.Buffers;

///<summary>
///Static helpers over <see cref="Utf8Formatter"/> for formatting numeric and related values directly to UTF-8 bytes in
///an <see cref="IBufferWriter{Byte}"/>, growing the destination automatically rather than requiring the caller to pre-
///size a destination span and retry on failure — the write-side complement to ///<see cref="Utf8SpanNumberParser"/>,
///and distinct from the binary-primitive-focused <see cref="SpanReader"/>/ ///<see cref="SpanWriter"/>.
///</summary>
public static class Utf8SpanNumberFormatter
{
    #region Constants
    private const int InitialBufferSize = 32;
    #endregion

    #region Delegates
    private delegate bool TryFormat<T>(T value, Span<byte> destination, out int bytesWritten, StandardFormat format);
    #endregion

    #region Private methods
    private static int WriteGrowing<T>(IBufferWriter<byte> destination, T value, StandardFormat format, TryFormat<T> tryFormat)
    {
        ArgumentNullException.ThrowIfNull(destination);

        int size = InitialBufferSize;

        while(true)
        {
            Span<byte> span = destination.GetSpan(size);

            if(tryFormat(value, span, out int bytesWritten, format))
            {
                destination.Advance(bytesWritten);
                return bytesWritten;
            }

            size *= 2;
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Formats a <see cref="bool"/> as UTF-8 bytes into <paramref name="destination"/>.
    ///</summary>
    ///<returns>The number of bytes written.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public static int Format(IBufferWriter<byte> destination, bool value, StandardFormat format = default) => WriteGrowing(destination, value, format, Utf8Formatter.TryFormat);

    ///<summary>
    ///Formats a <see cref="DateTime"/> as UTF-8 bytes into <paramref name="destination"/>.
    ///</summary>
    ///<returns>The number of bytes written.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public static int Format(IBufferWriter<byte> destination, DateTime value, StandardFormat format = default) => WriteGrowing(destination, value, format, Utf8Formatter.TryFormat);

    ///<summary>
    ///Formats a <see cref="decimal"/> as UTF-8 bytes into <paramref name="destination"/>.
    ///</summary>
    ///<returns>The number of bytes written.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public static int Format(IBufferWriter<byte> destination, decimal value, StandardFormat format = default) => WriteGrowing(destination, value, format, Utf8Formatter.TryFormat);

    ///<summary>
    ///Formats a <see cref="double"/> as UTF-8 bytes into <paramref name="destination"/>.
    ///</summary>
    ///<returns>The number of bytes written.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public static int Format(IBufferWriter<byte> destination, double value, StandardFormat format = default) => WriteGrowing(destination, value, format, Utf8Formatter.TryFormat);

    ///<summary>
    ///Formats a <see cref="Guid"/> as UTF-8 bytes into <paramref name="destination"/>.
    ///</summary>
    ///<returns>The number of bytes written.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public static int Format(IBufferWriter<byte> destination, Guid value, StandardFormat format = default) => WriteGrowing(destination, value, format, Utf8Formatter.TryFormat);

    ///<summary>
    ///Formats an <see cref="int"/> as UTF-8 bytes into <paramref name="destination"/>.
    ///</summary>
    ///<returns>The number of bytes written.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public static int Format(IBufferWriter<byte> destination, int value, StandardFormat format = default) => WriteGrowing(destination, value, format, Utf8Formatter.TryFormat);

    ///<summary>
    ///Formats a <see cref="long"/> as UTF-8 bytes into <paramref name="destination"/>.
    ///</summary>
    ///<returns>The number of bytes written.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public static int Format(IBufferWriter<byte> destination, long value, StandardFormat format = default) => WriteGrowing(destination, value, format, Utf8Formatter.TryFormat);
    #endregion
}
