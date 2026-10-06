using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Catharsis.Networking;

///<summary>
///A minimal line-protocol TCP client for local services and tests: connects to an endpoint and exchanges
///newline-delimited text, the client-side counterpart to <see cref="TcpEchoServer"/>.
///</summary>
public sealed class TcpLineClient : IAsyncDisposable
{
    #region Fields
    readonly TcpClient _client = new();
    StreamReader? _reader;
    StreamWriter? _writer;
    bool _disposed;
    #endregion

    #region Private methods
    void EnsureConnected()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if((_reader is null) || (_writer is null))
        {
            throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Connects to the given endpoint.
    ///</summary>
    ///<param name="endpoint">The endpoint to connect to.</param>
    ///<param name="cancellationToken">A token to cancel the connection attempt.</param>
    ///<exception cref="ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
    ///<exception cref="ObjectDisposedException">This instance has already been disposed.</exception>
    public async Task ConnectAsync(IPEndPoint endpoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _client.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);

        NetworkStream stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
    }

    ///<summary>
    ///Sends a single line of text, terminated by a newline.
    ///</summary>
    ///<param name="line">The line to send.</param>
    ///<param name="cancellationToken">A token to cancel the send.</param>
    ///<exception cref="ArgumentNullException"><paramref name="line"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException"><see cref="ConnectAsync"/> has not been called.</exception>
    public async Task SendLineAsync(string line, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(line);
        EnsureConnected();

        await _writer!.WriteLineAsync(line.AsMemory(), cancellationToken).ConfigureAwait(false);
    }

    ///<summary>
    ///Reads a single line of text.
    ///</summary>
    ///<param name="cancellationToken">A token to cancel the read.</param>
    ///<returns>The line read, or <c>null</c> if the remote end closed the connection.</returns>
    ///<exception cref="InvalidOperationException"><see cref="ConnectAsync"/> has not been called.</exception>
    public async Task<string?> ReceiveLineAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        return await _reader!.ReadLineAsync(cancellationToken).ConfigureAwait(false);
    }

    ///<summary>
    ///Closes the connection and releases all resources.
    ///</summary>
    public async ValueTask DisposeAsync()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;

        _reader?.Dispose();

        if(_writer is not null)
        {
            await _writer.DisposeAsync().ConfigureAwait(false);
        }

        _client.Dispose();
    }
    #endregion
}
