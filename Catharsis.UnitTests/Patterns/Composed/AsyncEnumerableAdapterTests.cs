using Catharsis.Patterns.Composed;
using System.Runtime.CompilerServices;

namespace Catharsis.UnitTests.Patterns.Composed;

///<summary>
///Unit tests for <see cref="AsyncEnumerableAdapter"/>.
///</summary>
[TestClass]
public class AsyncEnumerableAdapterTests
{
    static async IAsyncEnumerable<int> Numbers(int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for(int i = 1; i <= count; i++)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();

            yield return i;
        }
    }

    [TestMethod]
    public async Task ToAsyncEnumerable_YieldsEveryElementInOrder()
    {
        List<int> seen = [];

        await foreach(int value in AsyncEnumerableAdapter.ToAsyncEnumerable([1, 2, 3]))
        {
            seen.Add(value);
        }

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, seen);
    }

    [TestMethod]
    public async Task ToAsyncEnumerable_Cancelled_StopsEnumeration()
    {
        using CancellationTokenSource cts = new();
        List<int> seen = [];

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
        {
            await foreach(int value in AsyncEnumerableAdapter.ToAsyncEnumerable([1, 2, 3], cts.Token))
            {
                seen.Add(value);
                cts.Cancel();
            }
        });

        CollectionAssert.AreEqual(new[] { 1 }, seen);
    }

    [TestMethod]
    public void ToAsyncEnumerable_Null_ThrowsEagerly() { Assert.ThrowsExactly<ArgumentNullException>(static () => AsyncEnumerableAdapter.ToAsyncEnumerable<int>(null!)); }

    [TestMethod]
    public void ToEnumerable_YieldsEveryElementInOrder() { CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, AsyncEnumerableAdapter.ToEnumerable(Numbers(4)).ToArray()); }

    [TestMethod]
    public void ToEnumerable_RoundTripsThroughToAsyncEnumerable() { CollectionAssert.AreEqual(new[] { 5, 6 }, AsyncEnumerableAdapter.ToEnumerable(AsyncEnumerableAdapter.ToAsyncEnumerable([5, 6])).ToArray()); }

    [TestMethod]
    public void ToEnumerable_SourceThrows_PropagatesToConsumer() { Assert.ThrowsExactly<InvalidOperationException>(static () => AsyncEnumerableAdapter.ToEnumerable(Failing()).ToArray()); }

    static async IAsyncEnumerable<int> Failing()
    {
        yield return 1;
        await Task.Yield();
        throw new InvalidOperationException();
    }

    [TestMethod]
    public void ToEnumerable_AbandonedEarly_DisposesSource()
    {
        bool disposed = false;

        async IAsyncEnumerable<int> Tracked()
        {
            try
            {
                yield return 1;
                yield return 2;
            }
            finally
            {
                disposed = true;
                await Task.Yield();
            }
        }

        Assert.AreEqual(1, AsyncEnumerableAdapter.ToEnumerable(Tracked()).First());
        Assert.IsTrue(disposed);
    }

    [TestMethod]
    public void ToEnumerable_Null_ThrowsEagerly() { Assert.ThrowsExactly<ArgumentNullException>(static () => AsyncEnumerableAdapter.ToEnumerable<int>(null!)); }
}
