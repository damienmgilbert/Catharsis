using Catharsis.Events;

namespace Catharsis.UnitTests.Events;

///<summary>
///Unit tests for the <see cref="EventDebouncer{T}"/> class.
///</summary>
[TestClass]
public class EventDebouncerTests
{
    static readonly TimeSpan Quiet = TimeSpan.FromMilliseconds(40);

    [TestMethod]
    public void Constructor_InvalidArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new EventDebouncer<int>(TimeSpan.FromSeconds(-1), static _ => { }));
        Assert.ThrowsExactly<ArgumentNullException>(static () => new EventDebouncer<int>(Quiet, (Action<int>)null!));
        Assert.ThrowsExactly<ArgumentNullException>(static () => new EventDebouncer<int>(Quiet, (Func<int, CancellationToken, Task>)null!));
    }

    [TestMethod]
    public async Task Post_Burst_RunsOnceWithLatestValue()
    {
        TaskCompletionSource<int> done = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;
        using EventDebouncer<int> debouncer = new(Quiet, value => { calls++; done.TrySetResult(value); });

        debouncer.Post(1);
        debouncer.Post(2);
        debouncer.Post(3);

        Assert.AreEqual(3, await done.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        await Task.Delay(Quiet * 3);
        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public async Task Post_AfterQuietPeriod_RunsAgain()
    {
        List<int> seen = [];
        TaskCompletionSource first = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource second = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using EventDebouncer<int> debouncer = new(Quiet, value =>
        {
            lock(seen)
            {
                seen.Add(value);
                (seen.Count == 1 ? first : second).TrySetResult();
            }
        });

        debouncer.Post(1);
        await first.Task.WaitAsync(TimeSpan.FromSeconds(5));
        debouncer.Post(2);
        await second.Task.WaitAsync(TimeSpan.FromSeconds(5));

        CollectionAssert.AreEqual(new[] { 1, 2 }, seen);
    }

    [TestMethod]
    public async Task AsyncAction_ReceivesValueAndToken()
    {
        TaskCompletionSource<(string, bool)> done = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using EventDebouncer<string> debouncer = new(Quiet, (value, ct) =>
        {
            done.TrySetResult((value, ct.CanBeCanceled));
            return Task.CompletedTask;
        });

        debouncer.Post("x");

        (string value, bool cancellable) = await done.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.AreEqual("x", value);
        Assert.IsTrue(cancellable);
    }

    [TestMethod]
    public async Task Dispose_CancelsPendingRun_AndRejectsFurtherPosts()
    {
        int calls = 0;
        EventDebouncer<int> debouncer = new(Quiet, _ => calls++);

        debouncer.Post(1);
        debouncer.Dispose();
        await Task.Delay(Quiet * 3);

        Assert.AreEqual(0, calls);
        Assert.ThrowsExactly<ObjectDisposedException>(() => debouncer.Post(2));
    }
}
