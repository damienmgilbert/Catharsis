using System.Net.WebSockets;
using Catharsis.Resilience;

namespace Catharsis.Networking;

///<summary>
///Wraps a <see cref="ClientWebSocket"/> with <see cref="Resilience.RetryPolicy"/>-driven reconnect: a failed ///<see
///cref="SendAsync"/> call reconnects to the last-used URI and resends, according to the supplied policy's retry/backoff
///configuration.
///</summary>
///<param name="retryPolicy">The retry policy governing reconnect-and-resend behavior.</param>
///<exception cref="ArgumentNullException"><paramref name="retryPolicy"/> is <c>null</c>.</exception>
public sealed class ResilientWebSocketClient(RetryPolicy retryPolicy) : IAsyncDisposable
{
    #region Fields
    private bool _disposed;
    private readonly RetryPolicy _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
    private ClientWebSocket _socket = new();
    private Uri? _uri;
    #endregion

    #region Private methods
    private async Task ReconnectAsync(CancellationToken cancellationToken)
    {
        if(_uri is null)
        {
            throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
        }

        _socket.Dispose();
        _socket = new ClientWebSocket();
        await _socket.ConnectAsync(_uri, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Connects to the given URI.
    ///</summary>
    ///<param name="uri">The <c>ws://</c> or <c>wss://</c> URI to connect to.</param>
    ///<param name="cancellationToken">A token to cancel the connection attempt.</param>
    ///<exception cref="ArgumentNullException"><paramref name="uri"/> is <c>null</c>.</exception>
    ///<exception cref="ObjectDisposedException">This instance has already been disposed.</exception>
    public Task ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ObjectDisposedException.ThrowIf(_disposed, this);

        _uri = uri;
        return _socket.ConnectAsync(uri, cancellationToken);
    }

    ///<summary>
    ///Closes the connection gracefully, if open, and releases all resources.
    ///</summary>
    public async ValueTask DisposeAsync()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;

        if(_socket.State == WebSocketState.Open)
        {
            try
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None).ConfigureAwait(false);
            } catch(WebSocketException)
            {
            }
        }

        _socket.Dispose();
    }

    ///<summary>
    ///Receives a message fragment directly from the underlying socket, without retry.
    ///</summary>
    ///<param name="buffer">The buffer to receive into.</param>
    ///<param name="cancellationToken">A token to cancel the operation.</param>
    ///<exception cref="ObjectDisposedException">This instance has already been disposed.</exception>
    public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _socket.ReceiveAsync(buffer, cancellationToken);
    }

    ///<summary>
    ///Sends a message, reconnecting and resending according to the configured <see cref="Resilience.RetryPolicy"/> if
    ///the socket is not open or the send fails.
    ///</summary>
    ///<param name="buffer">The message payload.</param>
    ///<param name="messageType">The WebSocket message type.</param>
    ///<param name="endOfMessage">Whether this buffer completes the message.</param>
    ///<param name="cancellationToken">A token to cancel the operation.</param>
    ///<exception cref="ObjectDisposedException">This instance has already been disposed.</exception>
    ///<exception cref="InvalidOperationException"><see cref="ConnectAsync"/> has not been called.</exception>
    public Task SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage = true, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _retryPolicy.ExecuteAsync(
               async token =>
               {
                   if(_socket.State != WebSocketState.Open)
                   {
                       await ReconnectAsync(token).ConfigureAwait(false);
                   }

                   await _socket.SendAsync(buffer, messageType, endOfMessage, token).ConfigureAwait(false);
               },
               cancellationToken);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///The underlying socket's current state.
    ///</summary>
    public WebSocketState State => _socket.State;
    #endregion
}
