using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Catharsis.Generators.UnitTests;

///<summary>
///Snapshot and incrementality tests for both generators.
///</summary>
[TestClass]
public class SnapshotTests
{
    const string EnumInput = @"
using Catharsis.Generators;
namespace Sample;
[EnumExtensions]
public enum Color { Red = 1, Green = 2, Blue = 4 }
";

    const string NotifyInput = @"
using Catharsis.Generators;
namespace Sample;
[AutoNotify]
public sealed partial class Person
{
    private string _name = """";
    private int age;
}
";

    [TestMethod]
    public void EnumExtensions_Output_MatchesSnapshot() { Snapshot.AssertMatches("Color.EnumExtensions.g.cs.txt", GeneratorHarness.Run(EnumInput, new EnumExtensionsGenerator()).GeneratedText("Sample.Color.EnumExtensions")); }

    [TestMethod]
    public void AutoNotify_Output_MatchesSnapshot() { Snapshot.AssertMatches("Person.AutoNotify.g.cs.txt", GeneratorHarness.Run(NotifyInput, new AutoNotifyGenerator()).GeneratedText("Sample.Person.AutoNotify")); }

    [TestMethod]
    public void EnumExtensions_UnchangedInput_IsFullyCachedOnSecondRun()
    {
        IncrementalStepRunReason[] reasons = GeneratorHarness.SecondRunReasons(EnumInput, new EnumExtensionsGenerator());

        Assert.IsTrue(reasons.Length > 0);
        Assert.IsTrue(reasons.All(static r => r is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged), string.Join(", ", reasons));
    }

    [TestMethod]
    public void AutoNotify_UnchangedInput_IsFullyCachedOnSecondRun()
    {
        IncrementalStepRunReason[] reasons = GeneratorHarness.SecondRunReasons(NotifyInput, new AutoNotifyGenerator());

        Assert.IsTrue(reasons.Length > 0);
        Assert.IsTrue(reasons.All(static r => r is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged), string.Join(", ", reasons));
    }

    [TestMethod]
    public void AutoNotify_DiagnosticModel_IsAlsoCached()
    {
        IncrementalStepRunReason[] reasons = GeneratorHarness.SecondRunReasons("using Catharsis.Generators;\n[AutoNotify]\npublic class NotPartial { private int _x; }", new AutoNotifyGenerator());

        Assert.IsTrue(reasons.All(static r => r is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged), string.Join(", ", reasons));
    }
}
