using Catharsis.Patterns;

namespace Catharsis.UnitTests.Patterns;

///<summary>
///Unit tests for the <see cref="BinaryProcessingPipeline"/> class.
///</summary>
[TestClass]
public class BinaryProcessingPipelineTests
{
    #region Public methods
    [TestMethod]
    public void AddStage_IncreasesStageCount()
    {
        using BinaryProcessingPipeline pipeline = new();
        pipeline.AddStage(BinaryProcessingPipeline.CreateXorStage(0xFF));
        Assert.AreEqual(1, pipeline.StageCount);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        BinaryProcessingPipeline pipeline = new();
        pipeline.Dispose();
        pipeline.Dispose();
    }

    [TestMethod]
    public void Execute_DoubleXor_ReturnsOriginal()
    {
        using BinaryProcessingPipeline pipeline = new();
        pipeline.AddStage(BinaryProcessingPipeline.CreateXorStage(0xAB));
        pipeline.AddStage(BinaryProcessingPipeline.CreateXorStage(0xAB));

        byte[] result = pipeline.Execute([10, 20, 30], out _);

        CollectionAssert.AreEqual(new byte[] { 10, 20, 30 }, result);
    }

    [TestMethod]
    public void Execute_NoStages_ReturnsOriginal()
    {
        using BinaryProcessingPipeline pipeline = new();
        byte[] result = pipeline.Execute([1, 2], out _);
        CollectionAssert.AreEqual(new byte[] { 1, 2 }, result);
    }

    [TestMethod]
    public void Execute_ReverseStage_ReversesData()
    {
        using BinaryProcessingPipeline pipeline = new();
        pipeline.AddStage(BinaryProcessingPipeline.CreateReverseStage());

        byte[] result = pipeline.Execute([1, 2, 3], out _);

        CollectionAssert.AreEqual(new byte[] { 3, 2, 1 }, result);
    }

    [TestMethod]
    public void Execute_XorStage_TransformsData()
    {
        using BinaryProcessingPipeline pipeline = new();
        pipeline.AddStage(BinaryProcessingPipeline.CreateXorStage(0xFF));

        byte[] result = pipeline.Execute([0x00, 0x01, 0x02], out double elapsed);

        Assert.AreEqual(0xFF, result[0]);
        Assert.AreEqual(0xFE, result[1]);
        Assert.AreEqual(0xFD, result[2]);
        Assert.IsGreaterThanOrEqualTo(0, elapsed);
    }
    #endregion
}
