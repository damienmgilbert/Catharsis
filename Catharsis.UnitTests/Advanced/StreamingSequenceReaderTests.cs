using System.Buffers;
using Catharsis;
using Catharsis.Advanced;

namespace Catharsis.UnitTests.Advanced;

[TestClass]
public class StreamingSequenceReaderTests
{
    [TestMethod]
    public async Task ReadAllAsync_ReadsEntireStream()
    {
        using StreamingSequenceReader reader = new StreamingSequenceReader();
        byte[] data = [1, 2, 3, 4, 5];
        using MemoryStream stream = new MemoryStream(data);

        ReadOnlySequence<byte> sequence = await reader.ReadAllAsync(stream);

        Assert.AreEqual(5, sequence.Length);
        Assert.AreEqual(5, reader.TotalBytesRead);
    }

    [TestMethod]
    public async Task ReadAllAsync_EmptyStream()
    {
        using StreamingSequenceReader reader = new StreamingSequenceReader();
        using MemoryStream stream = new MemoryStream([]);

        ReadOnlySequence<byte> sequence = await reader.ReadAllAsync(stream);

        Assert.AreEqual(0, sequence.Length);
    }

    [TestMethod]
    public void Reset_ClearsTotalBytesRead()
    {
        using StreamingSequenceReader reader = new StreamingSequenceReader();
        reader.Reset();
        Assert.AreEqual(0, reader.TotalBytesRead);
    }
}
