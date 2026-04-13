using System.Text.RegularExpressions;
using Catharsis.Text.RegularExpressions;

namespace Catharsis.UnitTests.Text.RegularExpressions;

///<summary>
///Unit tests for the <see cref="RegexTokenizer"/> class.
///</summary>
[TestClass]
public class RegexTokenizerTests
{
    [TestMethod]
    public void Tokenize_ReturnsMatchResults()
    {
        RegexTokenizer tokenizer = new(@"\d+");
        IReadOnlyList<MatchResult> tokens = tokenizer.Tokenize("abc123def456");

        Assert.HasCount(2, tokens);
        Assert.AreEqual("123", tokens[0].Value);
        Assert.AreEqual(3, tokens[0].Index);
        Assert.AreEqual(3, tokens[0].Length);
        Assert.AreEqual("456", tokens[1].Value);
        Assert.AreEqual(9, tokens[1].Index);
    }

    [TestMethod]
    public void Tokenize_NoMatches_ReturnsEmpty()
    {
        RegexTokenizer tokenizer = new(@"\d+");
        IReadOnlyList<MatchResult> tokens = tokenizer.Tokenize("no digits here");
        Assert.IsEmpty(tokens);
    }

    [TestMethod]
    public void Tokenize_NullInput_Throws()
    {
        RegexTokenizer tokenizer = new(@"\w+");
        Assert.ThrowsExactly<ArgumentNullException>(() => tokenizer.Tokenize(null!));
    }

    [TestMethod]
    public void Constructor_NullStringPattern_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => new RegexTokenizer((string)null!));
    }

    [TestMethod]
    public void Constructor_NullRegex_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => new RegexTokenizer((Regex)null!));
    }

    [TestMethod]
    public void Constructor_RegexInstance_Works()
    {
        Regex pattern = new(@"\w+", RegexOptions.Compiled);
        RegexTokenizer tokenizer = new(pattern);
        IReadOnlyList<MatchResult> tokens = tokenizer.Tokenize("hello world");

        Assert.HasCount(2, tokens);
        Assert.AreEqual("hello", tokens[0].Value);
        Assert.AreEqual("world", tokens[1].Value);
    }

    [TestMethod]
    public void Split_ReturnsSegmentsBetweenTokens()
    {
        RegexTokenizer tokenizer = new(@"\d+");
        string[] segments = tokenizer.Split("abc123def456ghi");

        CollectionAssert.AreEqual(new[] { "abc", "def", "ghi" }, segments);
    }

    [TestMethod]
    public void Split_NullInput_Throws()
    {
        RegexTokenizer tokenizer = new(@"\d+");
        Assert.ThrowsExactly<ArgumentNullException>(() => tokenizer.Split(null!));
    }

    [TestMethod]
    public void Count_ReturnsNumberOfMatches()
    {
        RegexTokenizer tokenizer = new(@"\b\w+\b");
        Assert.AreEqual(3, tokenizer.Count("one two three"));
    }

    [TestMethod]
    public void Count_NullInput_Throws()
    {
        RegexTokenizer tokenizer = new(@"\w+");
        Assert.ThrowsExactly<ArgumentNullException>(() => tokenizer.Count(null!));
    }

    [TestMethod]
    public void Tokenize_WithOptions_RespectsRegexOptions()
    {
        RegexTokenizer tokenizer = new(@"[a-z]+", RegexOptions.IgnoreCase);
        IReadOnlyList<MatchResult> tokens = tokenizer.Tokenize("ABC def");

        Assert.HasCount(2, tokens);
        Assert.AreEqual("ABC", tokens[0].Value);
        Assert.AreEqual("def", tokens[1].Value);
    }

    [TestMethod]
    public void Tokenize_EmailExtraction_Scenario()
    {
        // CommonPatterns.Email() uses ^...$ anchors for full-string validation,
        // so we use a non-anchored pattern for embedded extraction.
        RegexTokenizer tokenizer = new(@"[^\s@]+@[^\s@]+\.[^\s@]+");
        IReadOnlyList<MatchResult> tokens = tokenizer.Tokenize("Contact us at info@example.com or support@test.org");

        Assert.HasCount(2, tokens);
        Assert.AreEqual("info@example.com", tokens[0].Value);
        Assert.AreEqual("support@test.org", tokens[1].Value);
    }

    [TestMethod]
    public void Tokenize_EmptyInput_ReturnsEmpty()
    {
        RegexTokenizer tokenizer = new(@"\w+");
        IReadOnlyList<MatchResult> tokens = tokenizer.Tokenize("");
        Assert.IsEmpty(tokens);
    }
}
