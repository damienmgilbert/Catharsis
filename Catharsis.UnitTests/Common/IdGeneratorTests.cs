using Catharsis.Common;
using System.Collections.Concurrent;

namespace Catharsis.UnitTests.Common;

///<summary>
///Unit tests for the <see cref="IdGenerator"/> class.
///</summary>
[TestClass]
public class IdGeneratorTests
{
    #region Public methods

    [TestMethod]
    public void NextId_SuccessiveCalls_AreStrictlyIncreasing()
    {
        IdGenerator generator = new();

        long first = generator.NextId();
        long second = generator.NextId();

        Assert.IsTrue(second > first);
    }

    [TestMethod]
    public void NextId_ManyCallsFromSameInstance_AreAllUnique()
    {
        IdGenerator generator = new();
        HashSet<long> ids = [];

        for(int i = 0; i < 5000; i++)
        {
            Assert.IsTrue(ids.Add(generator.NextId()));
        }
    }

    [TestMethod]
    public void NextId_ConcurrentCallers_ProduceUniqueIds()
    {
        IdGenerator generator = new();
        ConcurrentBag<long> ids = [];

        Parallel.For(0, 2000, _ => ids.Add(generator.NextId()));

        Assert.AreEqual(2000, ids.Distinct().Count());
    }

    [TestMethod]
    public void NextId_CustomEpoch_ProducesSmallerValueThanDefaultEpoch()
    {
        IdGenerator defaultEpochGenerator = new();
        IdGenerator customEpochGenerator = new(DateTimeOffset.UtcNow);

        long defaultId = defaultEpochGenerator.NextId();
        long customId = customEpochGenerator.NextId();

        Assert.IsTrue(customId < defaultId);
    }

    #endregion
}
