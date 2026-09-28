using System.Buffers;
using System.Text;

namespace Catharsis.IO;

///<summary>
///Enumerates the lines of a large text source without allocating a <see cref="string"/> per line: each line is
///delivered as a <see cref="ReadOnlySpan{T}"/> view into a rented buffer, valid only for the duration of the
///callback. This is why the API is callback-based (via <see cref="ReadOnlySpanAction{T, TArg}"/>) rather than
///<see cref="IEnumerable{T}"/>: a <see cref="ReadOnlySpan{T}"/> cannot be a type argument, so it cannot be the
///element type of a yielded sequence.
///</summary>
///<param name="stream">The stream to read lines from.</param>
///<param name="encoding">The text encoding to use, or <c>null</c> to use UTF-8.</param>
///<param name="bufferSize">The initial size of the rented read buffer, grown automatically for lines longer than it.</param>
///<exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
///<exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is not positive.</exception>
public sealed class LineReader(Stream stream, Encoding? encoding = null, int bufferSize = 4096) : IDisposable
{
    #region Fields
    readonly int _bufferSize = bufferSize > 0 ? bufferSize : throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be positive.");
    readonly Encoding _encoding = encoding ?? Encoding.UTF8;
    readonly Stream _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    #endregion

    #region Private methods
    static ReadOnlySpan<char> TrimTrailingCarriageReturn(ReadOnlySpan<char> line) => ((line.Length > 0) && (line[^1] == '\r')) ? line[..^1] : line;
    #endregion

    #region Public methods
    ///<summary>
    ///Releases the underlying stream.
    ///</summary>
    public void Dispose() => _stream.Dispose();

    ///<summary>
    ///Reads every line from the source, invoking <paramref name="onLine"/> once per line with a span into an
    ///internal rented buffer. The span is only valid for the duration of the call; do not store it.
    ///</summary>
    ///<param name="onLine">The callback invoked for each line, with the line's text and <paramref name="state"/>.</param>
    ///<param name="state">Arbitrary state passed through to every call of <paramref name="onLine"/>.</param>
    ///<typeparam name="TState">The type of <paramref name="state"/>.</typeparam>
    ///<exception cref="ArgumentNullException"><paramref name="onLine"/> is <c>null</c>.</exception>
    public void ReadLines<TState>(ReadOnlySpanAction<char, TState> onLine, TState state)
    {
        ArgumentNullException.ThrowIfNull(onLine);

        using StreamReader reader = new(_stream, _encoding, detectEncodingFromByteOrderMarks: true, _bufferSize, leaveOpen: true);
        char[] buffer = ArrayPool<char>.Shared.Rent(_bufferSize);
        int carryLength = 0;

        try
        {
            int read;

            while((read = reader.Read(buffer, carryLength, buffer.Length - carryLength)) > 0)
            {
                int total = carryLength + read;
                int consumed = 0;

                int newlineIndex;

                while((newlineIndex = Array.IndexOf(buffer, '\n', consumed, total - consumed)) >= 0)
                {
                    onLine(TrimTrailingCarriageReturn(buffer.AsSpan(consumed, newlineIndex - consumed)), state);
                    consumed = newlineIndex + 1;
                }

                carryLength = total - consumed;

                if(carryLength == buffer.Length)
                {
                    char[] biggerBuffer = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                    Array.Copy(buffer, consumed, biggerBuffer, 0, carryLength);
                    ArrayPool<char>.Shared.Return(buffer);
                    buffer = biggerBuffer;
                } else if(carryLength > 0)
                {
                    Array.Copy(buffer, consumed, buffer, 0, carryLength);
                }
            }

            if(carryLength > 0)
            {
                onLine(TrimTrailingCarriageReturn(buffer.AsSpan(0, carryLength)), state);
            }
        } finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
    #endregion
}
