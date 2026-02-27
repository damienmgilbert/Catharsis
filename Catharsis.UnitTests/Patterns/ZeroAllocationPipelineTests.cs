using Catharsis.Patterns;

namespace Catharsis.UnitTests.Patterns;

[TestClass]
public class ZeroAllocationPipelineTests
{
    #region Public methods
    [TestMethod]
    public void ParseRecords_ParsesCorrectly()
    {
        const int recordSize = sizeof(int) + sizeof(double) + sizeof(long);
        byte[] data = new byte[recordSize];
        BitConverter.TryWriteBytes(data.AsSpan(0), 42);
        BitConverter.TryWriteBytes(data.AsSpan(4), 3.14);
        BitConverter.TryWriteBytes(data.AsSpan(12), 999L);

        ZeroAllocationPipeline.ParsedRecord[] dest = new ZeroAllocationPipeline.ParsedRecord[1];
        int count = ZeroAllocationPipeline.ParseRecords(data, dest);

        Assert.AreEqual(1, count);
        Assert.AreEqual(42, dest[0].Id);
        Assert.AreEqual(3.14, dest[0].Value, 0.001);
        Assert.AreEqual(999L, dest[0].Timestamp);
    }

    [TestMethod]
    public void SumCsvIntegers_EmptyString_ReturnsZero()
    {
        long sum = ZeroAllocationPipeline.SumCsvIntegers(string.Empty.AsSpan());
        Assert.AreEqual(0, sum);
    }

    [TestMethod]
    public void SumCsvIntegers_SingleValue()
    {
        long sum = ZeroAllocationPipeline.SumCsvIntegers("42".AsSpan());
        Assert.AreEqual(42, sum);
    }

    [TestMethod]
    public void SumCsvIntegers_SumsCorrectly()
    {
        long sum = ZeroAllocationPipeline.SumCsvIntegers("1,2,3,4,5".AsSpan());
        Assert.AreEqual(15, sum);
    }

    [TestMethod]
    public void SumCsvIntegers_WithSpaces_TrimsCorrectly()
    {
        long sum = ZeroAllocationPipeline.SumCsvIntegers(" 10 , 20 , 30 ".AsSpan());
        Assert.AreEqual(60, sum);
    }
    #endregion
}
