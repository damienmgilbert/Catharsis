using Catharsis.Networking;
using System.Net.ServerSentEvents;
using System.Text;

namespace Catharsis.UnitTests.Networking;

///<summary>
///Unit tests for the <see cref="SseEventStreamWriter{T}"/> class.
///</summary>
[TestClass]
public class SseEventStreamWriterTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullDestination_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new SseEventStreamWriter<int>(null!)); }

    #endregion

    #region WriteAsync

    [TestMethod]
    public async Task WriteAsync_NullEvents_Throws()
    {
        using MemoryStream stream = new();
        SseEventStreamWriter<int> writer = new(stream);

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => writer.WriteAsync(null!));
    }

    [TestMethod]
    public async Task WriteAsync_SingleItem_WritesJsonEncodedDataLine()
    {
        using MemoryStream stream = new();
        SseEventStreamWriter<int> writer = new(stream);

        await writer.WriteAsync(CreateItems(new SseItem<int>(42)));

        string text = Encoding.UTF8.GetString(stream.ToArray());
        StringAssert.Contains(text, "data: 42");
    }

    [TestMethod]
    public async Task WriteAsync_ItemWithEventType_WritesEventLine()
    {
        using MemoryStream stream = new();
        SseEventStreamWriter<string> writer = new(stream);

        await writer.WriteAsync(CreateItems(new SseItem<string>("payload", "update")));

        string text = Encoding.UTF8.GetString(stream.ToArray());
        StringAssert.Contains(text, "event: update");
        StringAssert.Contains(text, "data: \"payload\"");
    }

    [TestMethod]
    public async Task WriteAsync_MultipleItems_WritesEachItemsData()
    {
        using MemoryStream stream = new();
        SseEventStreamWriter<int> writer = new(stream);

        await writer.WriteAsync(CreateItems(new SseItem<int>(1), new SseItem<int>(2)));

        string text = Encoding.UTF8.GetString(stream.ToArray());
        StringAssert.Contains(text, "data: 1");
        StringAssert.Contains(text, "data: 2");
    }

    #endregion

    #region Private methods
    static async IAsyncEnumerable<SseItem<T>> CreateItems<T>(params SseItem<T>[] items)
    {
        foreach (SseItem<T> item in items)
        {
            yield return item;
            await Task.Yield();
        }
    }
    #endregion
}
