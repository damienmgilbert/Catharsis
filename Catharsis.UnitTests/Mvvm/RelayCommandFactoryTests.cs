using Catharsis.Mvvm;
using CommunityToolkit.Mvvm.Input;

namespace Catharsis.UnitTests.Mvvm;

///<summary>
///Unit tests for the <see cref="RelayCommandFactory"/> class.
///</summary>
[TestClass]
public class RelayCommandFactoryTests
{
    #region Create (no parameter)

    [TestMethod]
    public void Create_NullExecute_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => RelayCommandFactory.Create(null!)); }

    [TestMethod]
    public async Task Create_Executes_ReportsBusyThenNotBusy()
    {
        List<bool> busyStates = [];
        IAsyncRelayCommand command = RelayCommandFactory.Create(static () => Task.CompletedTask, busyStates.Add);

        await command.ExecuteAsync(null);

        CollectionAssert.AreEqual(new[] { true, false }, busyStates);
    }

    [TestMethod]
    public async Task Create_ExecuteThrows_RoutesToOnException()
    {
        Exception? caught = null;
        IAsyncRelayCommand command = RelayCommandFactory.Create(static () => throw new InvalidOperationException("boom"), onException: ex => caught = ex);

        await command.ExecuteAsync(null);

        Assert.IsNotNull(caught);
        Assert.AreEqual("boom", caught!.Message);
    }

    [TestMethod]
    public async Task Create_ExecuteThrows_StillReportsNotBusy()
    {
        List<bool> busyStates = [];
        IAsyncRelayCommand command = RelayCommandFactory.Create(static () => throw new InvalidOperationException(), busyStates.Add, static _ => { });

        await command.ExecuteAsync(null);

        CollectionAssert.AreEqual(new[] { true, false }, busyStates);
    }

    #endregion

    #region Create<T> (with parameter)

    [TestMethod]
    public void CreateOfT_NullExecute_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => RelayCommandFactory.Create<string>(null!)); }

    [TestMethod]
    public async Task CreateOfT_Executes_PassesParameter()
    {
        string? received = null;
        IAsyncRelayCommand<string> command = RelayCommandFactory.Create<string>(value => { received = value; return Task.CompletedTask; });

        await command.ExecuteAsync("hello");

        Assert.AreEqual("hello", received);
    }

    [TestMethod]
    public async Task CreateOfT_ExecuteThrows_RoutesToOnException()
    {
        Exception? caught = null;
        IAsyncRelayCommand<string> command = RelayCommandFactory.Create<string>(static _ => throw new InvalidOperationException("boom"), onException: ex => caught = ex);

        await command.ExecuteAsync("hello");

        Assert.IsNotNull(caught);
    }

    #endregion
}
