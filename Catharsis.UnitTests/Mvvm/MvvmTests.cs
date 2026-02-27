namespace Catharsis.Mvvm.UnitTests;

[TestClass]
public class AsyncBufferLoaderTests
{
    [TestMethod]
    public void Constructor_Default_InitialState()
    {
        using var loader = new AsyncBufferLoader();
        Assert.AreEqual(0, loader.BytesLoaded);
        Assert.IsFalse(loader.IsLoading);
        Assert.AreEqual(0.0, loader.LoadProgress);
        Assert.IsTrue(loader.Data.IsEmpty);
    }

    [TestMethod]
    public async Task LoadAsync_LoadsFromStream()
    {
        using var loader = new AsyncBufferLoader();
        byte[] data = [1, 2, 3, 4, 5];
        using var stream = new MemoryStream(data);

        await loader.LoadAsync(stream, bufferSize: 4096);

        Assert.AreEqual(5, loader.BytesLoaded);
        Assert.IsFalse(loader.IsLoading);
        Assert.AreEqual(5, loader.Data.Length);
    }

    [TestMethod]
    public async Task LoadAsync_EmptyStream()
    {
        using var loader = new AsyncBufferLoader();
        using var stream = new MemoryStream([]);

        await loader.LoadAsync(stream);

        Assert.AreEqual(0, loader.BytesLoaded);
    }
}

[TestClass]
public class ObservablePooledBufferTests
{
    [TestMethod]
    public void Constructor_Default_InitialState()
    {
        using var buf = new ObservablePooledBuffer<int>();
        Assert.AreEqual(0, buf.Count);
        Assert.IsTrue(buf.Capacity >= 256);
    }

    [TestMethod]
    public void Write_IncreasesCount()
    {
        using var buf = new ObservablePooledBuffer<byte>();
        buf.Write([1, 2, 3]);
        Assert.AreEqual(3, buf.Count);
    }

    [TestMethod]
    public void WrittenMemory_ReturnsWrittenData()
    {
        using var buf = new ObservablePooledBuffer<byte>();
        buf.Write([10, 20]);
        var mem = buf.WrittenMemory;
        Assert.AreEqual(2, mem.Length);
        Assert.AreEqual(10, mem.Span[0]);
    }

    [TestMethod]
    public void Clear_ResetsCount()
    {
        using var buf = new ObservablePooledBuffer<byte>();
        buf.Write([1, 2, 3]);
        buf.Clear();
        Assert.AreEqual(0, buf.Count);
    }

    [TestMethod]
    public void ToArray_ReturnsCopy()
    {
        using var buf = new ObservablePooledBuffer<int>();
        buf.Write([10, 20, 30]);
        int[] arr = buf.ToArray();
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, arr);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var buf = new ObservablePooledBuffer<int>();
        buf.Dispose();
        buf.Dispose();
    }

    [TestMethod]
    public void Write_GrowsBufferAutomatically()
    {
        using var buf = new ObservablePooledBuffer<byte>(4);
        byte[] data = new byte[100];
        buf.Write(data);
        Assert.AreEqual(100, buf.Count);
    }

    [TestMethod]
    public void PropertyChanged_FiredOnWrite()
    {
        using var buf = new ObservablePooledBuffer<int>();
        var changedProps = new List<string>();
        buf.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName!);

        buf.Write([42]);

        Assert.IsTrue(changedProps.Contains("Count"));
    }
}

[TestClass]
public class BufferViewModelBaseTests
{
    private sealed class TestBufferViewModel : BufferViewModelBase
    {
        public bool LoadCoreCalled { get; private set; }
        public bool ShouldThrow { get; set; }
        public bool ShouldCancel { get; set; }

        protected override Task LoadCoreAsync(CancellationToken cancellationToken)
        {
            LoadCoreCalled = true;

            if (ShouldCancel)
                throw new OperationCanceledException();

            if (ShouldThrow)
                throw new InvalidOperationException("Test error");

            DataSize = 100;
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task LoadAsync_SetsHasDataOnSuccess()
    {
        using var vm = new TestBufferViewModel();
        await vm.LoadAsync();
        Assert.IsTrue(vm.LoadCoreCalled);
        Assert.IsTrue(vm.HasData);
        Assert.AreEqual(100, vm.DataSize);
        Assert.IsFalse(vm.IsLoading);
        Assert.IsFalse(vm.HasError);
    }

    [TestMethod]
    public async Task LoadAsync_SetsErrorMessageOnException()
    {
        using var vm = new TestBufferViewModel { ShouldThrow = true };
        await vm.LoadAsync();
        Assert.IsTrue(vm.HasError);
        Assert.AreEqual("Test error", vm.ErrorMessage);
        Assert.IsFalse(vm.HasData);
    }

    [TestMethod]
    public async Task LoadAsync_SetsCancelledMessage()
    {
        using var vm = new TestBufferViewModel { ShouldCancel = true };
        await vm.LoadAsync();
        Assert.IsTrue(vm.HasError);
        Assert.AreEqual("Operation was cancelled.", vm.ErrorMessage);
    }

    [TestMethod]
    public void ClearData_ResetsState()
    {
        using var vm = new TestBufferViewModel();
        vm.ClearData();
        Assert.IsFalse(vm.HasData);
        Assert.AreEqual(0, vm.DataSize);
        Assert.IsNull(vm.ErrorMessage);
    }
}
