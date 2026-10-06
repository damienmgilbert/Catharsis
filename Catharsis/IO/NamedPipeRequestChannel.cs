using System.IO.Pipes;

namespace Catharsis.IO;

///<summary>
///A request/response wrapper over <see cref="NamedPipeServerStream"/>/<see cref="NamedPipeClientStream"/> for
///simple local IPC: the server side reads one line, invokes a handler, and writes back one line; the static
///<see cref="SendRequestAsync"/> helper is the client side, sending one line and waiting for the reply. Named pipes
///map to Unix domain sockets on Linux/macOS, so this stays cross-platform.
///</summary>
public sealed class NamedPipeRequestChannel : IAsyncDisposable
{
    #region Fields
    readonly Func<string, CancellationToken, Task<string>> _handler;
    readonly NamedPipeServerStream _server;
    readonly CancellationTokenSource _stoppingSource = new();
    readonly Task _acceptLoop;
    bool _disposed;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new channel and immediately starts listening for connections in the background.
    ///</summary>
    ///<param name="pipeName">The named pipe to listen on.</param>
    ///<param name="handler">Invoked with each received request line; its return value is sent back as the response line.</param>
    ///<exception cref="ArgumentException"><paramref name="pipeName"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
    public NamedPipeRequestChannel(string pipeName, Func<string, CancellationToken, Task<string>> handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pipeName);
        ArgumentNullException.ThrowIfNull(handler);

        _handler = handler;
        _server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        _acceptLoop = AcceptLoopAsync(_stoppingSource.Token);
    }
    #endregion

    #region Private methods
    async Task AcceptLoopAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _server.WaitForConnectionAsync(stoppingToken).ConfigureAwait(false);

                using (StreamReader reader = new(_server, leaveOpen: true))
                await using (StreamWriter writer = new(_server, leaveOpen: true) { AutoFlush = true })
                {
                    string? request = await reader.ReadLineAsync(stoppingToken).ConfigureAwait(false);

                    if (request is not null)
                    {
                        string response = await _handler(request, stoppingToken).ConfigureAwait(false);
                        await writer.WriteLineAsync(response.AsMemory(), stoppingToken).ConfigureAwait(false);
                    }
                }

                if (_server.IsConnected)
                {
                    _server.Disconnect();
                }
            }
        }
        catch (Exception exception) when (exception is OperationCanceledException or ObjectDisposedException or IOException)
        {
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Stops listening and releases the underlying pipe.
    ///</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        await _stoppingSource.CancelAsync().ConfigureAwait(false);

        try
        {
            await _acceptLoop.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }

        _stoppingSource.Dispose();
        await _server.DisposeAsync().ConfigureAwait(false);
    }

    ///<summary>
    ///Connects to a running <see cref="NamedPipeRequestChannel"/>, sends a single request line, and waits for the
    ///response line.
    ///</summary>
    ///<param name="pipeName">The named pipe to connect to.</param>
    ///<param name="request">The request to send.</param>
    ///<param name="timeout">The connection timeout. Defaults to 5 seconds if not specified.</param>
    ///<param name="cancellationToken">A token to cancel the operation.</param>
    ///<returns>The response line.</returns>
    ///<exception cref="ArgumentException"><paramref name="pipeName"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
    ///<exception cref="IOException">The server closed the connection without responding.</exception>
    public static async Task<string> SendRequestAsync(string pipeName, string request, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pipeName);
        ArgumentNullException.ThrowIfNull(request);

        await using NamedPipeClientStream client = new(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await client.ConnectAsync((int)(timeout ?? TimeSpan.FromSeconds(5)).TotalMilliseconds, cancellationToken).ConfigureAwait(false);

        using StreamReader reader = new(client, leaveOpen: true);
        await using StreamWriter writer = new(client, leaveOpen: true) { AutoFlush = true };

        await writer.WriteLineAsync(request.AsMemory(), cancellationToken).ConfigureAwait(false);
        string? response = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

        return response ?? throw new IOException("The server closed the connection without responding.");
    }
    #endregion
}
