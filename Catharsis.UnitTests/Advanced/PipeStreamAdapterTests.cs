using Catharsis.Advanced;
using System.Text;

namespace Catharsis.UnitTests.Advanced;

///<summary>
///Unit tests for the <see cref="PipeStreamAdapter"/> class.
///</summary>
[TestClass]
public class PipeStreamAdapterTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullStream_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new PipeStreamAdapter(null!)); }

    #endregion

    #region Reader / Writer

    [TestMethod]
    public async Task Writer_WrittenBytes_AreVisibleThroughReader()
    {
        MemoryStream stream = new();
        PipeStreamAdapter adapter = new(stream, leaveOpen: true);

        byte[] payload = "hello"u8.ToArray();
        await adapter.Writer.WriteAsync(payload);
        await adapter.Writer.FlushAsync();

        await adapter.DisposeAsync();

        Assert.AreEqual("hello", Encoding.UTF8.GetString(stream.ToArray()));
    }

    #endregion

    #region DisposeAsync

    [TestMethod]
    public async Task DisposeAsync_LeaveOpenTrue_DoesNotDisposeStream()
    {
        MemoryStream stream = new();
        PipeStreamAdapter adapter = new(stream, leaveOpen: true);

        await adapter.DisposeAsync();

        Assert.IsTrue(stream.CanRead);
    }

    [TestMethod]
    public async Task DisposeAsync_LeaveOpenFalse_DisposesStream()
    {
        MemoryStream stream = new();
        PipeStreamAdapter adapter = new(stream);

        await adapter.DisposeAsync();

        Assert.ThrowsExactly<ObjectDisposedException>(() => stream.ReadByte());
    }

    [TestMethod]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        PipeStreamAdapter adapter = new(new MemoryStream());

        await adapter.DisposeAsync();
        await adapter.DisposeAsync();
    }

    #endregion
}
