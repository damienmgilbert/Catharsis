using Catharsis.Concurrency;
using System.Threading.Tasks.Dataflow;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="BoundedTransformBlockFactory"/> class.
///</summary>
[TestClass]
public class BoundedTransformBlockFactoryTests
{
    #region CreateForCpuBoundWork

    [TestMethod]
    public void CreateForCpuBoundWork_NullTransform_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => BoundedTransformBlockFactory.CreateForCpuBoundWork<int, int>(null!)); }

    [TestMethod]
    public async Task CreateForCpuBoundWork_ProcessesEachItem()
    {
        TransformBlock<int, int> block = BoundedTransformBlockFactory.CreateForCpuBoundWork<int, int>(x => x * x);

        foreach (int value in Enumerable.Range(1, 4))
        {
            await block.SendAsync(value);
        }

        block.Complete();

        List<int> results = [];

        for (int i = 0; i < 4; i++)
        {
            results.Add(await block.ReceiveAsync());
        }

        await block.Completion;

        CollectionAssert.AreEquivalent(new[] { 1, 4, 9, 16 }, results);
    }

    #endregion

    #region CreateForIoBoundWork

    [TestMethod]
    public void CreateForIoBoundWork_NullTransform_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => BoundedTransformBlockFactory.CreateForIoBoundWork<int, int>(null!)); }

    [TestMethod]
    public void CreateForIoBoundWork_ZeroMaxDegreeOfParallelism_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => BoundedTransformBlockFactory.CreateForIoBoundWork<int, int>(static x => Task.FromResult(x), 0));
    }

    [TestMethod]
    public async Task CreateForIoBoundWork_ProcessesEachItem()
    {
        TransformBlock<int, int> block = BoundedTransformBlockFactory.CreateForIoBoundWork<int, int>(async x =>
        {
            await Task.Yield();
            return x + 1;
        });

        foreach (int value in Enumerable.Range(1, 3))
        {
            await block.SendAsync(value);
        }

        block.Complete();

        List<int> results = [];

        for (int i = 0; i < 3; i++)
        {
            results.Add(await block.ReceiveAsync());
        }

        await block.Completion;

        CollectionAssert.AreEquivalent(new[] { 2, 3, 4 }, results);
    }

    #endregion
}
