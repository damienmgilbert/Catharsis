# Catharsis

A broad, dependency-light utility library for **.NET 10**.

Catharsis gathers the general-purpose building blocks that projects tend to
reimplement — specialized collections and data structures, span/buffer
primitives, LINQ and sequence operators, a rich `System.ComponentModel` toolkit,
validation attributes, design-pattern scaffolding, and curated scientific
constants — into one cohesive, fully XML-documented package.

## Dependency policy

Catharsis builds **only** on:

- the **.NET 10 (`net10.0`) base class library**, and
- an opinionated set of first-party packages — nothing else:
  - **`CommunityToolkit.*`** — `Common`, `Diagnostics`, `HighPerformance`, `Mvvm`
  - **`Microsoft.Extensions.*`** — `DependencyInjection.Abstractions`,
    `Logging.Abstractions`, `Primitives`

This keeps the dependency graph shallow and the library easy to trust and adopt.

## Requirements

- **Target framework:** `net10.0`
- **SDK:** .NET 10 (the project uses `<LangVersion>preview</LangVersion>` and
  `<Nullable>enable</Nullable>`)

## Installation

```bash
dotnet add package Catharsis
```

Or add a project reference to `Catharsis.csproj`.

## Quick start

```csharp
using Catharsis.Collections;
using Catharsis.Extensions;

// Fluent collection extensions (all return the source for chaining)
var items = new List<int>();
items.AddRange([1, 2, 3, 4])
     .RemoveWhere(n => n % 2 == 0);   // removes 2 and 4

// A double-ended queue with fast operations at both ends
var deque = new Deque<string>();
deque.AddFirst("first");
deque.AddLast("last");

// Curated constants and conversions
double km = 3 * Catharsis.Units.UnitConversions.Length.MileToKilometer;
```

## Feature map

Everything lives under the `Catharsis.*` root namespace.

| Namespace | What it provides |
| --- | --- |
| `Catharsis.Collections` | `Deque`, `CircularBuffer`, `OrderedSet`, `BoundedCollection`, `EventCollection`, `HistoryStack`, `SortedList`, `TrackingCollection`, `ValidatingCollection`, `ReadOnlyListAdapter`, `WeightedList`, `MultiValueDictionary`, `BiDictionary`, `FrequencyCounter`. |
| `Catharsis.DataStructures` | `Graph`, `Trie`, `LruCache`, `Multimap`, `Interval`, `PriorityBucket`, `TreeNode`, `BloomFilter`, `DisjointSet`, `IntervalTree`, `MinMaxHeap`, `SkipList`. |
| `Catharsis.Buffers` | Pooled buffers and builders, `SpanReader`/`SpanWriter`, `BufferWriterStream`, `MemoryPoolManager`, growth strategies, sequence segments. |
| `Catharsis.HighPerformance` | `BitSpan`, `ImageBuffer`, `MemoryMappedSpanAccessor`, `PooledDictionary`, `PooledList`, `SpanTokenizer`, memory-owner extensions. |
| `Catharsis.Advanced` | Pooled UTF-8 strings and JSON documents, memory-backed channels, streaming sequence readers, sequence slices, a high-performance serializer. |
| `Catharsis.Immutable` | Immutable buffers and sequences with change notification. |
| `Catharsis.Diagnostics` | Safety-checked buffer helpers and stand-alone guards — `CheckedSpan`, `CheckedDictionary`, `SafeSequenceParser`, `ValidatedBufferWriter`, `AssertionScope`, `InvariantGuard`, `DebugOnlyValidator`. |
| `Catharsis.Linq` | Async sequence operators, queryable/expression builders, windowing, partitioning, lookups, memoization, top-N selection, Cartesian products, predicate combinators, and a small rule engine. |
| `Catharsis.Extensions` | Extension methods for arrays, lists, collections, dictionaries, sets, queues/stacks, and concurrent/immutable collections, plus span, enum, task, string, nullable, exception, and date/time helpers. |
| `Catharsis.ComponentModel` | Components, containers, dynamic type descriptors, DTO records, lifecycle management, change tracking, type converters, validation (including async), property-path resolution, snapshot/restore, and a lightweight event aggregator. |
| `Catharsis.DataAnnotations` | Additional validation attributes: `UniqueElements`, `MutuallyExclusive`, `RequiredIf`/`RequiredWhen`, `NotEqualTo`, `CollectionCount`, `DataTypePattern`, `Sorted`, `FutureDate`/`PastDate`, `CreditCardLuhn`, `EnumRange`, `NoWhitespace`/`Trimmed`, and more. |
| `Catharsis.DesignPatterns` | Behavioral, creational, and structural Gang-of-Four implementations, plus enterprise-pattern scaffolding — `Specification`, `UnitOfWork`, `Repository`, `NullObject`, `EventSourcing`, `CircuitBreaker`, `ObjectPool`. |
| `Catharsis.Patterns` | Composed pipelines — zero-allocation, DI buffer, high-throughput logging, and hybrids. |
| `Catharsis.Mvvm` | MVVM helpers built on `CommunityToolkit.Mvvm`: pooled/observable buffers and spans, `RelayCommandFactory`, `DebouncedObservableProperty`, and a validated buffer with `INotifyDataErrorInfo`. |
| `Catharsis.Services` | DI-friendly services and abstractions: buffer processing, pooled-object policies, a background work queue, and a rate-limited `ThrottledService`. |
| `Catharsis.Caching` | `MemoCache`, `TtlCache`, `CacheStampedeGuard`, `LayeredCache` (with the pluggable `ICacheLayer` interface). |
| `Catharsis.Concurrency` | Async-friendly synchronization primitives: `AsyncSemaphore`, `AsyncLazy`, `AsyncManualResetEvent`, `AsyncAutoResetEvent`, `AsyncCountdownEvent`, `AsyncBarrier`, `KeyedAsyncLock`, `SingleFlightExecutor`, `AsyncProducerConsumerQueue`, `TaskDebouncer`. |
| `Catharsis.Resilience` | Composable resilience policies implementing `IAsyncPolicy`: `RetryPolicy`, `CircuitBreaker`, `TimeoutPolicy`, `BulkheadPolicy`, `FallbackPolicy`, `PolicyWrap`, `RateLimiter`. |
| `Catharsis.Scheduling` | `CronExpression`, `RecurringTimer`, `JitteredDelayCalculator`. |
| `Catharsis.Randomization` | `WeightedRandomPicker`, `SeededRandomSequence`, `ShuffleExtensions`, `RandomStringGenerator`. |
| `Catharsis.Common` | `AsyncBufferLock`, `BufferComparer`, `ValueStopwatch`, weak-reference caching, `Disposable` helpers, `WeakEventHandler`, `IdGenerator`, `HashCodeCombiner`, and a `TimeProvider`-backed `Clock`/`ISystemClock`. |
| `Catharsis.Text.RegularExpressions` | Common regex patterns, a tokenizer, a replacer, match results, a glob-style `WildcardMatcher`, a `{{token}}` `TemplateEngine`, and a quote-aware `CsvLineTokenizer`. |
| `Catharsis.Time` | `DateOnlyRange`, `TimeOnlyRange`, holiday-aware `BusinessCalendar`, and a time-zone-aware `RecurringSchedule`. |
| `Catharsis.Security` | `ConstantTimeComparer`, `Base32Codec`, and a `RandomNumberGenerator`-backed `SecureRandomToken` generator. |
| `Catharsis.Events` | A lightweight async `EventBus`, plus `DomainEvent`/`DomainEventDispatcher` scaffolding for DDD-style aggregates. |
| `Catharsis.Configuration` | `OptionsValidator`/`IOptionsValidator`, a precedence-based `LayeredSettingsResolver`, and a percentage-rollout `FeatureFlagEvaluator`. |
| `Catharsis.Serialization` | `DelimitedRecordReader`/`DelimitedRecordWriter` (CSV/TSV POCO mapping), `SpanJsonWriter`, and an attribute-driven `BinaryRecordSerializer`. |
| `Catharsis.IO` | `AtomicFileWriter` (write-then-rename), a debounced `DirectoryWatcherDebounced`, and an allocation-light `LineReader`. |
| `Catharsis.Networking` | `RetryableHttpMessageHandler` (wires `Resilience.RetryPolicy` into `HttpClient`) and `EndpointHealthTracker`. |
| `Catharsis.Mathematics` | Math constants and symbols, including the Greek alphabet. |
| `Catharsis.Physics` | Fundamental physical constants and reference data. |
| `Catharsis.Units` | SI units, metric prefixes, and unit-conversion factors. |
| `Catharsis.Geometry` | Geometry formulas. |

## Documentation

Every public type and member ships with XML documentation, so IntelliSense and
generated API docs describe each API in place.

## License & authorship

Authored by **Damien M Gilbert**. Current version: **0.0.1**.
