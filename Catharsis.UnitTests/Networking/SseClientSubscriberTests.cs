using Catharsis.Networking;
using System.Net;
using System.Net.ServerSentEvents;
using System.Text;

namespace Catharsis.UnitTests.Networking;

///<summary>
///Unit tests for the <see cref="SseClientSubscriber{T}"/> class.
///</summary>
[TestClass]
public class SseClientSubscriberTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullHttpClient_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new SseClientSubscriber<string>(null!, new Uri("https://a.test"), static (_, data) => Encoding.UTF8.GetString(data))); }

    [TestMethod]
    public void Constructor_NullRequestUri_Throws()
    {
        using HttpClient httpClient = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => new SseClientSubscriber<string>(httpClient, null!, static (_, data) => Encoding.UTF8.GetString(data)));
    }

    [TestMethod]
    public void Constructor_NullItemParser_Throws()
    {
        using HttpClient httpClient = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => new SseClientSubscriber<string>(httpClient, new Uri("https://a.test"), null!));
    }

    #endregion

    #region SubscribeAsync

    [TestMethod]
    public async Task SubscribeAsync_ServerSendsEvent_YieldsParsedItem()
    {
        await using TestSseServer server = TestSseServer.Start("data: hello\n\n");

        using HttpClient httpClient = new();
        SseClientSubscriber<string> subscriber = new(httpClient, server.Uri, static (_, data) => Encoding.UTF8.GetString(data));

        await foreach(SseItem<string> item in subscriber.SubscribeAsync())
        {
            Assert.AreEqual("hello", item.Data);
            break;
        }
    }

    [TestMethod]
    public async Task SubscribeAsync_StreamEndsNaturally_ReconnectsAndSendsLastEventId()
    {
        await using TestSseServer server = TestSseServer.Start("id: 1\ndata: hello\n\n");

        using HttpClient httpClient = new();
        SseClientSubscriber<string> subscriber = new(httpClient, server.Uri, static (_, data) => Encoding.UTF8.GetString(data));

        int received = 0;

        await foreach(SseItem<string> item in subscriber.SubscribeAsync())
        {
            received++;

            if(received == 2)
            {
                break;
            }
        }

        Assert.AreEqual(2, received);
        Assert.AreEqual("1", server.LastReceivedEventIdHeader);
    }

    #endregion

    #region Test infrastructure

    ///<remarks>
    ///Serves a fixed server-sent-event payload on every request, exactly once per connection (via a known
    ///<c>Content-Length</c>), so a subscriber's stream ends naturally after each response and must reconnect to
    ///receive another event. Records the <c>Last-Event-ID</c> request header of the most recent request.
    ///</remarks>
    sealed class TestSseServer : IAsyncDisposable
    {
        readonly HttpListener _listener;
        readonly CancellationTokenSource _stoppingSource = new();
        readonly Task _acceptLoop;
        readonly string _payload;

        TestSseServer(HttpListener listener, Uri uri, string payload)
        {
            _listener = listener;
            Uri = uri;
            _payload = payload;
            _acceptLoop = AcceptLoopAsync(_stoppingSource.Token);
        }

        public Uri Uri { get; }

        public string? LastReceivedEventIdHeader { get; private set; }

        public static TestSseServer Start(string payload)
        {
            int port = GetFreeTcpPort();
            HttpListener listener = new();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();

            return new TestSseServer(listener, new Uri($"http://localhost:{port}/"), payload);
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
                    await HandleRequestAsync(context, stoppingToken).ConfigureAwait(false);
                }
            } catch(Exception exception) when(exception is OperationCanceledException or ObjectDisposedException or HttpListenerException)
            {
            }
        }

        async Task HandleRequestAsync(HttpListenerContext context, CancellationToken stoppingToken)
        {
            LastReceivedEventIdHeader = context.Request.Headers["Last-Event-ID"];

            byte[] bytes = Encoding.UTF8.GetBytes(_payload);
            context.Response.ContentType = "text/event-stream";
            context.Response.ContentLength64 = bytes.Length;

            await context.Response.OutputStream.WriteAsync(bytes, stoppingToken).ConfigureAwait(false);
            context.Response.OutputStream.Close();
            context.Response.Close();
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
