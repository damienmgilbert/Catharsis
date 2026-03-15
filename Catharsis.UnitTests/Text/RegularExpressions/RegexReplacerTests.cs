using System.Text.RegularExpressions;
using Catharsis.Text.RegularExpressions;

namespace Catharsis.UnitTests.Text.RegularExpressions;

///<summary>
///Unit tests for the <see cref="RegexReplacer"/> class.
///</summary>
[TestClass]
public class RegexReplacerTests
{
    [TestMethod]
    public void Replace_StringPattern_AppliesReplacement()
    {
        string result = new RegexReplacer()
            .Replace(@"\d+", "#")
            .Apply("abc123def456");

        Assert.AreEqual("abc#def#", result);
    }

    [TestMethod]
    public void Replace_RegexInstance_AppliesReplacement()
    {
        Regex vowels = new(@"[aeiou]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        string result = new RegexReplacer()
            .Replace(vowels, "*")
            .Apply("Hello World");

        Assert.AreEqual("H*ll* W*rld", result);
    }

    [TestMethod]
    public void Replace_MultipleRules_AppliedInOrder()
    {
        string result = new RegexReplacer()
            .Replace(@"\s+", " ")
            .Replace(@"^\s+|\s+$", "")
            .Apply("  hello   world  ");

        Assert.AreEqual("hello world", result);
    }

    [TestMethod]
    public void Replace_GroupReferences_Work()
    {
        string result = new RegexReplacer()
            .Replace(@"(\w+)\s(\w+)", "$2 $1")
            .Apply("hello world");

        Assert.AreEqual("world hello", result);
    }

    [TestMethod]
    public void Apply_EmptyRules_ReturnsInput()
    {
        string result = new RegexReplacer().Apply("unchanged");
        Assert.AreEqual("unchanged", result);
    }

    [TestMethod]
    public void Apply_NullInput_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new RegexReplacer().Apply(null!));
    }

    [TestMethod]
    public void Replace_NullStringPattern_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new RegexReplacer().Replace((string)null!, "x"));
    }

    [TestMethod]
    public void Replace_NullStringReplacement_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new RegexReplacer().Replace("x", (string)null!));
    }

    [TestMethod]
    public void Replace_NullRegex_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new RegexReplacer().Replace((Regex)null!, "x"));
    }

    [TestMethod]
    public void Replace_NullRegexReplacement_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new RegexReplacer().Replace(new Regex("x"), null!));
    }

    [TestMethod]
    public void Count_ReflectsRuleCount()
    {
        RegexReplacer replacer = new RegexReplacer()
            .Replace("a", "b")
            .Replace("c", "d");

        Assert.AreEqual(2, replacer.Count);
    }

    [TestMethod]
    public void Clear_RemovesAllRules()
    {
        RegexReplacer replacer = new RegexReplacer()
            .Replace("a", "b")
            .Clear();

        Assert.AreEqual(0, replacer.Count);
        Assert.AreEqual("abc", replacer.Apply("abc"));
    }

    [TestMethod]
    public void Replace_WithRegexOptions_Respects()
    {
        string result = new RegexReplacer()
            .Replace("hello", "HI", RegexOptions.IgnoreCase)
            .Apply("Hello HELLO hello");

        Assert.AreEqual("HI HI HI", result);
    }

    [TestMethod]
    public void Replace_HtmlTagStripping_Scenario()
    {
        string result = new RegexReplacer()
            .Replace(@"<[^>]+>", "")
            .Replace(@"\s+", " ")
            .Replace(@"^\s+|\s+$", "")
            .Apply("<p>Hello  <b>World</b></p>");

        Assert.AreEqual("Hello World", result);
    }
}
