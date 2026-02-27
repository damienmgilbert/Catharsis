using CommunityToolkit.HighPerformance.Buffers;
using System.Buffers;

namespace Catharsis.HighPerformance.UnitTests;

[TestClass]
public class PooledListTests
{
    [TestMethod]
    public void Constructor_Default_CreatesEmptyList()
    {
        using var list = new PooledList<int>();
        Assert.AreEqual(0, list.Count);
        Assert.IsTrue(list.Capacity >= 16);
    }

    [TestMethod]
    public void Add_IncreasesCount()
    {
        using var list = new PooledList<int>();
        list.Add(1); list.Add(2);
        Assert.AreEqual(2, list.Count);
        Assert.AreEqual(1, list[0]);
        Assert.AreEqual(2, list[1]);
    }

    [TestMethod]
    public void AddRange_AddsMultipleItems()
    {
        using var list = new PooledList<int>();
        list.AddRange([1, 2, 3]);
        Assert.AreEqual(3, list.Count);
    }

    [TestMethod]
    public void Insert_InsertsAtIndex()
    {
        using var list = new PooledList<int>();
        list.Add(1); list.Add(3);
        list.Insert(1, 2);
        Assert.AreEqual(3, list.Count);
        Assert.AreEqual(2, list[1]);
    }

    [TestMethod]
    public void Remove_RemovesItem()
    {
        using var list = new PooledList<int>();
        list.Add(1); list.Add(2);
        Assert.IsTrue(list.Remove(1));
        Assert.AreEqual(1, list.Count);
    }

    [TestMethod]
    public void RemoveAt_RemovesAtIndex()
    {
        using var list = new PooledList<int>();
        list.Add(10); list.Add(20); list.Add(30);
        list.RemoveAt(1);
        Assert.AreEqual(2, list.Count);
        Assert.AreEqual(30, list[1]);
    }

    [TestMethod]
    public void IndexOf_ReturnsCorrectIndex()
    {
        using var list = new PooledList<int>();
        list.Add(10); list.Add(20);
        Assert.AreEqual(1, list.IndexOf(20));
        Assert.AreEqual(-1, list.IndexOf(99));
    }

    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        using var list = new PooledList<string>();
        list.Add("hello");
        Assert.IsTrue(list.Contains("hello"));
        Assert.IsFalse(list.Contains("world"));
    }

    [TestMethod]
    public void Clear_ResetsCount()
    {
        using var list = new PooledList<int>();
        list.Add(1); list.Add(2);
        list.Clear();
        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public void Indexer_SetValue()
    {
        using var list = new PooledList<int>();
        list.Add(1);
        list[0] = 42;
        Assert.AreEqual(42, list[0]);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var list = new PooledList<int>();
        list.Dispose();
        list.Dispose();
    }

    [TestMethod]
    public void Add_GrowsAutomatically()
    {
        using var list = new PooledList<int>(2);
        for (int i = 0; i < 100; i++) list.Add(i);
        Assert.AreEqual(100, list.Count);
    }

    [TestMethod]
    public void Enumeration_ReturnsAllItems()
    {
        using var list = new PooledList<int>();
        list.Add(1); list.Add(2); list.Add(3);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToList());
    }
}

[TestClass]
public class PooledDictionaryTests
{
    [TestMethod]
    public void Add_And_Retrieve()
    {
        using var dict = new PooledDictionary<string, int>();
        dict.Add("key", 42);
        Assert.AreEqual(42, dict["key"]);
        Assert.AreEqual(1, dict.Count);
    }

    [TestMethod]
    public void TryAdd_Duplicate_ReturnsFalse()
    {
        using var dict = new PooledDictionary<string, int>();
        dict.Add("key", 1);
        Assert.IsFalse(dict.TryAdd("key", 2));
    }

    [TestMethod]
    public void Remove_ExistingKey_ReturnsTrue()
    {
        using var dict = new PooledDictionary<string, int>();
        dict.Add("key", 1);
        Assert.IsTrue(dict.Remove("key"));
        Assert.AreEqual(0, dict.Count);
    }

    [TestMethod]
    public void ContainsKey_ReturnsCorrectResult()
    {
        using var dict = new PooledDictionary<string, int>();
        dict.Add("a", 1);
        Assert.IsTrue(dict.ContainsKey("a"));
        Assert.IsFalse(dict.ContainsKey("b"));
    }

    [TestMethod]
    public void TryGetValue_ReturnsCorrectResult()
    {
        using var dict = new PooledDictionary<string, int>();
        dict.Add("a", 42);
        Assert.IsTrue(dict.TryGetValue("a", out int val));
        Assert.AreEqual(42, val);
    }

    [TestMethod]
    public void Clear_RemovesAll()
    {
        using var dict = new PooledDictionary<string, int>();
        dict.Add("a", 1);
        dict.Clear();
        Assert.AreEqual(0, dict.Count);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var dict = new PooledDictionary<string, int>();
        dict.Dispose();
        dict.Dispose();
    }

    [TestMethod]
    public void Indexer_AfterDispose_Throws()
    {
        var dict = new PooledDictionary<string, int>();
        dict.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = dict["key"]);
    }
}

[TestClass]
public class SpanTokenizerTests
{
    [TestMethod]
    public void TryGetNext_TokenizesCorrectly()
    {
        var tokenizer = new SpanTokenizer("a,b,c".AsSpan(), ',');
        var tokens = new List<string>();

        while (tokenizer.TryGetNext(out var token))
            tokens.Add(token.ToString());

        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, tokens);
    }

    [TestMethod]
    public void TryGetNext_SingleToken_ReturnsWholeSpan()
    {
        var tokenizer = new SpanTokenizer("hello".AsSpan(), ',');
        Assert.IsTrue(tokenizer.TryGetNext(out var token));
        Assert.AreEqual("hello", token.ToString());
        Assert.IsFalse(tokenizer.TryGetNext(out _));
    }

    [TestMethod]
    public void TryGetNext_EmptySpan_ReturnsSingleEmptyToken()
    {
        var tokenizer = new SpanTokenizer(ReadOnlySpan<char>.Empty, ',');
        Assert.IsTrue(tokenizer.TryGetNext(out var token));
        Assert.AreEqual(0, token.Length);
        Assert.IsFalse(tokenizer.TryGetNext(out _));
    }

    [TestMethod]
    public void Count_ReturnsCorrectTokenCount()
    {
        Assert.AreEqual(3, SpanTokenizer.Count("a,b,c".AsSpan(), ','));
        Assert.AreEqual(1, SpanTokenizer.Count("hello".AsSpan(), ','));
        Assert.AreEqual(0, SpanTokenizer.Count(ReadOnlySpan<char>.Empty, ','));
    }

    [TestMethod]
    public void HasMore_InitiallyTrue()
    {
        var tokenizer = new SpanTokenizer("a,b".AsSpan(), ',');
        Assert.IsTrue(tokenizer.HasMore);
    }

    [TestMethod]
    public void Reset_AllowsReTokenization()
    {
        var tokenizer = new SpanTokenizer("a,b".AsSpan(), ',');
        while (tokenizer.TryGetNext(out _)) { }
        Assert.IsFalse(tokenizer.HasMore);

        tokenizer.Reset("x,y,z".AsSpan());
        Assert.IsTrue(tokenizer.HasMore);
        int count = 0;
        while (tokenizer.TryGetNext(out _)) count++;
        Assert.AreEqual(3, count);
    }
}

[TestClass]
public class BitSpanTests
{
    [TestMethod]
    public void Indexer_SetAndGetBit()
    {
        byte[] storage = new byte[2];
        var bits = new BitSpan(storage, 16);

        bits[0] = true;
        bits[7] = true;
        bits[8] = true;

        Assert.IsTrue(bits[0]);
        Assert.IsTrue(bits[7]);
        Assert.IsTrue(bits[8]);
        Assert.IsFalse(bits[1]);
    }

    [TestMethod]
    public void Length_ReturnsCorrectBitCount()
    {
        byte[] storage = new byte[3];
        var bits = new BitSpan(storage, 20);
        Assert.AreEqual(20, bits.Length);
        Assert.AreEqual(3, bits.ByteLength);
    }

    [TestMethod]
    public void Fill_True_SetsAllBits()
    {
        byte[] storage = new byte[2];
        var bits = new BitSpan(storage, 16);
        bits.Fill(true);
        Assert.IsTrue(bits[0]);
        Assert.IsTrue(bits[15]);
    }

    [TestMethod]
    public void Clear_ClearsAllBits()
    {
        byte[] storage = new byte[2];
        var bits = new BitSpan(storage, 16);
        bits.Fill(true);
        bits.Clear();
        Assert.IsFalse(bits[0]);
    }

    [TestMethod]
    public void PopCount_CountsSetBits()
    {
        byte[] storage = new byte[1];
        var bits = new BitSpan(storage, 8);
        bits[0] = true;
        bits[2] = true;
        bits[4] = true;
        Assert.AreEqual(3, bits.PopCount());
    }

    [TestMethod]
    public void Not_InvertsAllBits()
    {
        byte[] storage = new byte[1];
        var bits = new BitSpan(storage, 8);
        bits[0] = true;
        bits.Not();
        Assert.IsFalse(bits[0]);
        Assert.IsTrue(bits[1]);
    }

    [TestMethod]
    public void GetByteCount_ReturnsCorrectByteCount()
    {
        Assert.AreEqual(1, BitSpan.GetByteCount(1));
        Assert.AreEqual(1, BitSpan.GetByteCount(8));
        Assert.AreEqual(2, BitSpan.GetByteCount(9));
    }
}

[TestClass]
public class ImageBufferTests
{
    [TestMethod]
    public void Constructor_SetsWidthAndHeight()
    {
        using var buf = new ImageBuffer<int>(10, 20);
        Assert.AreEqual(10, buf.Width);
        Assert.AreEqual(20, buf.Height);
        Assert.AreEqual(200, buf.PixelCount);
    }

    [TestMethod]
    public void Indexer_SetAndGetPixel()
    {
        using var buf = new ImageBuffer<byte>(5, 5);
        buf[2, 3] = 0xFF;
        Assert.AreEqual(0xFF, buf[2, 3]);
    }

    [TestMethod]
    public void GetRowSpan_ReturnsCorrectRow()
    {
        using var buf = new ImageBuffer<int>(3, 3);
        buf[0, 1] = 42;
        var row = buf.GetRowSpan(1);
        Assert.AreEqual(3, row.Length);
        Assert.AreEqual(42, row[0]);
    }

    [TestMethod]
    public void GetPixelSpan_ReturnsAllPixels()
    {
        using var buf = new ImageBuffer<int>(3, 2);
        var span = buf.GetPixelSpan();
        Assert.AreEqual(6, span.Length);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var buf = new ImageBuffer<int>(2, 2);
        buf.Dispose();
        buf.Dispose();
    }
}

[TestClass]
public class MemoryOwnerExtensionsTests
{
    [TestMethod]
    public void ToMemoryOwner_FromSpan_CopiesData()
    {
        ReadOnlySpan<int> data = [1, 2, 3];
        using var owner = data.ToMemoryOwner();
        Assert.AreEqual(3, owner.Length);
        Assert.AreEqual(1, owner.Span[0]);
        Assert.AreEqual(3, owner.Span[2]);
    }

    [TestMethod]
    public void ToMemoryOwner_FromMemory_CopiesData()
    {
        ReadOnlyMemory<byte> data = new byte[] { 10, 20 };
        using var owner = data.ToMemoryOwner();
        Assert.AreEqual(2, owner.Length);
        Assert.AreEqual(10, owner.Span[0]);
    }

    [TestMethod]
    public void WriteTo_WritesToBufferWriter()
    {
        using var owner = MemoryOwner<byte>.Allocate(3);
        owner.Span[0] = 1; owner.Span[1] = 2; owner.Span[2] = 3;
        var writer = new ArrayBufferWriter<byte>();

        owner.WriteTo(writer);

        Assert.AreEqual(3, writer.WrittenCount);
        Assert.AreEqual(1, writer.WrittenSpan[0]);
    }

    [TestMethod]
    public void SliceCopy_ReturnsSlicedCopy()
    {
        using var owner = MemoryOwner<int>.Allocate(5);
        for (int i = 0; i < 5; i++) owner.Span[i] = i * 10;

        using var sliced = owner.SliceCopy(1, 3);

        Assert.AreEqual(3, sliced.Length);
        Assert.AreEqual(10, sliced.Span[0]);
        Assert.AreEqual(30, sliced.Span[2]);
    }

    [TestMethod]
    public void Fill_FillsEntireOwner()
    {
        using var owner = MemoryOwner<int>.Allocate(5);
        owner.Fill(42);
        foreach (int v in owner.Span)
            Assert.AreEqual(42, v);
    }

    [TestMethod]
    public void Clear_ClearsToDefault()
    {
        using var owner = MemoryOwner<int>.Allocate(3);
        owner.Fill(99);
        owner.Clear();
        foreach (int v in owner.Span)
            Assert.AreEqual(0, v);
    }
}
