using Catharsis.HighPerformance;

namespace Catharsis.UnitTests.HighPerformance;

///<summary>
///Unit tests for the <see cref="SpanTokenizer"/> class.
///</summary>
[TestClass]
public class SpanTokenizerTests
{
    #region Public methods
    [TestMethod]
    public void Count_ReturnsCorrectTokenCount()
    {
        Assert.AreEqual(3, SpanTokenizer.Count("a,b,c".AsSpan(), ','));
        Assert.AreEqual(1, SpanTokenizer.Count("hello".AsSpan(), ','));
        Assert.AreEqual(0, SpanTokenizer.Count([], ','));
    }

    [TestMethod]
    public void HasMore_InitiallyTrue()
    {
        SpanTokenizer tokenizer = new("a,b".AsSpan(), ',');
        Assert.IsTrue(tokenizer.HasMore);
    }

    [TestMethod]
    public void Reset_AllowsReTokenization()
    {
        SpanTokenizer tokenizer = new("a,b".AsSpan(), ',');
        while (tokenizer.TryGetNext(out _))
        {
        }
        Assert.IsFalse(tokenizer.HasMore);

        tokenizer.Reset("x,y,z".AsSpan());
        Assert.IsTrue(tokenizer.HasMore);
        int count = 0;
        while (tokenizer.TryGetNext(out _))
        {
            count++;
        }

        Assert.AreEqual(3, count);
    }

    [TestMethod]
    public void TryGetNext_EmptySpan_ReturnsSingleEmptyToken()
    {
        SpanTokenizer tokenizer = new([], ',');
        Assert.IsTrue(tokenizer.TryGetNext(out ReadOnlySpan<char> token));
        Assert.AreEqual(0, token.Length);
        Assert.IsFalse(tokenizer.TryGetNext(out _));
    }

    [TestMethod]
    public void TryGetNext_SingleToken_ReturnsWholeSpan()
    {
        SpanTokenizer tokenizer = new("hello".AsSpan(), ',');
        Assert.IsTrue(tokenizer.TryGetNext(out ReadOnlySpan<char> token));
        Assert.AreEqual("hello", token.ToString());
        Assert.IsFalse(tokenizer.TryGetNext(out _));
    }

    [TestMethod]
    public void TryGetNext_TokenizesCorrectly()
    {
        SpanTokenizer tokenizer = new("a,b,c".AsSpan(), ',');
        List<string> tokens = [];

        while (tokenizer.TryGetNext(out ReadOnlySpan<char> token))
        {
            tokens.Add(token.ToString());
        }

        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, tokens);
    }
    #endregion
}
