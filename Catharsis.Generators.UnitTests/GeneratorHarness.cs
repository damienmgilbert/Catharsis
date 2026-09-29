using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;

namespace Catharsis.Generators.UnitTests;

///<summary>
///Runs a source generator over a snippet of C# in a real Roslyn compilation, so tests can inspect the diagnostics and
///generated text, confirm the result compiles, and even load and call the generated code.
///</summary>
internal static class GeneratorHarness
{
    static readonly ImmutableArray<MetadataReference> References = [.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator)
        .Select(static path => MetadataReference.CreateFromFile(path))];

    internal sealed record Result(Compilation Output, ImmutableArray<Diagnostic> GeneratorDiagnostics, ImmutableArray<SyntaxTree> GeneratedTrees)
    {
        public IEnumerable<Diagnostic> CompilationErrors => Output.GetDiagnostics().Where(static d => d.Severity == DiagnosticSeverity.Error);

        public string GeneratedText(string fileNameContains) => GeneratedTrees.Single(t => t.FilePath.Contains(fileNameContains, StringComparison.Ordinal)).ToString();

        public Assembly Load()
        {
            using MemoryStream stream = new();
            var emit = Output.Emit(stream);

            if(!emit.Success)
            {
                throw new InvalidOperationException("Generated code did not compile: " + string.Join(Environment.NewLine, emit.Diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error)));
            }

            stream.Position = 0;
            return new AssemblyLoadContext("generated", isCollectible: true).LoadFromStream(stream);
        }
    }

    ///<summary>
    ///Runs the generator twice over identical input and returns how every tracked output step was satisfied the second
    ///time. A generator whose pipeline models are value-equatable reports only <c>Cached</c> or <c>Unchanged</c> here,
    ///which is what lets Roslyn skip work while typing in the IDE.
    ///</summary>
    internal static IncrementalStepRunReason[] SecondRunReasons(string source, IIncrementalGenerator generator)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Preview);
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Incremental" + Guid.NewGuid().ToString("N"),
            [CSharpSyntaxTree.ParseText(source, parseOptions)],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [generator.AsSourceGenerator()],
            additionalTexts: null,
            parseOptions,
            optionsProvider: null,
            new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation);
        driver = driver.RunGenerators(compilation.Clone());

        return [.. driver.GetRunResult().Results[0].TrackedOutputSteps.SelectMany(static step => step.Value).SelectMany(static run => run.Outputs).Select(static output => output.Reason)];
    }

    internal static Result Run(string source, IIncrementalGenerator generator)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Preview);
        CSharpCompilation compilation = CSharpCompilation.Create(
            "GeneratedTests" + Guid.NewGuid().ToString("N"),
            [CSharpSyntaxTree.ParseText(source, parseOptions)],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create([generator.AsSourceGenerator()], parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out ImmutableArray<Diagnostic> diagnostics);

        return new Result(output, diagnostics, [.. driver.GetRunResult().GeneratedTrees]);
    }
}
