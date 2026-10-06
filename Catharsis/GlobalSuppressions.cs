// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// The CancellationToken parameter is required by the async-iterator contract
// (it is bound by [EnumeratorCancellation] on the returned IAsyncEnumerable),
// so it cannot be removed even though these particular iterators never observe it.
[assembly: SuppressMessage(
           "Style",
           "IDE0060:Remove unused parameter",
           Justification = "Required by the async-iterator signature (EnumeratorCancellation).",
           Scope = "member",
           Target = "~M:Catharsis.Linq.AsyncEnumerableFactory.EmptyIterator``1(System.Threading.CancellationToken)~System.Collections.Generic.IAsyncEnumerable{``0}")]
[assembly: SuppressMessage(
           "Style",
           "IDE0060:Remove unused parameter",
           Justification = "Required by the async-iterator signature (EnumeratorCancellation).",
           Scope = "member",
           Target = "~M:Catharsis.Linq.AsyncEnumerableFactory.ReturnIterator``1(``0,System.Threading.CancellationToken)~System.Collections.Generic.IAsyncEnumerable{``0}")]
