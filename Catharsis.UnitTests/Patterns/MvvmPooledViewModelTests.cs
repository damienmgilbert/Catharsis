using Catharsis.Patterns;

namespace Catharsis.UnitTests.Patterns;

[TestClass]
public class MvvmPooledViewModelTests
{
    #region Public methods
    [TestMethod]
    public void ClearAllDataCommand_ClearsState()
    {
        using MvvmPooledViewModel vm = new MvvmPooledViewModel();
        vm.ClearAllDataCommand.Execute(null);
        Assert.IsFalse(vm.HasData);
        Assert.AreEqual(string.Empty, vm.DisplayText);
    }

    [TestMethod]
    public void Commands_AreNotNull()
    {
        using MvvmPooledViewModel vm = new MvvmPooledViewModel();
        Assert.IsNotNull(vm.LoadSampleDataCommand);
        Assert.IsNotNull(vm.ClearAllDataCommand);
    }

    [TestMethod]
    public void DataBuffer_IsNotNull()
    {
        using MvvmPooledViewModel vm = new MvvmPooledViewModel();
        Assert.IsNotNull(vm.DataBuffer);
    }

    [TestMethod]
    public async Task LoadAsync_LoadsDataIntoBuffer()
    {
        using MvvmPooledViewModel vm = new MvvmPooledViewModel();
        await vm.LoadAsync();
        Assert.IsTrue(vm.HasData);
        Assert.AreEqual(1024, vm.DataSize);
        Assert.Contains("1024", vm.DisplayText);
    }
    #endregion
}
