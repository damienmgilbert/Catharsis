using Catharsis.Advanced;
using System.Text;
using System.Threading.Channels;

namespace Catharsis.UnitTests.Advanced;

///<summary>
///Unit tests for the <see cref="PipelineChannelBridge"/> class.
///</summary>
[TestClass]
public class PipelineChannelBridgeTests
{
    #region Pumping

    [TestMethod]
    public async Task WrittenData_IsPumpedIntoChannel()
    {
        await using PipelineChannelBridge bridge = new();

        byte[] payload = "hello"u8.ToArray();
        await bridge.Writer.WriteAsync(payload);
        await bridge.Writer.FlushAsync();

        MemoryBackedChannel<byte>.OwnedSegment segment = await bridge.Reader.ReadAsync();

        using (segment)
        {
            Assert.AreEqual("hello", Encoding.UTF8.GetString(segment.Memory.Span));
        }
    }

    [TestMethod]
    public async Task MultipleWrites_ArePumpedInOrder()
    {
        // Small sequential writes can land in the same underlying pipe buffer segment before the pump reads, so a
        // channel item corresponds to a raw buffer chunk, not a logical write; draining and concatenating verifies
        // ordering is preserved without assuming a particular chunking.
        await using PipelineChannelBridge bridge = new();

        await bridge.Writer.WriteAsync("first"u8.ToArray());
        await bridge.Writer.FlushAsync();
        await bridge.Writer.WriteAsync("second"u8.ToArray());
        await bridge.Writer.FlushAsync();
        await bridge.Writer.CompleteAsync();

        List<byte> received = [];

        await foreach (MemoryBackedChannel<byte>.OwnedSegment segment in bridge.Reader.ReadAllAsync())
        {
            using (segment)
            {
                received.AddRange(segment.Memory.ToArray());
            }
        }

        Assert.AreEqual("firstsecond", Encoding.UTF8.GetString([.. received]));
    }

    [TestMethod]
    public async Task CompletingWriter_CompletesChannel()
    {
        await using PipelineChannelBridge bridge = new();

        await bridge.Writer.CompleteAsync();

        await Assert.ThrowsExactlyAsync<ChannelClosedException>(async () => await bridge.Reader.ReadAsync());
    }

    #endregion

    #region Dispose

    [TestMethod]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        PipelineChannelBridge bridge = new();

        await bridge.DisposeAsync();
        await bridge.DisposeAsync();
    }

    #endregion
}
