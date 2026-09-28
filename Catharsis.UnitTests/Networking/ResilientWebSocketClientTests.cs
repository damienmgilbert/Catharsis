using Catharsis.Networking;
using Catharsis.Resilience;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Catharsis.UnitTests.Networking;

///<summary>
///Unit tests for the <see cref="ResilientWebSocketClient"/> class.
///</summary>
[TestClass]
public class ResilientWebSocketClientTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullRetryPolicy_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ResilientWebSocketClient(null!)); }

    #endregion

    #region ConnectAsync

    [TestMethod]
    public async Task ConnectAsync_NullUri_Throws()
    {
        await using ResilientWebSocketClient client = new(new RetryPolicy());
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => client.ConnectAsync(null!));
    }

    [TestMethod]
    public async Task ConnectAsync_ValidServer_StateBecomesOpen()
    {
        await using TestWebSocketServer server = TestWebSocketServer.Start();
        await using ResilientWebSocketClient client = new(new RetryPolicy());

        await client.ConnectAsync(server.Uri);

        Assert.AreEqual(WebSocketState.Open, client.State);
    }

    #endregion

    #region SendAsync / ReceiveAsync

    [TestMethod]
    public async Task SendAsync_EchoServer_ReceivesSameMessageBack()
    {
        await using TestWebSocketServer server = TestWebSocketServer.Start();
        await using ResilientWebSocketClient client = new(new RetryPolicy());
        await client.ConnectAsync(server.Uri);

        byte[] payload = Encoding.UTF8.GetBytes("hello");
        await client.SendAsync(payload, WebSocketMessageType.Text);

        byte[] buffer = new byte[1024];
        WebSocketReceiveResult result = await client.ReceiveAsync(buffer);

        Assert.AreEqual("hello", Encoding.UTF8.GetString(buffer, 0, result.Count));
    }

    [TestMethod]
    public async Task SendAsync_NeverConnected_Throws()
    {
        await using ResilientWebSocketClient client = new(new RetryPolicy().MaxAttempts(1));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.SendAsync("hi"u8.ToArray(), WebSocketMessageType.Text));
    }

    #endregion

    #region Dispose

    [TestMethod]
    public async Task DisposeAsync_BeforeConnect_DoesNotThrow()
    {
        ResilientWebSocketClient client = new(new RetryPolicy());
        await client.DisposeAsync();
    }

    [TestMethod]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        ResilientWebSocketClient client = new(new RetryPolicy());
        await client.DisposeAsync();
        await client.DisposeAsync();
    }

    [TestMethod]
    public async Task DisposeAsync_AfterConnect_ClosesGracefully()
    {
        await using TestWebSocketServer server = TestWebSocketServer.Start();
        ResilientWebSocketClient client = new(new RetryPolicy());
        await client.ConnectAsync(server.Uri);

        await client.DisposeAsync();
    }

    #endregion

    #region Test infrastructure

    ///<remarks>
    ///<see cref="ClientWebSocket"/> is sealed and cannot be mocked, so a real local WebSocket server is required to
    ///exercise <see cref="ResilientWebSocketClient"/> end-to-end. This minimal echo server is backed by
    ///<see cref="HttpListenerContext.AcceptWebSocketAsync(string?)"/>, bound to <c>localhost</c> so it does not
    ///require administrative URL-ACL reservations.
    ///</remarks>
    sealed class TestWebSocketServer : IAsyncDisposable
    {
        readonly HttpListener _listener;
        readonly CancellationTokenSource _stoppingSource = new();
        readonly Task _acceptLoop;

        TestWebSocketServer(HttpListener listener, Uri uri)
        {
            _listener = listener;
            Uri = uri;
            _acceptLoop = AcceptLoopAsync(_stoppingSource.Token);
        }

        public Uri Uri { get; }

        public static TestWebSocketServer Start()
        {
            int port = GetFreeTcpPort();
            HttpListener listener = new();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();

            return new TestWebSocketServer(listener, new Uri($"ws://localhost:{port}/"));
        }

        static int GetFreeTcpPort()
        {
            using System.Net.Sockets.TcpListener probe = new(IPAddress.Loopback, 0);
            probe.Start();
            int port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();
            return port;
        }

        async Task AcceptLoopAsync(CancellationToken stoppingToken)
        {
            try
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    HttpListenerContext context = await _listener.GetContextAsync().WaitAsync(stoppingToken).ConfigureAwait(false);

                    if(!context.Request.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                        continue;
                    }

                    _ = HandleWebSocketAsync(context, stoppingToken);
                }
            } catch(Exception exception) when(exception is OperationCanceledException or ObjectDisposedException or HttpListenerException)
            {
            }
        }

        static async Task HandleWebSocketAsync(HttpListenerContext context, CancellationToken stoppingToken)
        {
            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);
            WebSocket socket = webSocketContext.WebSocket;

            try
            {
                byte[] buffer = new byte[4096];

                while(socket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, stoppingToken).ConfigureAwait(false);

                    if(result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", stoppingToken).ConfigureAwait(false);
                        break;
                    }

                    await socket.SendAsync(buffer.AsMemory(0, result.Count), result.MessageType, result.EndOfMessage, stoppingToken).ConfigureAwait(false);
                }
            } catch(Exception exception) when(exception is WebSocketException or OperationCanceledException)
            {
            } finally
            {
                socket.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _stoppingSource.CancelAsync().ConfigureAwait(false);
            _listener.Stop();
            _listener.Close();

            try
            {
                await _acceptLoop.ConfigureAwait(false);
            } catch(OperationCanceledException)
            {
            }

            _stoppingSource.Dispose();
        }
    }

    #endregion
}
