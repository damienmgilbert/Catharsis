using Catharsis.Text.RegularExpressions;

namespace Catharsis.UnitTests.Text.RegularExpressions;

///<summary>
///Unit tests for the <see cref="CsvLineTokenizer"/> class.
///</summary>
[TestClass]
public class CsvLineTokenizerTests
{
    #region Tokenize

    [TestMethod]
    public void Tokenize_NullLine_Throws()
    {
        CsvLineTokenizer tokenizer = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => tokenizer.Tokenize(null!));
    }

    [TestMethod]
    public void Tokenize_SimpleUnquotedFields_SplitsOnDelimiter()
    {
        CsvLineTokenizer tokenizer = new();
        IReadOnlyList<string> fields = tokenizer.Tokenize("a,b,c");
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, fields.ToList());
    }

    [TestMethod]
    public void Tokenize_EmptyLine_ReturnsSingleEmptyField()
    {
        CsvLineTokenizer tokenizer = new();
        IReadOnlyList<string> fields = tokenizer.Tokenize("");
        CollectionAssert.AreEqual(new[] { "" }, fields.ToList());
    }

    [TestMethod]
    public void Tokenize_QuotedFieldContainingDelimiter_KeepsFieldIntact()
    {
        CsvLineTokenizer tokenizer = new();
        IReadOnlyList<string> fields = tokenizer.Tokenize("a,\"b,c\",d");
        CollectionAssert.AreEqual(new[] { "a", "b,c", "d" }, fields.ToList());
    }

    [TestMethod]
    public void Tokenize_QuotedFieldWithEscapedQuote_Unescapes()
    {
        CsvLineTokenizer tokenizer = new();
        IReadOnlyList<string> fields = tokenizer.Tokenize("a,\"say \"\"hi\"\"\",b");
        CollectionAssert.AreEqual(new[] { "a", "say \"hi\"", "b" }, fields.ToList());
    }

    [TestMethod]
    public void Tokenize_TrailingEmptyField_IsIncluded()
    {
        CsvLineTokenizer tokenizer = new();
        IReadOnlyList<string> fields = tokenizer.Tokenize("a,b,");
        CollectionAssert.AreEqual(new[] { "a", "b", "" }, fields.ToList());
    }

    [TestMethod]
    public void Tokenize_CustomDelimiter_SplitsOnConfiguredCharacter()
    {
        CsvLineTokenizer tokenizer = new(delimiter: ';');
        IReadOnlyList<string> fields = tokenizer.Tokenize("a;b;c");
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, fields.ToList());
    }

    [TestMethod]
    public void Tokenize_EmptyQuotedField_ReturnsEmptyString()
    {
        CsvLineTokenizer tokenizer = new();
        IReadOnlyList<string> fields = tokenizer.Tokenize("a,\"\",c");
        CollectionAssert.AreEqual(new[] { "a", "", "c" }, fields.ToList());
    }

    #endregion
}
