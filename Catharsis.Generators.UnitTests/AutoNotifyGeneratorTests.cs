using Microsoft.CodeAnalysis;
using System.ComponentModel;
using System.Reflection;

namespace Catharsis.Generators.UnitTests;

///<summary>
///Unit tests for <see cref="AutoNotifyGenerator"/>.
///</summary>
[TestClass]
public class AutoNotifyGeneratorTests
{
    static GeneratorHarness.Result Run(string source) => GeneratorHarness.Run(source, new AutoNotifyGenerator());

    static (object Instance, List<string> Raised) Create(Assembly assembly, string typeName)
    {
        object instance = Activator.CreateInstance(assembly.GetType(typeName)!)!;
        List<string> raised = [];
        ((INotifyPropertyChanged)instance).PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        return (instance, raised);
    }

    static void Set(object instance, string property, object? value) => instance.GetType().GetProperty(property)!.SetValue(instance, value);

    static object? Get(object instance, string property) => instance.GetType().GetProperty(property)!.GetValue(instance);

    const string PersonSource = @"
using Catharsis.Generators;
namespace Sample;
[AutoNotify]
public partial class Person
{
    private string _name = """";
    private int age;
    private string? _nickname;
}
";

    #region Output

    [TestMethod]
    public void Generates_PropertiesForFields_AndCompiles()
    {
        GeneratorHarness.Result result = Run(PersonSource);

        Assert.AreEqual(0, result.GeneratorDiagnostics.Length);
        Assert.AreEqual(0, result.CompilationErrors.Count(), string.Join(Environment.NewLine, result.CompilationErrors));

        Type person = result.Load().GetType("Sample.Person")!;
        Assert.IsNotNull(person.GetProperty("Name"));
        Assert.IsNotNull(person.GetProperty("Age"));
        Assert.IsNotNull(person.GetProperty("Nickname"));
        Assert.IsTrue(typeof(INotifyPropertyChanged).IsAssignableFrom(person));
    }

    [TestMethod]
    public void Setter_RaisesPropertyChangedOnlyWhenValueChanges()
    {
        (object person, List<string> raised) = Create(Run(PersonSource).Load(), "Sample.Person");

        Set(person, "Name", "Ada");
        Set(person, "Name", "Ada");
        Set(person, "Age", 36);

        CollectionAssert.AreEqual(new[] { "Name", "Age" }, raised);
        Assert.AreEqual("Ada", Get(person, "Name"));
        Assert.AreEqual(36, Get(person, "Age"));
    }

    [TestMethod]
    public void NullableReferenceField_KeepsNullabilityAndAcceptsNull()
    {
        (object person, List<string> raised) = Create(Run(PersonSource).Load(), "Sample.Person");

        Set(person, "Nickname", "A");
        Set(person, "Nickname", null);

        CollectionAssert.AreEqual(new[] { "Nickname", "Nickname" }, raised);
        Assert.IsNull(Get(person, "Nickname"));
    }

    [TestMethod]
    public void StaticAndConstFields_AreSkipped()
    {
        Type type = Run(@"
using Catharsis.Generators;
[AutoNotify]
public partial class Sample
{
    private static int _counter;
    private const int _limit = 3;
    private int _value;
}
").Load().GetType("Sample")!;

        Assert.IsNull(type.GetProperty("Counter"));
        Assert.IsNull(type.GetProperty("Limit"));
        Assert.IsNotNull(type.GetProperty("Value"));
    }

    [TestMethod]
    public void SealedClass_CompilesWithPrivateNotifier()
    {
        GeneratorHarness.Result result = Run(@"
using Catharsis.Generators;
[AutoNotify]
public sealed partial class Sealed { private int _x; }
");
        Assert.AreEqual(0, result.CompilationErrors.Count(), string.Join(Environment.NewLine, result.CompilationErrors));
        StringAssert.Contains(result.GeneratedText("Sealed.AutoNotify"), "private void OnPropertyChanged");
    }

    [TestMethod]
    public void UnsealedClass_AllowsDerivedOverride()
    {
        GeneratorHarness.Result result = Run(@"
using Catharsis.Generators;
[AutoNotify]
public partial class Base { private int _x; }
public class Derived : Base { protected override void OnPropertyChanged(string propertyName) { base.OnPropertyChanged(propertyName); } }
");
        Assert.AreEqual(0, result.CompilationErrors.Count(), string.Join(Environment.NewLine, result.CompilationErrors));
    }

    [TestMethod]
    public void GlobalNamespaceClass_HasNoNamespaceBlock()
    {
        string text = Run(@"
using Catharsis.Generators;
[AutoNotify]
public partial class Plain { private int _x; }
").GeneratedText("Plain.AutoNotify");

        Assert.IsFalse(text.Contains("namespace ", StringComparison.Ordinal));
    }

    #endregion

    #region Diagnostics

    [TestMethod]
    public void NonPartialClass_ReportsError()
    {
        GeneratorHarness.Result result = Run(@"
using Catharsis.Generators;
[AutoNotify]
public class NotPartial { private int _x; }
");
        Diagnostic diagnostic = result.GeneratorDiagnostics.Single();

        Assert.AreEqual("CATGEN001", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.IsFalse(result.GeneratedTrees.Any(static t => t.FilePath.Contains("NotPartial", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void NestedClass_ReportsUnsupported()
    {
        GeneratorHarness.Result result = Run(@"
using Catharsis.Generators;
public class Outer { [AutoNotify] public partial class Inner { private int _x; } }
");
        Assert.AreEqual("CATGEN002", result.GeneratorDiagnostics.Single().Id);
    }

    [TestMethod]
    public void GenericClass_ReportsUnsupported()
    {
        GeneratorHarness.Result result = Run(@"
using Catharsis.Generators;
[AutoNotify]
public partial class Box<T> { private T? _value; }
");
        Assert.AreEqual("CATGEN002", result.GeneratorDiagnostics.Single().Id);
    }

    [TestMethod]
    public void PropertyNameCollision_WarnsAndSkipsThatField()
    {
        GeneratorHarness.Result result = Run(@"
using Catharsis.Generators;
[AutoNotify]
public partial class Clash
{
    private int _size;
    private int _other;
    public int Size => 0;
}
");
        Diagnostic warning = result.GeneratorDiagnostics.Single();

        Assert.AreEqual("CATGEN003", warning.Id);
        Assert.AreEqual(DiagnosticSeverity.Warning, warning.Severity);
        Assert.AreEqual(0, result.CompilationErrors.Count(), string.Join(Environment.NewLine, result.CompilationErrors));
        Assert.IsNotNull(result.Load().GetType("Clash")!.GetProperty("Other"));
    }

    [TestMethod]
    public void FieldWhoseNameEqualsItsPropertyName_WarnsInsteadOfGeneratingBrokenCode()
    {
        GeneratorHarness.Result result = Run(@"
using Catharsis.Generators;
[AutoNotify]
public partial class Same { private int Value; }
");
        Assert.AreEqual("CATGEN003", result.GeneratorDiagnostics.Single().Id);
        Assert.AreEqual(0, result.CompilationErrors.Count(), string.Join(Environment.NewLine, result.CompilationErrors));
    }

    [TestMethod]
    public void UnmarkedClass_GeneratesNothingBeyondTheAttribute()
    {
        GeneratorHarness.Result result = Run("public partial class Unmarked { private int _x; }");

        Assert.AreEqual(1, result.GeneratedTrees.Length);
        Assert.AreEqual(0, result.GeneratorDiagnostics.Length);
    }

    #endregion
}
