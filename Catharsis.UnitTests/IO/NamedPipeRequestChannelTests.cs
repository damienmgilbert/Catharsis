using Catharsis.IO;

namespace Catharsis.UnitTests.IO;

///<summary>
///Unit tests for the <see cref="NamedPipeRequestChannel"/> class.
///</summary>
[TestClass]
public class NamedPipeRequestChannelTests
{
    #region Private methods
    static string UniquePipeName() => $"catharsis-test-{Guid.NewGuid():N}";
    #endregion

    #region Constructor

    [TestMethod]
    public void Constructor_NullPipeName_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new NamedPipeRequestChannel(null!, static (_, _) => Task.FromResult(""))); }

    [TestMethod]
    public void Constructor_NullHandler_Throws() { Assert.ThrowsExactly<ArgumentNullException>(() => new NamedPipeRequestChannel(UniquePipeName(), null!)); }

    #endregion

    #region SendRequestAsync validation

    [TestMethod]
    public async Task SendRequestAsync_NullPipeName_Throws() { await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => NamedPipeRequestChannel.SendRequestAsync(null!, "request")); }

    [TestMethod]
    public async Task SendRequestAsync_NullRequest_Throws() { await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => NamedPipeRequestChannel.SendRequestAsync(UniquePipeName(), null!)); }

    [TestMethod]
    public async Task SendRequestAsync_NoServerListening_ThrowsOnConnect()
    {
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => NamedPipeRequestChannel.SendRequestAsync(UniquePipeName(), "request", TimeSpan.FromMilliseconds(200)));
    }

    #endregion

    #region End-to-end request/response

    [TestMethod]
    public async Task SendRequestAsync_EchoHandler_ReturnsTransformedResponse()
    {
        string pipeName = UniquePipeName();
        await using NamedPipeRequestChannel channel = new(pipeName, static (request, _) => Task.FromResult(request.ToUpperInvariant()));

        string response = await NamedPipeRequestChannel.SendRequestAsync(pipeName, "hello");

        Assert.AreEqual("HELLO", response);
    }

    [TestMethod]
    public async Task SendRequestAsync_MultipleSequentialRequests_EachHandledIndependently()
    {
        string pipeName = UniquePipeName();
        await using NamedPipeRequestChannel channel = new(pipeName, static (request, _) => Task.FromResult($"echo:{request}"));

        string first = await NamedPipeRequestChannel.SendRequestAsync(pipeName, "one");
        string second = await NamedPipeRequestChannel.SendRequestAsync(pipeName, "two");

        Assert.AreEqual("echo:one", first);
        Assert.AreEqual("echo:two", second);
    }

    #endregion

    #region Dispose

    [TestMethod]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        NamedPipeRequestChannel channel = new(UniquePipeName(), static (request, _) => Task.FromResult(request));

        await channel.DisposeAsync();
        await channel.DisposeAsync();
    }

    #endregion
}
