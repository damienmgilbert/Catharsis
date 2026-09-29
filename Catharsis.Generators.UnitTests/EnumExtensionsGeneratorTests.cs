using System.Reflection;

namespace Catharsis.Generators.UnitTests;

///<summary>
///Unit tests for <see cref="EnumExtensionsGenerator"/>.
///</summary>
[TestClass]
public class EnumExtensionsGeneratorTests
{
    static GeneratorHarness.Result Run(string source) => GeneratorHarness.Run(source, new EnumExtensionsGenerator());

    static MethodInfo Method(Assembly assembly, string type, string name) => assembly.GetType(type)!.GetMethod(name)!;

    const string ColorSource = @"
using Catharsis.Generators;
namespace Sample;
[EnumExtensions]
public enum Color { Red = 1, Green = 2, Blue = 4 }
";

    #region Output

    [TestMethod]
    public void Generates_ExtensionsClass_ThatCompiles()
    {
        GeneratorHarness.Result result = Run(ColorSource);

        Assert.AreEqual(0, result.GeneratorDiagnostics.Length);
        Assert.AreEqual(0, result.CompilationErrors.Count(), string.Join(Environment.NewLine, result.CompilationErrors));
        StringAssert.Contains(result.GeneratedText("Sample.Color.EnumExtensions"), "public static class ColorExtensions");
    }

    [TestMethod]
    public void ToStringFast_ReturnsNames_AndNumberForUndefinedValues()
    {
        Assembly assembly = Run(ColorSource).Load();
        MethodInfo toString = Method(assembly, "Sample.ColorExtensions", "ToStringFast");
        Type color = assembly.GetType("Sample.Color")!;

        Assert.AreEqual("Green", toString.Invoke(null, [Enum.ToObject(color, 2)]));
        Assert.AreEqual("99", toString.Invoke(null, [Enum.ToObject(color, 99)]));
    }

    [TestMethod]
    public void TryParseFast_AcceptsExactNamesOnly()
    {
        Assembly assembly = Run(ColorSource).Load();
        MethodInfo tryParse = Method(assembly, "Sample.ColorExtensions", "TryParseFast");

        object?[] hit = ["Blue", null];
        Assert.IsTrue((bool)tryParse.Invoke(null, hit)!);
        Assert.AreEqual(4, Convert.ToInt32(hit[1], System.Globalization.CultureInfo.InvariantCulture));

        Assert.IsFalse((bool)tryParse.Invoke(null, ["blue", null])!);
        Assert.IsFalse((bool)tryParse.Invoke(null, [null, null])!);
    }

    [TestMethod]
    public void IsDefinedFast_TrueOnlyForNamedMembers()
    {
        Assembly assembly = Run(ColorSource).Load();
        MethodInfo isDefined = Method(assembly, "Sample.ColorExtensions", "IsDefinedFast");
        Type color = assembly.GetType("Sample.Color")!;

        Assert.IsTrue((bool)isDefined.Invoke(null, [Enum.ToObject(color, 1)])!);
        Assert.IsFalse((bool)isDefined.Invoke(null, [Enum.ToObject(color, 3)])!);
    }

    #endregion

    #region Edge cases

    [TestMethod]
    public void AliasedValues_CompileAndUseFirstNameForToString()
    {
        GeneratorHarness.Result result = Run(@"
using Catharsis.Generators;
[EnumExtensions]
public enum Level { Low = 1, Minimum = 1, High = 2 }
");
        Assert.AreEqual(0, result.CompilationErrors.Count(), string.Join(Environment.NewLine, result.CompilationErrors));

        Assembly assembly = result.Load();
        object low = Enum.ToObject(assembly.GetType("Level")!, 1);

        Assert.AreEqual("Low", Method(assembly, "LevelExtensions", "ToStringFast").Invoke(null, [low]));

        object?[] parsed = ["Minimum", null];
        Assert.IsTrue((bool)Method(assembly, "LevelExtensions", "TryParseFast").Invoke(null, parsed)!);
        Assert.AreEqual(1, Convert.ToInt32(parsed[1], System.Globalization.CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void KeywordMemberNames_AreEscaped()
    {
        GeneratorHarness.Result result = Run(@"
using Catharsis.Generators;
[EnumExtensions]
public enum Keywords { @default, @new }
");
        Assert.AreEqual(0, result.CompilationErrors.Count(), string.Join(Environment.NewLine, result.CompilationErrors));
    }

    [TestMethod]
    public void InternalEnum_ProducesInternalExtensions()
    {
        GeneratorHarness.Result result = Run(@"
using Catharsis.Generators;
[EnumExtensions]
enum Hidden { A }
");
        StringAssert.Contains(result.GeneratedText("Hidden.EnumExtensions"), "internal static class HiddenExtensions");
        Assert.AreEqual(0, result.CompilationErrors.Count());
    }

    [TestMethod]
    public void GlobalNamespaceEnum_HasNoNamespaceBlock()
    {
        string text = Run(@"
using Catharsis.Generators;
[EnumExtensions]
public enum Plain { A }
").GeneratedText("Plain.EnumExtensions");

        Assert.IsFalse(text.Contains("namespace ", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FlagsEnum_ToStringFastFallsBackToEnumToStringForCombinations()
    {
        Assembly assembly = Run(@"
using Catharsis.Generators;
[System.Flags]
[EnumExtensions]
public enum Perms { None = 0, Read = 1, Write = 2 }
").Load();
        object both = Enum.ToObject(assembly.GetType("Perms")!, 3);

        Assert.AreEqual("Read, Write", Method(assembly, "PermsExtensions", "ToStringFast").Invoke(null, [both]));
    }

    [TestMethod]
    public void NestedEnum_ReportsDiagnosticAndGeneratesNothing()
    {
        GeneratorHarness.Result result = Run(@"
using Catharsis.Generators;
public class Outer { [EnumExtensions] public enum Inner { A } }
");
        Assert.AreEqual("CATGEN004", result.GeneratorDiagnostics.Single().Id);
        Assert.IsFalse(result.GeneratedTrees.Any(static t => t.FilePath.Contains("Inner", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void UnmarkedEnum_GeneratesNothingBeyondTheAttribute()
    {
        GeneratorHarness.Result result = Run("public enum Unmarked { A }");

        Assert.AreEqual(1, result.GeneratedTrees.Length);
        StringAssert.Contains(result.GeneratedTrees[0].FilePath, "EnumExtensionsAttribute");
    }

    #endregion
}
