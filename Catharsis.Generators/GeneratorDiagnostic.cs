using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Catharsis.Generators
{
    /// <summary>
    /// A value-equatable description of a diagnostic to report. Generator pipeline stages must produce values that
    /// compare equal when nothing changed so Roslyn can cache them, and <see cref="Diagnostic"/> and
    /// <see cref="Location"/> do not, so the diagnostic is carried as plain data and built at the output stage.
    /// </summary>
    internal sealed record GeneratorDiagnostic(string Id, string Argument, string FilePath, TextSpan Span, LinePositionSpan LineSpan)
    {
        public Diagnostic ToDiagnostic(DiagnosticDescriptor descriptor) =>
            Diagnostic.Create(descriptor, Location.Create(FilePath, Span, LineSpan), Argument);

        public static GeneratorDiagnostic Create(string id, string argument, Location location)
        {
            FileLinePositionSpan mapped = location.GetLineSpan();

            return new GeneratorDiagnostic(id, argument, mapped.Path, location.SourceSpan, mapped.Span);
        }
    }
}
