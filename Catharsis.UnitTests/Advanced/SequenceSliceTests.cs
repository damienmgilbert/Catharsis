using Catharsis.Advanced;
using System.Buffers;

namespace Catharsis.UnitTests.Advanced;

[TestClass]
public class SequenceSliceTests
{
    [TestMethod]
    public void Constructor_SetsLength()
    {
        ReadOnlySequence<byte> data = new ReadOnlySequence<byte>(new byte[] { 1, 2, 3, 4, 5 });
        SequenceSlice<byte> slice = new SequenceSlice<byte>(in data);
        Assert.AreEqual(5, slice.Length);
        Assert.IsFalse(slice.IsEmpty);
    }

    [TestMethod]
    public void Empty_HasZeroLength()
    {
        ReadOnlySequence<byte> data = ReadOnlySequence<byte>.Empty;
        SequenceSlice<byte> slice = new SequenceSlice<byte>(in data);
        Assert.AreEqual(0, slice.Length);
        Assert.IsTrue(slice.IsEmpty);
    }

    [TestMethod]
    public void Slice_ReturnsSubRange()
    {
        ReadOnlySequence<byte> data = new ReadOnlySequence<byte>(new byte[] { 10, 20, 30, 40, 50 });
        SequenceSlice<byte> slice = new SequenceSlice<byte>(in data);
        SequenceSlice<byte> sub = slice.Slice(1, 3);
        Assert.AreEqual(3, sub.Length);
    }

    [TestMethod]
    public void ToArray_ReturnsAllData()
    {
        byte[] source = [1, 2, 3];
        ReadOnlySequence<byte> data = new ReadOnlySequence<byte>(source);
        SequenceSlice<byte> slice = new SequenceSlice<byte>(in data);
        CollectionAssert.AreEqual(source, slice.ToArray());
    }

    [TestMethod]
    public void FirstSpan_ReturnsData()
    {
        ReadOnlySequence<byte> data = new ReadOnlySequence<byte>(new byte[] { 42 });
        SequenceSlice<byte> slice = new SequenceSlice<byte>(in data);
        Assert.AreEqual(42, slice.FirstSpan[0]);
    }
}
