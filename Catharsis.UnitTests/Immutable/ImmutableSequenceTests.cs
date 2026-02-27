using System.Buffers;
using Catharsis.Immutable;

namespace Catharsis.UnitTests.Immutable;

[TestClass]
public class ImmutableSequenceTests
{
    #region Public methods
    [TestMethod]
    public void Add_ReturnsNewSequenceWithItem()
    {
        ImmutableSequence<int> seq = ImmutableSequence<int>.Create([ 1, 2 ]);
        ImmutableSequence<int> seq2 = seq.Add(3);
        Assert.AreEqual(3, seq2.Count);
        Assert.AreEqual(2, seq.Count); // original unchanged
    }

    [TestMethod]
    public void Create_FromSpan_StoresData()
    {
        ImmutableSequence<int> seq = ImmutableSequence<int>.Create([ 1, 2, 3 ]);
        Assert.AreEqual(3, seq.Count);
        Assert.AreEqual(2, seq[1]);
    }

    [TestMethod]
    public void CreateFrom_ReadOnlySequence()
    {
        ReadOnlySequence<byte> data = new ReadOnlySequence<byte>(new byte[] { 10, 20, 30 });
        ImmutableSequence<byte> seq = ImmutableSequence<byte>.CreateFrom(in data);
        Assert.AreEqual(3, seq.Count);
        Assert.AreEqual(10, seq[0]);
    }

    [TestMethod]
    public void Empty_ReturnsEmptySequence()
    {
        ImmutableSequence<int> seq = ImmutableSequence<int>.Empty;
        Assert.AreEqual(0, seq.Count);
        Assert.IsTrue(seq.IsEmpty);
    }

    [TestMethod]
    public void Equals_DifferentData_ReturnsFalse()
    {
        ImmutableSequence<int> a = ImmutableSequence<int>.Create([ 1, 2 ]);
        ImmutableSequence<int> b = ImmutableSequence<int>.Create([ 1, 3 ]);
        Assert.IsFalse(a.Equals(b));
    }

    [TestMethod]
    public void Equals_SameData_ReturnsTrue()
    {
        ImmutableSequence<int> a = ImmutableSequence<int>.Create([ 1, 2 ]);
        ImmutableSequence<int> b = ImmutableSequence<int>.Create([ 1, 2 ]);
        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void Slice_ReturnsSubset()
    {
        ImmutableSequence<int> seq = ImmutableSequence<int>.Create([ 10, 20, 30, 40 ]);
        ImmutableSequence<int> sliced = seq.Slice(1, 2);
        Assert.AreEqual(2, sliced.Count);
        Assert.AreEqual(20, sliced[0]);
    }
    #endregion
}
