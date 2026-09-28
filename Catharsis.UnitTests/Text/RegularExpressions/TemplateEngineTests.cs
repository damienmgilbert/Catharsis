using Catharsis.Text.RegularExpressions;

namespace Catharsis.UnitTests.Text.RegularExpressions;

///<summary>
///Unit tests for the <see cref="TemplateEngine"/> class.
///</summary>
[TestClass]
public class TemplateEngineTests
{
    #region Render

    [TestMethod]
    public void Render_NullTemplate_Throws()
    {
        TemplateEngine engine = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => engine.Render(null!, new Dictionary<string, string?>()));
    }

    [TestMethod]
    public void Render_NullValues_Throws()
    {
        TemplateEngine engine = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => engine.Render("hello", null!));
    }

    [TestMethod]
    public void Render_SingleKnownToken_SubstitutesValue()
    {
        TemplateEngine engine = new();
        string result = engine.Render("Hello, {{name}}!", new Dictionary<string, string?> { ["name"] = "World" });
        Assert.AreEqual("Hello, World!", result);
    }

    [TestMethod]
    public void Render_MultipleTokens_SubstitutesAll()
    {
        TemplateEngine engine = new();
        string result = engine.Render("{{greeting}}, {{name}}!", new Dictionary<string, string?> { ["greeting"] = "Hi", ["name"] = "Alice" });
        Assert.AreEqual("Hi, Alice!", result);
    }

    [TestMethod]
    public void Render_UnknownToken_LeftUnchanged()
    {
        TemplateEngine engine = new();
        string result = engine.Render("Hello, {{name}}!", new Dictionary<string, string?>());
        Assert.AreEqual("Hello, {{name}}!", result);
    }

    [TestMethod]
    public void Render_NullValue_SubstitutesEmptyString()
    {
        TemplateEngine engine = new();
        string result = engine.Render("Value: [{{value}}]", new Dictionary<string, string?> { ["value"] = null });
        Assert.AreEqual("Value: []", result);
    }

    [TestMethod]
    public void Render_TokenWithSurroundingWhitespace_StillMatches()
    {
        TemplateEngine engine = new();
        string result = engine.Render("Hi {{ name }}!", new Dictionary<string, string?> { ["name"] = "Bob" });
        Assert.AreEqual("Hi Bob!", result);
    }

    #endregion
}
