using Catharsis.Mvvm;

namespace Catharsis.UnitTests.Mvvm;

///<summary>
///Unit tests for the <see cref="ObservablePooledBuffer"/> class.
///</summary>
[TestClass]
public class ObservablePooledBufferTests
{
    #region Public methods
    [TestMethod]
    public void Clear_ResetsCount()
    {
        using ObservablePooledBuffer<byte> buf = new();
        buf.Write([ 1, 2, 3 ]);
        buf.Clear();
        Assert.AreEqual(0, buf.Count);
    }

    [TestMethod]
    public void Constructor_Default_InitialState()
    {
        using ObservablePooledBuffer<int> buf = new();
        Assert.AreEqual(0, buf.Count);
        Assert.IsGreaterThanOrEqualTo(256, buf.Capacity);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        ObservablePooledBuffer<int> buf = new();
        buf.Dispose();
        buf.Dispose();
    }

    [TestMethod]
    public void PropertyChanged_FiredOnWrite()
    {
        using ObservablePooledBuffer<int> buf = new();
        List<string> changedProps = [];
        buf.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName!);

        buf.Write([ 42 ]);

        Assert.Contains("Count", changedProps);
    }

    [TestMethod]
    public void ToArray_ReturnsCopy()
    {
        using ObservablePooledBuffer<int> buf = new();
        buf.Write([ 10, 20, 30 ]);
        int[] arr = buf.ToArray();
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, arr);
    }

    [TestMethod]
    public void Write_GrowsBufferAutomatically()
    {
        using ObservablePooledBuffer<byte> buf = new(4);
        byte[] data = new byte[100];
        buf.Write(data);
        Assert.AreEqual(100, buf.Count);
    }

    [TestMethod]
    public void Write_IncreasesCount()
    {
        using ObservablePooledBuffer<byte> buf = new();
        buf.Write([ 1, 2, 3 ]);
        Assert.AreEqual(3, buf.Count);
    }

    [TestMethod]
    public void WrittenMemory_ReturnsWrittenData()
    {
        using ObservablePooledBuffer<byte> buf = new();
        buf.Write([ 10, 20 ]);
        ReadOnlyMemory<byte> mem = buf.WrittenMemory;
        Assert.AreEqual(2, mem.Length);
        Assert.AreEqual(10, mem.Span[0]);
    }
    #endregion
}
