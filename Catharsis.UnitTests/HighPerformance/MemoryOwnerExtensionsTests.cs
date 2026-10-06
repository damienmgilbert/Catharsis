using System.Buffers;
using Catharsis.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace Catharsis.UnitTests.HighPerformance;

///<summary>
///Unit tests for the <see cref="MemoryOwnerExtensions"/> class.
///</summary>
[TestClass]
public class MemoryOwnerExtensionsTests
{
    #region Public methods
    [TestMethod]
    public void Clear_ClearsToDefault()
    {
        using MemoryOwner<int> owner = MemoryOwner<int>.Allocate(3);
        owner.Fill(99);
        owner.Clear();
        foreach (int v in owner.Span)
        {
            Assert.AreEqual(0, v);
        }
    }

    [TestMethod]
    public void Fill_FillsEntireOwner()
    {
        using MemoryOwner<int> owner = MemoryOwner<int>.Allocate(5);
        owner.Fill(42);
        foreach (int v in owner.Span)
        {
            Assert.AreEqual(42, v);
        }
    }

    [TestMethod]
    public void SliceCopy_ReturnsSlicedCopy()
    {
        using MemoryOwner<int> owner = MemoryOwner<int>.Allocate(5);
        for (int i = 0; i < 5; i++)
        {
            owner.Span[i] = i * 10;
        }

        using MemoryOwner<int> sliced = owner.SliceCopy(1, 3);

        Assert.AreEqual(3, sliced.Length);
        Assert.AreEqual(10, sliced.Span[0]);
        Assert.AreEqual(30, sliced.Span[2]);
    }

    [TestMethod]
    public void ToMemoryOwner_FromMemory_CopiesData()
    {
        ReadOnlyMemory<byte> data = new byte[] { 10, 20 };
        using MemoryOwner<byte> owner = data.ToMemoryOwner();
        Assert.AreEqual(2, owner.Length);
        Assert.AreEqual(10, owner.Span[0]);
    }

    [TestMethod]
    public void ToMemoryOwner_FromSpan_CopiesData()
    {
        ReadOnlySpan<int> data = [1, 2, 3];
        using MemoryOwner<int> owner = data.ToMemoryOwner();
        Assert.AreEqual(3, owner.Length);
        Assert.AreEqual(1, owner.Span[0]);
        Assert.AreEqual(3, owner.Span[2]);
    }

    [TestMethod]
    public void WriteTo_WritesToBufferWriter()
    {
        using MemoryOwner<byte> owner = MemoryOwner<byte>.Allocate(3);
        owner.Span[0] = 1;
        owner.Span[1] = 2;
        owner.Span[2] = 3;
        ArrayBufferWriter<byte> writer = new();

        owner.WriteTo(writer);

        Assert.AreEqual(3, writer.WrittenCount);
        Assert.AreEqual(1, writer.WrittenSpan[0]);
    }
    #endregion
}
