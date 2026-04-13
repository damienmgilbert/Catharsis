using Catharsis.Buffers;

namespace Catharsis.UnitTests.Buffers;

///<summary>
///Unit tests for the <see cref="PooledStringBuilder"/> class.
///</summary>
[TestClass]
public class PooledStringBuilderTests
{
    #region Public methods
    ///<summary>
    ///Tests that Advance throws after disposal.
    ///</summary>
    [TestMethod]
    public void Advance_AfterDispose_ThrowsObjectDisposedException()
    {
        PooledStringBuilder sb = new();
        sb.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => sb.Advance(1));
    }

    ///<summary>
    ///Tests IBufferWriter Advance method.
    ///</summary>
    [TestMethod]
    public void Advance_IncreasesLength()
    {
        using PooledStringBuilder sb = new();
        sb.GetSpan(5);

        sb.Advance(3);

        Assert.AreEqual(3, sb.Length);
    }

    ///<summary>
    ///Tests that Advance throws for negative count.
    ///</summary>
    [TestMethod]
    public void Advance_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        using PooledStringBuilder sb = new();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => sb.Advance(-1));
    }

    ///<summary>
    ///Tests that Append(char) throws when disposed.
    ///</summary>
    [TestMethod]
    public void Append_Char_AfterDispose_ThrowsObjectDisposedException()
    {
        PooledStringBuilder sb = new();
        sb.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => sb.Append('A'));
    }

    ///<summary>
    ///Tests that Append(char) appends a single character.
    ///</summary>
    [TestMethod]
    public void Append_Char_AppendsSingleCharacter()
    {
        using PooledStringBuilder sb = new();

        sb.Append('A');

        Assert.AreEqual(1, sb.Length);
        Assert.AreEqual("A", sb.ToString());
    }

    ///<summary>
    ///Tests that Append with empty span does nothing.
    ///</summary>
    [TestMethod]
    public void Append_EmptySpan_DoesNotChangeLength()
    {
        using PooledStringBuilder sb = new();

        sb.Append([]);

        Assert.AreEqual(0, sb.Length);
    }

    ///<summary>
    ///Tests that the buffer grows when capacity is exceeded.
    ///</summary>
    [TestMethod]
    public void Append_ExceedsCapacity_GrowsBuffer()
    {
        using PooledStringBuilder sb = new(16);
        string longText = new('X', 100);

        sb.Append(longText);

        Assert.AreEqual(longText, sb.ToString());
        Assert.IsGreaterThanOrEqualTo(100, sb.Capacity);
    }

    ///<summary>
    ///Tests that Append&lt;T&gt; formats an integer.
    ///</summary>
    [TestMethod]
    public void Append_ISpanFormattable_FormatsValue()
    {
        using PooledStringBuilder sb = new();

        sb.Append(42);

        Assert.AreEqual("42", sb.ToString());
    }

    ///<summary>
    ///Tests multiple append types in sequence.
    ///</summary>
    [TestMethod]
    public void Append_MixedTypes_ProducesCorrectString()
    {
        using PooledStringBuilder sb = new();

        sb.Append("Count: ");
        sb.Append(42);
        sb.Append(' ');
        sb.AppendLine("done");

        Assert.AreEqual($"Count: 42 done{Environment.NewLine}", sb.ToString());
    }

    ///<summary>
    ///Tests that Append(null string) does nothing.
    ///</summary>
    [TestMethod]
    public void Append_NullString_DoesNothing()
    {
        using PooledStringBuilder sb = new();

        sb.Append((string?)null);

        Assert.AreEqual(0, sb.Length);
    }

    ///<summary>
    ///Tests that Append(ReadOnlySpan&lt;char&gt;) appends characters.
    ///</summary>
    [TestMethod]
    public void Append_Span_AppendsCharacters()
    {
        using PooledStringBuilder sb = new();
        ReadOnlySpan<char> text = "Hello".AsSpan();

        sb.Append(text);

        Assert.AreEqual("Hello", sb.ToString());
    }

    ///<summary>
    ///Tests that Append(string) appends the string.
    ///</summary>
    [TestMethod]
    public void Append_String_AppendsString()
    {
        using PooledStringBuilder sb = new();

        sb.Append("World");

        Assert.AreEqual("World", sb.ToString());
    }

    ///<summary>
    ///Tests that AppendLine appends a line break.
    ///</summary>
    [TestMethod]
    public void AppendLine_AppendsNewLine()
    {
        using PooledStringBuilder sb = new();

        sb.AppendLine();

        Assert.AreEqual(Environment.NewLine, sb.ToString());
    }

    ///<summary>
    ///Tests that AppendLine(string) appends string followed by line break.
    ///</summary>
    [TestMethod]
    public void AppendLine_String_AppendsStringAndNewLine()
    {
        using PooledStringBuilder sb = new();

        sb.AppendLine("Hello");

        Assert.AreEqual($"Hello{Environment.NewLine}", sb.ToString());
    }

    ///<summary>
    ///Tests that Clear allows reuse.
    ///</summary>
    [TestMethod]
    public void Clear_AllowsReuse()
    {
        using PooledStringBuilder sb = new();
        sb.Append("Old");
        sb.Clear();

        sb.Append("New");

        Assert.AreEqual("New", sb.ToString());
    }

    ///<summary>
    ///Tests that Clear resets the length.
    ///</summary>
    [TestMethod]
    public void Clear_ResetsLength()
    {
        using PooledStringBuilder sb = new();
        sb.Append("Hello");

        sb.Clear();

        Assert.AreEqual(0, sb.Length);
    }

    ///<summary>
    ///Tests that the default constructor creates a valid instance.
    ///</summary>
    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        using PooledStringBuilder sb = new();

        Assert.AreEqual(0, sb.Length);
        Assert.IsGreaterThanOrEqualTo(256, sb.Capacity);
    }

    ///<summary>
    ///Tests that the constructor throws when pool is null.
    ///</summary>
    [TestMethod]
    public void Constructor_NullPool_ThrowsArgumentNullException() { Assert.ThrowsExactly<ArgumentNullException>(static () => new PooledStringBuilder(null!, 256)); }
    ///<summary>
    ///Tests that the constructor throws when capacity is zero.
    ///</summary>
    [TestMethod]
    public void Constructor_ZeroCapacity_ThrowsArgumentOutOfRangeException() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new PooledStringBuilder(0)); }
    ///<summary>
    ///Tests that Dispose is idempotent.
    ///</summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        PooledStringBuilder sb = new();

        sb.Dispose();
        sb.Dispose();
    }

    ///<summary>
    ///Tests that GetMemory throws after disposal.
    ///</summary>
    [TestMethod]
    public void GetMemory_AfterDispose_ThrowsObjectDisposedException()
    {
        PooledStringBuilder sb = new();
        sb.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => sb.GetMemory(1));
    }

    ///<summary>
    ///Tests that GetMemory returns writable memory.
    ///</summary>
    [TestMethod]
    public void GetMemory_ReturnsWritableMemory()
    {
        using PooledStringBuilder sb = new();

        Memory<char> mem = sb.GetMemory(10);

        Assert.IsGreaterThanOrEqualTo(10, mem.Length);
    }

    ///<summary>
    ///Tests that GetSpan throws after disposal.
    ///</summary>
    [TestMethod]
    public void GetSpan_AfterDispose_ThrowsObjectDisposedException()
    {
        PooledStringBuilder sb = new();
        sb.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => sb.GetSpan(1));
    }

    ///<summary>
    ///Tests that GetSpan returns writable span.
    ///</summary>
    [TestMethod]
    public void GetSpan_ReturnsWritableSpan()
    {
        using PooledStringBuilder sb = new();

        Span<char> span = sb.GetSpan(10);

        Assert.IsGreaterThanOrEqualTo(10, span.Length);
    }

    ///<summary>
    ///Tests that ToString returns the accumulated string.
    ///</summary>
    [TestMethod]
    public void ToString_ReturnsAccumulatedString()
    {
        using PooledStringBuilder sb = new();
        sb.Append("Hello, ");
        sb.Append("World!");

        string result = sb.ToString();

        Assert.AreEqual("Hello, World!", result);
    }

    ///<summary>
    ///Tests that ToStringAndDispose returns string and disposes.
    ///</summary>
    [TestMethod]
    public void ToStringAndDispose_ReturnsStringAndDisposes()
    {
        PooledStringBuilder sb = new();
        sb.Append("Test");

        string result = sb.ToStringAndDispose();

        Assert.AreEqual("Test", result);
        // Verify disposed — Append should throw
        Assert.ThrowsExactly<ObjectDisposedException>(() => sb.Append('X'));
    }

    ///<summary>
    ///Tests that WrittenSpan returns the correct span.
    ///</summary>
    [TestMethod]
    public void WrittenSpan_ReturnsWrittenCharacters()
    {
        using PooledStringBuilder sb = new();
        sb.Append("ABC");

        ReadOnlySpan<char> span = sb.WrittenSpan;

        Assert.AreEqual(3, span.Length);
        Assert.AreEqual('A', span[0]);
        Assert.AreEqual('B', span[1]);
        Assert.AreEqual('C', span[2]);
    }
    #endregion
}
