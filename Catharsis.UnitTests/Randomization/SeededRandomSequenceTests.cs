using Catharsis.Randomization;

namespace Catharsis.UnitTests.Randomization;

///<summary>
///Unit tests for the <see cref="SeededRandomSequence"/> class.
///</summary>
[TestClass]
public class SeededRandomSequenceTests
{
    #region Construction

    [TestMethod]
    public void Constructor_SetsSeed()
    {
        SeededRandomSequence sequence = new(12345);
        Assert.AreEqual(12345, sequence.Seed);
    }

    #endregion

    #region Reproducibility

    [TestMethod]
    public void Reset_ReproducesTheSameSequence()
    {
        SeededRandomSequence sequence = new(12345);
        int[] first = [sequence.Next(), sequence.Next(), sequence.Next()];

        sequence.Reset();
        int[] second = [sequence.Next(), sequence.Next(), sequence.Next()];

        CollectionAssert.AreEqual(first, second);
    }

    [TestMethod]
    public void SameSeed_TwoInstances_ProduceTheSameSequence()
    {
        SeededRandomSequence a = new(999);
        SeededRandomSequence b = new(999);

        for(int i = 0; i < 5; i++)
        {
            Assert.AreEqual(a.Next(), b.Next());
        }
    }

    [TestMethod]
    public void DifferentSeeds_ProduceDifferentSequences()
    {
        SeededRandomSequence a = new(1);
        SeededRandomSequence b = new(2);

        int[] fromA = [a.Next(), a.Next(), a.Next(), a.Next(), a.Next()];
        int[] fromB = [b.Next(), b.Next(), b.Next(), b.Next(), b.Next()];

        CollectionAssert.AreNotEqual(fromA, fromB);
    }

    #endregion

    #region Next overloads

    [TestMethod]
    public void Next_WithMaxValue_IsBelowMax()
    {
        SeededRandomSequence sequence = new(1);

        for(int i = 0; i < 20; i++)
        {
            int value = sequence.Next(10);
            Assert.IsTrue(value is >= 0 and < 10);
        }
    }

    [TestMethod]
    public void Next_WithMinAndMax_IsWithinRange()
    {
        SeededRandomSequence sequence = new(1);

        for(int i = 0; i < 20; i++)
        {
            int value = sequence.Next(5, 10);
            Assert.IsTrue(value is >= 5 and < 10);
        }
    }

    [TestMethod]
    public void NextDouble_IsWithinUnitRange()
    {
        SeededRandomSequence sequence = new(1);
        double value = sequence.NextDouble();
        Assert.IsTrue(value is >= 0.0 and < 1.0);
    }

    [TestMethod]
    public void NextBytes_FillsBuffer()
    {
        SeededRandomSequence sequence = new(1);
        byte[] buffer = new byte[16];
        sequence.NextBytes(buffer);
        Assert.IsTrue(Array.Exists(buffer, static b => b != 0));
    }

    #endregion
}
