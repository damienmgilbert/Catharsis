using Catharsis.Immutable;
using Catharsis.Patterns;

namespace Catharsis.UnitTests.Patterns;

[TestClass]
public class ImmutablePooledHybridTests
{
    #region Public methods
    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        ImmutablePooledHybrid<byte> hybrid = new ImmutablePooledHybrid<byte>();
        hybrid.Dispose();
        hybrid.Dispose();
    }

    [TestMethod]
    public void Freeze_CreatesImmutableSnapshot()
    {
        using ImmutablePooledHybrid<int> hybrid = new ImmutablePooledHybrid<int>();
        hybrid.Write([ 10, 20 ]);
        ImmutableBuffer<int> frozen = hybrid.Freeze();
        Assert.IsTrue(hybrid.IsFrozen);
        Assert.AreEqual(2, frozen.Count);
        Assert.AreEqual(10, frozen[0]);
        Assert.AreEqual(20, frozen[1]);
    }

    [TestMethod]
    public void Reset_AllowsRewrite()
    {
        using ImmutablePooledHybrid<byte> hybrid = new ImmutablePooledHybrid<byte>();
        hybrid.Write([ 1 ]);
        hybrid.Freeze();

        hybrid.Reset();

        Assert.IsFalse(hybrid.IsFrozen);
        Assert.AreEqual(0, hybrid.Count);
        hybrid.Write([ 2, 3 ]);
        Assert.AreEqual(2, hybrid.Count);
    }

    [TestMethod]
    public void Span_ReturnsMutableDataBeforeFreeze()
    {
        using ImmutablePooledHybrid<byte> hybrid = new ImmutablePooledHybrid<byte>();
        hybrid.Write([ 5, 10 ]);
        ReadOnlySpan<byte> span = hybrid.Span;
        Assert.AreEqual(2, span.Length);
        Assert.AreEqual(5, span[0]);
    }

    [TestMethod]
    public void Write_AfterFreeze_Throws()
    {
        using ImmutablePooledHybrid<byte> hybrid = new ImmutablePooledHybrid<byte>();
        hybrid.Write([ 1 ]);
        hybrid.Freeze();
        Assert.ThrowsExactly<ArgumentException>(() => hybrid.Write([ 2 ]));
    }

    [TestMethod]
    public void Write_IncreasesCount()
    {
        using ImmutablePooledHybrid<byte> hybrid = new ImmutablePooledHybrid<byte>();
        hybrid.Write([ 1, 2, 3 ]);
        Assert.AreEqual(3, hybrid.Count);
        Assert.IsFalse(hybrid.IsFrozen);
    }
    #endregion
}
