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
| [`Catharsis.UnitTests/`](Catharsis.UnitTests/README.md) | MSTest suite covering the public surface (~4,320 tests). |
| [`CatharsisBenchmarkSuite/`](CatharsisBenchmarkSuite/README.md) | BenchmarkDotNet suite measuring key types against BCL baselines. |

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
  `Trie`, `Graph`, `LruCache`, `Multimap`, `Interval`, a `KeyedCollection`-based
  `NamedItemCollection`, a freezable `ReadOnlyObservableView`, a fluent
  `FrozenLookupTable` builder, an insertion-ordered `OrderedNameValueCollection`,
  and more.
- **Buffers / HighPerformance / Advanced** — pooled buffers and builders,
  `SpanReader`/`SpanWriter`, UTF-8 span number parsing/formatting, pooled UTF-8
  strings and JSON documents, memory-backed channels, `System.IO.Pipelines`
  adapters and pipe-to-channel bridging, and zero-allocation helpers.
- **Linq** — async sequence operators, queryable builders, expression helpers,
  windowing/partitioning, memoization, top-N selection, and a small rule engine.
- **ComponentModel** — components, containers, dynamic type descriptors and
  proxies, an `ExpandoObject`-backed typed property bag, DTO records, lifecycle
  management, change tracking, type converters, async validation,
  property-path resolution, and snapshot/restore.
- **DataAnnotations** — additional validation attributes (`UniqueElements`,
  `MutuallyExclusive`, `RequiredIf`/`RequiredWhen`, `NotEqualTo`, `CollectionCount`,
  `FutureDate`/`PastDate`, `CreditCardLuhn`, `EnumRange`, …).
- **DesignPatterns / Patterns** — reusable Gang-of-Four and enterprise-pattern
  implementations, plus composed pipelines.
- **Text / Text.RegularExpressions / Extensions / Diagnostics / Resilience /
  Services** — grapheme-cluster-aware string handling and HTML/JS encoding,
  everyday regex helpers, `Meter`/`Activity`/`EventSource`-based observability,
  retry policies, and DI-friendly services (background queues, pooled-object
  policies, throttling).
- **Concurrency** — async synchronization primitives plus a fluent
  `System.Threading.Tasks.Dataflow` pipeline builder and a bounded
  `TransformBlock` factory tuned for CPU- vs. I/O-bound work.
- **Time / Security / Events / Configuration** — date/time ranges and
  holiday-aware business-day math, constant-time comparison, secure token
  generation, HMAC signing, AES-GCM envelope encryption, certificate-thumbprint
  pinning, a fluent `ClaimsPrincipal` builder, a lightweight event bus, and
  options/feature-flag validation.
- **Serialization / IO / Networking** — delimited/binary/XML record
  (de)serialization, atomic and compressing file writes, a directory-subtree-
  pruning file enumerator, a named-pipe request/response channel, retry-aware
  HTTP handling, ICMP health checks, a resilient `WebSocket` client, and
  server-sent-event streaming.
- **Numerics / Mathematics / Physics / Units / Geometry** — generic-math
  extensions, an exact `BigInteger`-backed `Rational` type, portable-SIMD
  aggregation, curated constants, SI units, metric prefixes, and conversion
  factors.

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
