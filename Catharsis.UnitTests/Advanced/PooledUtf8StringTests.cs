using System.Text;
using Catharsis;
using Catharsis.Advanced;

namespace Catharsis.UnitTests.Advanced;

[TestClass]
public class PooledUtf8StringTests
{
    [TestMethod]
    public void Create_FromString_RoundTrips()
    {
        using PooledUtf8String utf8 = PooledUtf8String.Create("Hello");
        Assert.AreEqual("Hello", utf8.ToString());
        Assert.AreEqual(5, utf8.ByteLength);
    }

    [TestMethod]
    public void Create_FromCharSpan_RoundTrips()
    {
        using PooledUtf8String utf8 = PooledUtf8String.Create("World".AsSpan());
        Assert.AreEqual("World", utf8.ToString());
    }

    [TestMethod]
    public void FromUtf8_Bytes_RoundTrips()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("Test");
        using PooledUtf8String utf8 = PooledUtf8String.FromUtf8(bytes);
        Assert.AreEqual("Test", utf8.ToString());
        Assert.AreEqual(4, utf8.ByteLength);
    }

    [TestMethod]
    public void Span_ReturnsUtf8Bytes()
    {
        using PooledUtf8String utf8 = PooledUtf8String.Create("A");
        Assert.AreEqual(1, utf8.Span.Length);
        Assert.AreEqual((byte)'A', utf8.Span[0]);
    }

    [TestMethod]
    public void Memory_ReturnsReadOnlyMemory()
    {
        using PooledUtf8String utf8 = PooledUtf8String.Create("AB");
        Assert.AreEqual(2, utf8.Memory.Length);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        PooledUtf8String utf8 = PooledUtf8String.Create("X");
        utf8.Dispose();
        utf8.Dispose();
    }

    [TestMethod]
    public void Equals_SameContent_ReturnsTrue()
    {
        using PooledUtf8String a = PooledUtf8String.Create("abc");
        using PooledUtf8String b = PooledUtf8String.Create("abc");
        Assert.IsTrue(a.Equals(b));
    }

    [TestMethod]
    public void Equals_DifferentContent_ReturnsFalse()
    {
        using PooledUtf8String a = PooledUtf8String.Create("abc");
        using PooledUtf8String b = PooledUtf8String.Create("xyz");
        Assert.IsFalse(a.Equals(b));
    }
}
