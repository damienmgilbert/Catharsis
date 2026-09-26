# Catharsis

A broad, dependency-light utility library for **.NET 10**. Catharsis collects the
building blocks that most projects end up rewriting — specialized collections and
data structures, span/buffer primitives, LINQ and sequence operators, a rich
`System.ComponentModel` toolkit, validation attributes, design-pattern scaffolding,
and curated scientific constants — into one cohesive, fully XML-documented package.

## Design philosophy

Catharsis deliberately limits what it depends on. Every type is built **only** on:

- the **.NET 10 (`net10.0`) base class library**, and
- a small, opinionated set of first-party packages:
  - **`CommunityToolkit.*`** — `Common`, `Diagnostics`, `HighPerformance`, `Mvvm`
  - **`Microsoft.Extensions.*`** — `DependencyInjection.Abstractions`,
    `Logging.Abstractions`, `Primitives`

No other third-party dependencies are taken. This keeps the dependency graph
shallow, the trust surface small, and the library easy to adopt in any .NET 10
application or library.

## Repository layout

| Project | Description |
| --- | --- |
| [`Catharsis/`](Catharsis/README.md) | The library itself — the shippable NuGet package. |
| [`Catharsis.UnitTests/`](Catharsis.UnitTests/README.md) | MSTest suite covering the public surface (~2,800 tests). |

The solution is defined by [`Catharsis.slnx`](Catharsis.slnx) (the XML-based
solution format).

## Quick start

### Build and test

```bash
dotnet build Catharsis.slnx
dotnet test Catharsis.slnx
```

Requires the **.NET 10 SDK** (the projects use `<LangVersion>preview</LangVersion>`).

### Use the library

Add a project or package reference to `Catharsis`, then pull in the namespace you
need:

```csharp
using Catharsis.Collections;
using Catharsis.Extensions;

// A double-ended queue
var deque = new Deque<int>();
deque.AddFirst(1);
deque.AddLast(2);

// Fluent collection extensions
var list = new List<string>();
list.AddRange(["a", "b", "c"])
    .ReplaceAll("b", "B");
```

See the [library README](Catharsis/README.md) for the full feature map.

## What's inside

Catharsis is organized into focused namespaces under the `Catharsis.*` root:

- **Collections / DataStructures** — `Deque`, `CircularBuffer`, `OrderedSet`,
  `Trie`, `Graph`, `LruCache`, `Multimap`, `Interval`, and more.
- **Buffers / HighPerformance / Advanced** — pooled buffers and builders,
  `SpanReader`/`SpanWriter`, pooled UTF-8 strings and JSON documents,
  memory-backed channels, and zero-allocation helpers.
- **Linq** — async sequence operators, queryable builders, expression helpers,
  windowing/partitioning, and a small rule engine.
- **ComponentModel** — components, containers, dynamic type descriptors, DTO
  records, lifecycle management, change tracking, type converters, and validation.
- **DataAnnotations** — additional validation attributes (`UniqueElements`,
  `MutuallyExclusive`, `RequiredIf`, `NotEqualTo`, `CollectionCount`, …).
- **DesignPatterns / Patterns** — reusable Gang-of-Four implementations and
  composed pipelines.
- **Text.RegularExpressions / Extensions / Diagnostics / Resilience / Services** —
  everyday helpers, safety-checked buffer parsing, retry policies, and
  DI-friendly services.
- **Mathematics / Physics / Units / Geometry** — curated constants, SI units,
  metric prefixes, and conversion factors.

## Contributing

1. Fork and branch from `master`.
2. Keep the dependency policy intact — new code may only rely on the .NET 10 BCL
   and the approved `CommunityToolkit.*` / `Microsoft.Extensions.*` packages.
3. Every public type and member gets XML documentation.
4. Add or update tests in `Catharsis.UnitTests`; run `dotnet test` before opening
   a PR. Analyzers run in-build (`EnableNETAnalyzers`, `AnalysisMode=Recommended`,
   `EnforceCodeStyleInBuild`), so builds must be warning-clean.

## License & authorship

Authored by **Damien M Gilbert**. Current version: **0.0.1**.
