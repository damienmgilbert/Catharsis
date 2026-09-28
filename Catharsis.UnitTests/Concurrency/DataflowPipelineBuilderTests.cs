using Catharsis.Concurrency;
using System.Collections.Concurrent;

namespace Catharsis.UnitTests.Concurrency;

///<summary>
///Unit tests for the <see cref="DataflowPipelineBuilder"/> and <see cref="DataflowPipelineBuilder{TInput,TCurrent}"/> classes.
///</summary>
[TestClass]
public class DataflowPipelineBuilderTests
{
    #region Transform

    [TestMethod]
    public void Transform_NullTransform_Throws()
    {
        DataflowPipelineBuilder<int, int> builder = DataflowPipelineBuilder.Create<int>();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.Transform<int>(null!));
    }

    #endregion

    #region ActionBlock

    [TestMethod]
    public void ActionBlock_NullAction_Throws()
    {
        DataflowPipelineBuilder<int, int> builder = DataflowPipelineBuilder.Create<int>();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.ActionBlock(null!));
    }

    #endregion

    #region End-to-end pipeline

    [TestMethod]
    public async Task Pipeline_SingleTransformStage_ProcessesEachItem()
    {
        ConcurrentBag<int> results = [];

        DataflowPipeline<int> pipeline = DataflowPipelineBuilder.Create<int>()
            .Transform(x => x * 2)
            .ActionBlock(results.Add);

        foreach(int value in Enumerable.Range(1, 5))
        {
            await pipeline.SendAsync(value);
        }

        pipeline.Complete();
        await pipeline.Completion;

        CollectionAssert.AreEquivalent(new[] { 2, 4, 6, 8, 10 }, results.ToList());
    }

    [TestMethod]
    public async Task Pipeline_MultipleTransformStages_ChainsOutputTypesCorrectly()
    {
        ConcurrentBag<string> results = [];

        DataflowPipeline<int> pipeline = DataflowPipelineBuilder.Create<int>()
            .Transform(x => x + 1)
            .Transform(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .ActionBlock(results.Add);

        await pipeline.SendAsync(1);
        await pipeline.SendAsync(2);

        pipeline.Complete();
        await pipeline.Completion;

        CollectionAssert.AreEquivalent(new[] { "2", "3" }, results.ToList());
    }

    [TestMethod]
    public async Task Pipeline_FaultedStage_PropagatesToCompletion()
    {
        DataflowPipeline<int> pipeline = DataflowPipelineBuilder.Create<int>()
            .Transform<int>(static _ => throw new InvalidOperationException("boom"))
            .ActionBlock(static _ => { });

        await pipeline.SendAsync(1);

        await Assert.ThrowsExactlyAsync<AggregateException>(() => pipeline.Completion);
    }

    [TestMethod]
    public void Post_AcceptsItemSynchronously()
    {
        ConcurrentBag<int> results = [];

        DataflowPipeline<int> pipeline = DataflowPipelineBuilder.Create<int>()
            .ActionBlock(results.Add);

        bool accepted = pipeline.Post(1);

        Assert.IsTrue(accepted);
    }

    #endregion
}
