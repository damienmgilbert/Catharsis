using Catharsis.Extensions;

namespace Catharsis.UnitTests.Extensions;

///<summary>
///Unit tests for the <see cref="TaskExtensions"/> class.
///</summary>
[TestClass]
public class TaskExtensionsTests
{
    #region FireAndForget

    [TestMethod]
    public void FireAndForget_NullTask_Throws()
    {
        Task task = null!;
        Assert.ThrowsExactly<ArgumentNullException>(() => task.FireAndForget());
    }

    [TestMethod]
    public async Task FireAndForget_SuccessfulTask_CompletesWithoutCallingHandler()
    {
        bool called = false;
        Task.CompletedTask.FireAndForget(_ => called = true);
        await Task.Delay(20);
        Assert.IsFalse(called);
    }

    [TestMethod]
    public async Task FireAndForget_FaultedTask_InvokesExceptionHandler()
    {
        Exception? observed = null;
        using ManualResetEventSlim signal = new(false);

        Task.FromException(new InvalidOperationException("boom")).FireAndForget(ex =>
        {
            observed = ex;
            signal.Set();
        });

        signal.Wait(TimeSpan.FromSeconds(2));
        Assert.IsInstanceOfType<InvalidOperationException>(observed);
    }

    #endregion

    #region WhenAllOrFirstException

    [TestMethod]
    public async Task WhenAllOrFirstException_NullTasks_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await ((IEnumerable<Task>)null!).WhenAllOrFirstException());
    }

    [TestMethod]
    public async Task WhenAllOrFirstException_AllSucceed_Completes()
    {
        List<Task> tasks = [Task.CompletedTask, Task.Delay(10), Task.CompletedTask];
        await tasks.WhenAllOrFirstException();
    }

    [TestMethod]
    public async Task WhenAllOrFirstException_OneFails_ThrowsThatException()
    {
        List<Task> tasks =
        [
            Task.Delay(200),
            Task.FromException(new InvalidOperationException("first failure")),
        ];

        InvalidOperationException ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await tasks.WhenAllOrFirstException());
        Assert.AreEqual("first failure", ex.Message);
    }

    #endregion
}
