using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="SequenceZipper"/> class.
///</summary>
[TestClass]
public class SequenceZipperTests
{
    #region ZipPadded (tuple overload)

    [TestMethod]
    public void ZipPadded_NullFirst_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).ZipPadded([1]).ToList());
    }

    [TestMethod]
    public void ZipPadded_NullSecond_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new[] { 1 }.ZipPadded((IEnumerable<int>)null!).ToList());
    }

    [TestMethod]
    public void ZipPadded_EqualLength_PairsAllElements()
    {
        List<(int? First, int? Second)> result = [.. new[] { 1, 2, 3 }.ZipPadded(new[] { 10, 20, 30 })];
        CollectionAssert.AreEqual(new (int?, int?)[] { (1, 10), (2, 20), (3, 30) }, result);
    }

    [TestMethod]
    public void ZipPadded_FirstLonger_PadsSecondWithDefault()
    {
        List<(int? First, int? Second)> result = [.. new[] { 1, 2, 3 }.ZipPadded(new[] { 10 })];

        CollectionAssert.AreEqual(new (int?, int?)[] { (1, 10), (2, 0), (3, 0) }, result);
    }

    [TestMethod]
    public void ZipPadded_SecondLonger_PadsFirstWithDefault()
    {
        List<(int? First, int? Second)> result = [.. new[] { 1 }.ZipPadded(new[] { 10, 20, 30 })];

        CollectionAssert.AreEqual(new (int?, int?)[] { (1, 10), (0, 20), (0, 30) }, result);
    }

    [TestMethod]
    public void ZipPadded_BothEmpty_ReturnsEmpty()
    {
        Assert.IsEmpty(Array.Empty<int>().ZipPadded(Array.Empty<int>()).ToList());
    }

    #endregion

    #region ZipPadded (selector overload)

    [TestMethod]
    public void ZipPadded_WithSelector_NullResultSelector_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new[] { 1 }.ZipPadded<int, int, string>(new[] { 1 }, 0, 0, null!).ToList());
    }

    [TestMethod]
    public void ZipPadded_WithSelector_UsesFallbackValuesPastShorterSequence()
    {
        List<string> result = [.. new[] { 1, 2, 3 }.ZipPadded(new[] { "a" }, -1, "none", static (n, s) => $"{n}:{s}")];
        CollectionAssert.AreEqual(new[] { "1:a", "2:none", "3:none" }, result);
    }

    #endregion
}
