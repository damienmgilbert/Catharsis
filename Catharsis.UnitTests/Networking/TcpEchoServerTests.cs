using Catharsis.Networking;
using System.Net;

namespace Catharsis.UnitTests.Networking;

///<summary>
///Unit tests for the <see cref="TcpEchoServer"/> class, exercised through <see cref="TcpLineClient"/>.
///</summary>
[TestClass]
public class TcpEchoServerTests
{
    #region LocalEndpoint

    [TestMethod]
    public async Task LocalEndpoint_BeforeStart_Throws()
    {
        await using TcpEchoServer server = new();
        Assert.ThrowsExactly<InvalidOperationException>(() => _ = server.LocalEndpoint);
    }

    [TestMethod]
    public async Task LocalEndpoint_AfterStart_HasAssignedPort()
    {
        await using TcpEchoServer server = new();
        server.Start();

        Assert.IsGreaterThan(0, server.LocalEndpoint.Port);
    }

    #endregion

    #region Start

    [TestMethod]
    public async Task Start_CalledTwice_Throws()
    {
        await using TcpEchoServer server = new();
        server.Start();

        Assert.ThrowsExactly<InvalidOperationException>(server.Start);
    }

    #endregion

    #region End-to-end echo

    [TestMethod]
    public async Task Client_SendsLine_ReceivesSameLineBack()
    {
        await using TcpEchoServer server = new();
        server.Start();

        await using TcpLineClient client = new();
        await client.ConnectAsync(server.LocalEndpoint);

        await client.SendLineAsync("hello");
        string? received = await client.ReceiveLineAsync();

        Assert.AreEqual("hello", received);
    }

    [TestMethod]
    public async Task Client_SendsMultipleLines_ReceivesEachBackInOrder()
    {
        await using TcpEchoServer server = new();
        server.Start();

        await using TcpLineClient client = new();
        await client.ConnectAsync(server.LocalEndpoint);

        await client.SendLineAsync("first");
        await client.SendLineAsync("second");

        Assert.AreEqual("first", await client.ReceiveLineAsync());
        Assert.AreEqual("second", await client.ReceiveLineAsync());
    }

    [TestMethod]
    public async Task MultipleClients_EachReceivesOwnEcho()
    {
        await using TcpEchoServer server = new();
        server.Start();

        await using TcpLineClient clientA = new();
        await using TcpLineClient clientB = new();
        await clientA.ConnectAsync(server.LocalEndpoint);
        await clientB.ConnectAsync(server.LocalEndpoint);

        await clientA.SendLineAsync("from-a");
        await clientB.SendLineAsync("from-b");

        Assert.AreEqual("from-a", await clientA.ReceiveLineAsync());
        Assert.AreEqual("from-b", await clientB.ReceiveLineAsync());
    }

    [TestMethod]
    public async Task Client_Disconnects_ServerDoesNotThrow()
    {
        await using TcpEchoServer server = new();
        server.Start();

        TcpLineClient client = new();
        await client.ConnectAsync(server.LocalEndpoint);
        await client.SendLineAsync("hello");
        await client.ReceiveLineAsync();
        await client.DisposeAsync();

        await Task.Delay(100);
    }

    #endregion

    #region Dispose

    [TestMethod]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        TcpEchoServer server = new();
        server.Start();

        await server.DisposeAsync();
        await server.DisposeAsync();
    }

    [TestMethod]
    public async Task DisposeAsync_WithoutStart_DoesNotThrow()
    {
        TcpEchoServer server = new(IPAddress.Loopback);
        await server.DisposeAsync();
    }

    #endregion
}
