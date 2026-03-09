using Catharsis.Mvvm;

namespace Catharsis.UnitTests.Mvvm;

///<summary>
///Unit tests for the <see cref="AsyncBufferLoader"/> class.
///</summary>
[TestClass]
public class AsyncBufferLoaderTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_Default_InitialState()
    {
        using AsyncBufferLoader loader = new AsyncBufferLoader();
        Assert.AreEqual(0, loader.BytesLoaded);
        Assert.IsFalse(loader.IsLoading);
        Assert.AreEqual(0.0, loader.LoadProgress);
        Assert.IsTrue(loader.Data.IsEmpty);
    }

    [TestMethod]
    public async Task LoadAsync_EmptyStream()
    {
        using AsyncBufferLoader loader = new AsyncBufferLoader();
        using MemoryStream stream = new MemoryStream([]);

        await loader.LoadAsync(stream);

        Assert.AreEqual(0, loader.BytesLoaded);
    }

    [TestMethod]
    public async Task LoadAsync_LoadsFromStream()
    {
        using AsyncBufferLoader loader = new AsyncBufferLoader();
        byte[] data = [1, 2, 3, 4, 5];
        using MemoryStream stream = new MemoryStream(data);

        await loader.LoadAsync(stream, bufferSize: 4096);

        Assert.AreEqual(5, loader.BytesLoaded);
        Assert.IsFalse(loader.IsLoading);
        Assert.AreEqual(5, loader.Data.Length);
    }
    #endregion
}
