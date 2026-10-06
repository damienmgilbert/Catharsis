using System.IO.Pipelines;

namespace Catharsis.Advanced;

///<summary>
///Bundles a <see cref="PipeReader"/> and <see cref="PipeWriter"/> created over the same duplex <see cref="Stream"/>
///into a single disposable unit. The BCL's own <see cref="PipeReader.Create(Stream, StreamPipeReaderOptions)"/> and
public sealed class PipeStreamAdapter : IAsyncDisposable
{
    #region Fields
    private bool _disposed;
    private readonly bool _leaveOpen;
    private readonly Stream _stream;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new adapter over the given duplex stream.
    ///</summary>
    ///<param name="stream">The duplex stream (e.g. a <see cref="System.Net.Sockets.NetworkStream"/>) to adapt.</param>
    ///<param name="leaveOpen">Whether disposing this adapter should leave <paramref name="stream"/> open.</param>
    ///<exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    public PipeStreamAdapter(Stream stream, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);

        _stream = stream;
        _leaveOpen = leaveOpen;
        Reader = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: true));
        Writer = PipeWriter.Create(stream, new StreamPipeWriterOptions(leaveOpen: true));
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Completes the reader and writer, then disposes the underlying stream unless <c>leaveOpen</c> was specified.
    ///</summary>
    public async ValueTask DisposeAsync()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;

        await Reader.CompleteAsync().ConfigureAwait(false);
        await Writer.CompleteAsync().ConfigureAwait(false);

        if(!_leaveOpen)
        {
            await _stream.DisposeAsync().ConfigureAwait(false);
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///The pipe reader side of the adapted stream.
    ///</summary>
    public PipeReader Reader { get; }

    ///<summary>
    ///The pipe writer side of the adapted stream.
    ///</summary>
    public PipeWriter Writer { get; }
    #endregion
}
