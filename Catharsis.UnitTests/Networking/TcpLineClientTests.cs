using Catharsis.Networking;

namespace Catharsis.UnitTests.Networking;

///<summary>
///Unit tests for the <see cref="TcpLineClient"/> class not already covered end-to-end by <see cref="TcpEchoServerTests"/>.
///</summary>
[TestClass]
public class TcpLineClientTests
{
    #region ConnectAsync

    [TestMethod]
    public async Task ConnectAsync_NullEndpoint_Throws()
    {
        await using TcpLineClient client = new();
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => client.ConnectAsync(null!));
    }

    #endregion

    #region SendLineAsync / ReceiveLineAsync before connecting

    [TestMethod]
    public async Task SendLineAsync_NotConnected_Throws()
    {
        await using TcpLineClient client = new();
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.SendLineAsync("hello"));
    }

    [TestMethod]
    public async Task ReceiveLineAsync_NotConnected_Throws()
    {
        await using TcpLineClient client = new();
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.ReceiveLineAsync());
    }

    [TestMethod]
    public async Task SendLineAsync_NullLine_Throws()
    {
        await using TcpEchoServer server = new();
        server.Start();

        await using TcpLineClient client = new();
        await client.ConnectAsync(server.LocalEndpoint);

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => client.SendLineAsync(null!));
    }

    #endregion

    #region Dispose

    [TestMethod]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        TcpLineClient client = new();
        await client.DisposeAsync();
        await client.DisposeAsync();
    }

    [TestMethod]
    public async Task DisposeAsync_AfterConnect_DoesNotThrow()
    {
        await using TcpEchoServer server = new();
        server.Start();

        TcpLineClient client = new();
        await client.ConnectAsync(server.LocalEndpoint);
        await client.DisposeAsync();
    }

    #endregion
}
