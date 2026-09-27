using Catharsis.Randomization;

namespace Catharsis.UnitTests.Randomization;

///<summary>
///Unit tests for the <see cref="RandomStringGenerator"/> class.
///</summary>
[TestClass]
public class RandomStringGeneratorTests
{
    #region Construction

    [TestMethod]
    public void Constructor_NullOrEmptyCharset_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new RandomStringGenerator(null!));
        Assert.ThrowsExactly<ArgumentException>(() => new RandomStringGenerator(""));
    }

    [TestMethod]
    public void Constructor_Default_UsesAlphanumeric()
    {
        RandomStringGenerator generator = new();
        string result = generator.Generate(100);
        Assert.IsTrue(result.All(static c => RandomStringGenerator.Alphanumeric.Contains(c)));
    }

    #endregion

    #region Generate

    [TestMethod]
    public void Generate_NegativeLength_Throws()
    {
        RandomStringGenerator generator = new();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => generator.Generate(-1));
    }

    [TestMethod]
    public void Generate_ZeroLength_ReturnsEmptyString()
    {
        RandomStringGenerator generator = new();
        Assert.AreEqual(string.Empty, generator.Generate(0));
    }

    [TestMethod]
    public void Generate_ReturnsStringOfRequestedLength()
    {
        RandomStringGenerator generator = new();
        string result = generator.Generate(20);
        Assert.HasCount(20, result);
    }

    [TestMethod]
    public void Generate_OnlyUsesCharactersFromConfiguredCharset()
    {
        RandomStringGenerator generator = new(RandomStringGenerator.Digits);
        string result = generator.Generate(50);
        Assert.IsTrue(result.All(static c => RandomStringGenerator.Digits.Contains(c)));
    }

    [TestMethod]
    public void Generate_HexadecimalCharset_ProducesLowercaseHex()
    {
        RandomStringGenerator generator = new(RandomStringGenerator.Hexadecimal);
        string result = generator.Generate(32);
        Assert.IsTrue(result.All(static c => RandomStringGenerator.Hexadecimal.Contains(c)));
    }

    [TestMethod]
    public void Generate_DeterministicRandom_IsReproducible()
    {
        RandomStringGenerator generator1 = new(random: new Random(42));
        RandomStringGenerator generator2 = new(random: new Random(42));

        Assert.AreEqual(generator1.Generate(10), generator2.Generate(10));
    }

    #endregion
}
