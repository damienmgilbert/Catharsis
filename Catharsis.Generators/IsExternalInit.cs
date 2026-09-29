// Lets netstandard2.0 code use records and init-only setters; the compiler looks this type up by name.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
