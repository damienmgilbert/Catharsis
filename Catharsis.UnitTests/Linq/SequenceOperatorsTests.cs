using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

///<summary>
///Unit tests for the <see cref="SequenceOperators"/> class.
///</summary>
[TestClass]
public class SequenceOperatorsTests
{
    [TestMethod]
    public void Choose_Ref_YieldsNonNullResults()
    {
        List<string> result = new[] { 1, 2, 3, 4 }
            .Choose<int, string>(static x => x % 2 == 0 ? $"even:{x}" : null)
            .ToList();
        CollectionAssert.AreEqual(new[] { "even:2", "even:4" }, result);
    }

    [TestMethod]
    public void Choose_Value_YieldsHasValueResults()
    {
        List<int> result = new[] { 1, 2, 3, 4 }
            .Choose<int, int>(static x => x > 2 ? x * 10 : (int?)null)
            .ToList();
        CollectionAssert.AreEqual(new[] { 30, 40 }, result);
    }

    [TestMethod]
    public void Choose_Indexed_YieldsNonNullWithIndex()
    {
        List<string> result = new[] { "a", "b", "c" }
            .Choose<string, string>(static (x, i) => i % 2 == 0 ? x.ToUpperInvariant() : null)
            .ToList();
        CollectionAssert.AreEqual(new[] { "A", "C" }, result);
    }

    [TestMethod]
    public void Choose_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).Choose<int, string>(static x => x.ToString()).ToList());
    }

    [TestMethod]
    public void Let_Scalar_MaterializesAndAppliesSelector()
    {
        int result = new[] { 1, 2, 3 }.Let(static list => list.Count + list[0]);
        Assert.AreEqual(4, result);
    }

    [TestMethod]
    public void Let_Sequence_MaterializesAndReturnsSequence()
    {
        List<int> result = new[] { 3, 1, 2 }
            .Let<int, int>(static list => list.OrderBy(static x => x))
            .ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    [TestMethod]
    public void Let_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).Let(static list => list.Count));
    }

    [TestMethod]
    public void LetWhere_FiltersUsingMaterializedData()
    {
        // Keep only elements above average
        List<int> result = new[] { 1, 5, 3, 7 }
            .LetWhere(list =>
            {
                double avg = list.Average(x => (double)x);
                return list.Where(x => x > avg);
            })
            .ToList();
        CollectionAssert.AreEqual(new[] { 5, 7 }, result);
    }

    [TestMethod]
    public void Pipe_Scalar_AppliesTransformation()
    {
        int result = new[] { 1, 2, 3 }.Pipe(static s => s.Sum());
        Assert.AreEqual(6, result);
    }

    [TestMethod]
    public void Pipe_Sequence_TransformsSequence()
    {
        List<int> result = new[] { 3, 1, 2 }
            .Pipe<int, int>(static s => s.OrderBy(static x => x))
            .ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    [TestMethod]
    public void Pipe_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).Pipe(static s => s.Count()));
    }

    [TestMethod]
    public void PipeTap_ExecutesSideEffectAndReturnsSource()
    {
        int[] source = [1, 2, 3];
        int count = 0;
        IEnumerable<int> result = source.PipeTap(s => count = s.Count());
        Assert.AreSame(source, result);
        Assert.AreEqual(3, count);
    }

    [TestMethod]
    public void PipeTap_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).PipeTap(static _ => { }));
    }

    [TestMethod]
    public void PipeTapEach_ExecutesActionPerElement()
    {
        List<int> tapped = [];
        List<int> result = new[] { 1, 2, 3 }.PipeTapEach(x => tapped.Add(x)).ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, tapped);
    }

    [TestMethod]
    public void PipeTapEach_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ((IEnumerable<int>)null!).PipeTapEach(static _ => { }).ToList());
    }
}
