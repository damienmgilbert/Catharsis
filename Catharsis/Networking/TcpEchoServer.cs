using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Catharsis.Networking;

///<summary>
///A minimal line-protocol TCP echo server for local services and tests: accepts any number of concurrent connections
///and echoes every line it receives back to the same client, verbatim, until the client disconnects.
///</summary>
///<param name="address">The local address to bind to. Defaults to <see cref="IPAddress.Loopback"/>.</param>
///<param name="port">The local port to bind to. Defaults to <c>0</c>, letting the OS assign an ephemeral port.</param>
public sealed class TcpEchoServer(IPAddress? address = null, int port = 0) : IAsyncDisposable
{
    #region Fields
    private Task? _acceptLoop;
    private bool _disposed;
    private readonly TcpListener _listener = new(address ?? IPAddress.Loopback, port);
    private bool _started;
    private readonly CancellationTokenSource _stoppingSource = new();
    #endregion

    #region Private methods
    private async Task AcceptLoopAsync(CancellationToken stoppingToken)
    {
        try
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
                _ = HandleClientAsync(client, stoppingToken);
            }
        } catch(Exception exception) when(exception is OperationCanceledException or ObjectDisposedException or SocketException)
        {
        }
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
    {
        try
        {
            using(client)
            {
                await using NetworkStream stream = client.GetStream();
                using StreamReader reader = new(stream, Encoding.UTF8);
                await using StreamWriter writer = new(stream, Encoding.UTF8) { AutoFlush = true };

                string? line;
                while((line = await reader.ReadLineAsync(stoppingToken).ConfigureAwait(false)) is not null)
                {
                    await writer.WriteLineAsync(line.AsMemory(), stoppingToken).ConfigureAwait(false);
                }
            }
        } catch(Exception exception) when(exception is IOException or SocketException or ObjectDisposedException or OperationCanceledException)
        {
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Stops accepting new connections and waits for the accept loop to finish.
    ///</summary>
    public async ValueTask DisposeAsync()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;

        await _stoppingSource.CancelAsync().ConfigureAwait(false);
        _listener.Stop();

        if(_acceptLoop is not null)
        {
            try
            {
                await _acceptLoop.ConfigureAwait(false);
            } catch(OperationCanceledException)
            {
            }
        }

        _stoppingSource.Dispose();
    }

        ///<summary>
///Binds the listening socket and starts accepting connections in the background.
///</summary>
    ///<exception cref="ObjectDisposedException">This instance has already been disposed.</exception>
    ///<exception cref="InvalidOperationException">The server has already been started.</exception>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if(_started)
        {
            throw new InvalidOperationException("The server has already been started.");
        }

        _started = true;
        _listener.Start();
        _acceptLoop = AcceptLoopAsync(_stoppingSource.Token);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///The endpoint this server is bound to, including the actual port assigned by the OS if <c>0</c> was requested.
    ///</summary>
    ///<exception cref="InvalidOperationException">The server has not been started yet.</exception>
    public IPEndPoint LocalEndpoint => _started ? (IPEndPoint)_listener.LocalEndpoint : throw new InvalidOperationException("The server has not been started yet.");
    #endregion
}
