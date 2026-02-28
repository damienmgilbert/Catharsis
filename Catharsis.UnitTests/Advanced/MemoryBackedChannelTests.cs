using Catharsis.Advanced;

namespace Catharsis.UnitTests.Advanced;

[TestClass]
public class MemoryBackedChannelTests
{
    #region Public methods
    [TestMethod]
    public async Task Complete_And_ReadAll()
    {
        using MemoryBackedChannel<byte> channel = new MemoryBackedChannel<byte>();
        await channel.WriteAsync(new byte[] { 10 });
        await channel.WriteAsync(new byte[] { 20 });
        channel.Complete();

        List<byte> segments = new List<byte>();
        while(channel.Reader.TryRead(out MemoryBackedChannel<byte>.OwnedSegment? seg))
        {
            segments.Add(seg.Memory.Span[0]);
            seg.Dispose();
        }

        CollectionAssert.AreEqual(new byte[] { 10, 20 }, segments);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        MemoryBackedChannel<byte> channel = new MemoryBackedChannel<byte>();
        channel.Dispose();
        channel.Dispose();
    }

    [TestMethod]
    public async Task WriteAsync_And_ReadAsync_RoundTrips()
    {
        using MemoryBackedChannel<byte> channel = new MemoryBackedChannel<byte>();
        byte[] data = [ 1, 2, 3 ];

        await channel.WriteAsync(data);
        using MemoryBackedChannel<byte>.OwnedSegment segment = await channel.ReadAsync();

        Assert.AreEqual(3, segment.Length);
        Assert.AreEqual(1, segment.Memory.Span[0]);
    }
    #endregion
}
