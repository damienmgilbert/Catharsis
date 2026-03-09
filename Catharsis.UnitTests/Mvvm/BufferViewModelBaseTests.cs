using Catharsis.Mvvm;

namespace Catharsis.UnitTests.Mvvm;

///<summary>
///Unit tests for the <see cref="BufferViewModelBase"/> class.
///</summary>
[TestClass]
public class BufferViewModelBaseTests
{
    #region Public methods
    [TestMethod]
    public void ClearData_ResetsState()
    {
        using TestBufferViewModel vm = new TestBufferViewModel();
        vm.ClearData();
        Assert.IsFalse(vm.HasData);
        Assert.AreEqual(0, vm.DataSize);
        Assert.IsNull(vm.ErrorMessage);
    }

    [TestMethod]
    public async Task LoadAsync_SetsCancelledMessage()
    {
        using TestBufferViewModel vm = new TestBufferViewModel { ShouldCancel = true };
        await vm.LoadAsync();
        Assert.IsTrue(vm.HasError);
        Assert.AreEqual("Operation was cancelled.", vm.ErrorMessage);
    }

    [TestMethod]
    public async Task LoadAsync_SetsErrorMessageOnException()
    {
        using TestBufferViewModel vm = new TestBufferViewModel { ShouldThrow = true };
        await vm.LoadAsync();
        Assert.IsTrue(vm.HasError);
        Assert.AreEqual("Test error", vm.ErrorMessage);
        Assert.IsFalse(vm.HasData);
    }

    [TestMethod]
    public async Task LoadAsync_SetsHasDataOnSuccess()
    {
        using TestBufferViewModel vm = new TestBufferViewModel();
        await vm.LoadAsync();
        Assert.IsTrue(vm.LoadCoreCalled);
        Assert.IsTrue(vm.HasData);
        Assert.AreEqual(100, vm.DataSize);
        Assert.IsFalse(vm.IsLoading);
        Assert.IsFalse(vm.HasError);
    }
    #endregion

    sealed class TestBufferViewModel : BufferViewModelBase
    {
        #region Protected methods
        protected override Task LoadCoreAsync(CancellationToken cancellationToken)
        {
            LoadCoreCalled = true;

            if(ShouldCancel)
            {
                throw new OperationCanceledException();
            }

            if(ShouldThrow)
            {
                throw new InvalidOperationException("Test error");
            }

            DataSize = 100;
            return Task.CompletedTask;
        }
        #endregion

        #region Public properties
        public bool LoadCoreCalled { get; private set; }

        public bool ShouldCancel { get; set; }

        public bool ShouldThrow { get; set; }
        #endregion
    }
}
