using Catharsis.IO;
using System.Text;

namespace Catharsis.UnitTests.IO;

///<summary>
///Unit tests for the <see cref="LineReader"/> class.
///</summary>
[TestClass]
public class LineReaderTests
{
    #region Private methods
    private static MemoryStream CreateStream(string text) => new(Encoding.UTF8.GetBytes(text));
    #endregion

    #region Constructor

    [TestMethod]
    public void Constructor_NullStream_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new LineReader(null!)); }

    [TestMethod]
    public void Constructor_NonPositiveBufferSize_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new LineReader(new MemoryStream(), bufferSize: 0)); }

    #endregion

    #region ReadLines

    [TestMethod]
    public void ReadLines_NullCallback_Throws()
    {
        using LineReader reader = new(CreateStream("a\nb"));
        Assert.ThrowsExactly<ArgumentNullException>(() => reader.ReadLines<object?>(null!, null));
    }

    [TestMethod]
    public void ReadLines_SimpleLines_YieldsEachLine()
    {
        using LineReader reader = new(CreateStream("first\nsecond\nthird"));
        List<string> lines = [];

        reader.ReadLines((line, state) => state.Add(line.ToString()), lines);

        CollectionAssert.AreEqual(new[] { "first", "second", "third" }, lines);
    }

    [TestMethod]
    public void ReadLines_CrLfLineEndings_TrimsCarriageReturn()
    {
        using LineReader reader = new(CreateStream("first\r\nsecond\r\n"));
        List<string> lines = [];

        reader.ReadLines((line, state) => state.Add(line.ToString()), lines);

        CollectionAssert.AreEqual(new[] { "first", "second" }, lines);
    }

    [TestMethod]
    public void ReadLines_EmptyStream_YieldsNoLines()
    {
        using LineReader reader = new(CreateStream(""));
        List<string> lines = [];

        reader.ReadLines((line, state) => state.Add(line.ToString()), lines);

        Assert.IsEmpty(lines);
    }

    [TestMethod]
    public void ReadLines_TrailingNewline_DoesNotYieldEmptyFinalLine()
    {
        using LineReader reader = new(CreateStream("only-line\n"));
        List<string> lines = [];

        reader.ReadLines((line, state) => state.Add(line.ToString()), lines);

        CollectionAssert.AreEqual(new[] { "only-line" }, lines);
    }

    [TestMethod]
    public void ReadLines_NoTrailingNewline_StillYieldsFinalLine()
    {
        using LineReader reader = new(CreateStream("only-line"));
        List<string> lines = [];

        reader.ReadLines((line, state) => state.Add(line.ToString()), lines);

        CollectionAssert.AreEqual(new[] { "only-line" }, lines);
    }

    [TestMethod]
    public void ReadLines_LineLongerThanBufferSize_GrowsBufferAndYieldsWholeLine()
    {
        string longLine = new('x', 500);
        using LineReader reader = new(CreateStream($"{longLine}\nshort"), bufferSize: 16);
        List<string> lines = [];

        reader.ReadLines((line, state) => state.Add(line.ToString()), lines);

        CollectionAssert.AreEqual(new[] { longLine, "short" }, lines);
    }

    [TestMethod]
    public void ReadLines_ManyLinesAcrossMultipleBufferFills_YieldsAllInOrder()
    {
        string[] expected = [.. Enumerable.Range(0, 1000).Select(static i => $"line-{i}")];
        using LineReader reader = new(CreateStream(string.Join('\n', expected)), bufferSize: 64);
        List<string> lines = [];

        reader.ReadLines((line, state) => state.Add(line.ToString()), lines);

        CollectionAssert.AreEqual(expected, lines);
    }

    [TestMethod]
    public void ReadLines_EmptyLinesInMiddle_AreYieldedAsEmptyStrings()
    {
        using LineReader reader = new(CreateStream("a\n\nb"));
        List<string> lines = [];

        reader.ReadLines((line, state) => state.Add(line.ToString()), lines);

        CollectionAssert.AreEqual(new[] { "a", "", "b" }, lines);
    }

    #endregion
}
