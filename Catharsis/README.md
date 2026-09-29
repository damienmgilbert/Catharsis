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
| `Catharsis.Collections` | `Deque`, `CircularBuffer`, `OrderedSet`, `BoundedCollection`, `EventCollection`, `HistoryStack`, `SortedList`, `TrackingCollection`, `ValidatingCollection`, `ReadOnlyListAdapter`, `WeightedList`, `MultiValueDictionary`, `BiDictionary`, `FrequencyCounter`, `NamedItemCollection`, `ReadOnlyObservableView`, `FrozenLookupTable`, `OrderedNameValueCollection`. |
| `Catharsis.DataStructures` | `Graph`, `Trie`, `LruCache`, `Multimap`, `Interval`, `PriorityBucket`, `TreeNode`, `BloomFilter`, `DisjointSet`, `IntervalTree`, `MinMaxHeap`, `SkipList`. |
| `Catharsis.Buffers` | Pooled buffers and builders, `SpanReader`/`SpanWriter`, `BufferWriterStream`, `MemoryPoolManager`, growth strategies, sequence segments, and `Utf8Parser`/`Utf8Formatter`-based `Utf8SpanNumberParser`/`Utf8SpanNumberFormatter`. |
| `Catharsis.HighPerformance` | `BitSpan`, `ImageBuffer`, `MemoryMappedSpanAccessor`, `PooledDictionary`, `PooledList`, `SpanTokenizer`, memory-owner extensions. |
| `Catharsis.Advanced` | Pooled UTF-8 strings and JSON documents, memory-backed channels, streaming sequence readers, sequence slices, a high-performance serializer, a `PipeReader`/`PipeWriter` `PipeStreamAdapter`, and a `Pipe`-to-`MemoryBackedChannel` `PipelineChannelBridge`. |
| `Catharsis.Immutable` | Immutable buffers and sequences with change notification. |
| `Catharsis.Diagnostics` | Safety-checked buffer helpers and stand-alone guards — `CheckedSpan`, `CheckedDictionary`, `SafeSequenceParser`, `ValidatedBufferWriter`, `AssertionScope`, `InvariantGuard`, `DebugOnlyValidator` — plus observability primitives: a `Meter`-backed `MetricsRecorder`, an `Activity`-scoping `ActivityScope`, and an `EventSource`-based `EventSourceLogger`. |
| `Catharsis.Linq` | Async sequence operators, queryable/expression builders, windowing, partitioning, lookups, memoization, top-N selection, Cartesian products, predicate combinators, and a small rule engine. |
| `Catharsis.Extensions` | Extension methods for arrays, lists, collections, dictionaries, sets, queues/stacks, and concurrent/immutable collections, plus span, enum, task, string, nullable, exception, and date/time helpers. |
| `Catharsis.ComponentModel` | Components, containers, dynamic type descriptors, a `DynamicObject`-based `DynamicComponentProxy`, an `ExpandoObject`-backed `ExpandoBackedBag`, DTO records, lifecycle management, change tracking, type converters, validation (including async), property-path resolution, snapshot/restore, and a lightweight event aggregator. |
| `Catharsis.DataAnnotations` | Additional validation attributes: `UniqueElements`, `MutuallyExclusive`, `RequiredIf`/`RequiredWhen`, `NotEqualTo`, `CollectionCount`, `DataTypePattern`, `Sorted`, `FutureDate`/`PastDate`, `CreditCardLuhn`, `EnumRange`, `NoWhitespace`/`Trimmed`, and more. |
| `Catharsis.DesignPatterns` | Behavioral, creational, and structural Gang-of-Four implementations, plus enterprise-pattern scaffolding — `Specification`, `UnitOfWork`, `Repository`, `NullObject`, `EventSourcing`, `CircuitBreaker`, `ObjectPool`. |
| `Catharsis.Patterns` | Composed pipelines — zero-allocation, DI buffer, high-throughput logging, and hybrids. |
| `Catharsis.Mvvm` | MVVM helpers built on `CommunityToolkit.Mvvm`: pooled/observable buffers and spans, `RelayCommandFactory`, `DebouncedObservableProperty`, and a validated buffer with `INotifyDataErrorInfo`. |
| `Catharsis.Services` | DI-friendly services and abstractions: buffer processing, pooled-object policies, a background work queue, and a rate-limited `ThrottledService`. |
| `Catharsis.Caching` | `MemoCache`, `TtlCache`, `CacheStampedeGuard`, `LayeredCache` (with the pluggable `ICacheLayer` interface). |
| `Catharsis.Concurrency` | Async-friendly synchronization primitives: `AsyncSemaphore`, `AsyncLazy`, `AsyncManualResetEvent`, `AsyncAutoResetEvent`, `AsyncCountdownEvent`, `AsyncBarrier`, `KeyedAsyncLock`, `SingleFlightExecutor`, `AsyncProducerConsumerQueue`, `TaskDebouncer` — plus a fluent `DataflowPipelineBuilder` (chaining `BufferBlock`/`TransformBlock`/`ActionBlock` with completion propagation) and a `BoundedTransformBlockFactory` tuned for CPU- vs. I/O-bound work. |
| `Catharsis.Resilience` | Composable resilience policies implementing `IAsyncPolicy`: `RetryPolicy`, `CircuitBreaker`, `TimeoutPolicy`, `BulkheadPolicy`, `FallbackPolicy`, `PolicyWrap`, `RateLimiter`. |
| `Catharsis.Scheduling` | `CronExpression`, `RecurringTimer`, `JitteredDelayCalculator`. |
| `Catharsis.Randomization` | `WeightedRandomPicker`, `SeededRandomSequence`, `ShuffleExtensions`, `RandomStringGenerator`. |
| `Catharsis.Common` | `AsyncBufferLock`, `BufferComparer`, `ValueStopwatch`, weak-reference caching, `Disposable` helpers, `WeakEventHandler`, `IdGenerator`, `HashCodeCombiner`, and a `TimeProvider`-backed `Clock`/`ISystemClock`. |
| `Catharsis.Text.RegularExpressions` | Common regex patterns, a tokenizer, a replacer, match results, a glob-style `WildcardMatcher`, a `{{token}}` `TemplateEngine`, and a quote-aware `CsvLineTokenizer`. |
| `Catharsis.Text` | Non-regex text handling: a `StringInfo`-based `GraphemeClusterEnumerator` and `TextElementTruncator` (grapheme-cluster-correct, not UTF-16-code-unit-correct), plus `SafeHtmlEncoder`/`SafeJsEncoder` over `System.Text.Encodings.Web`. |
| `Catharsis.Time` | `DateOnlyRange`, `TimeOnlyRange`, holiday-aware `BusinessCalendar`, and a time-zone-aware `RecurringSchedule`. |
| `Catharsis.Security` | `ConstantTimeComparer`, `Base32Codec`, a `RandomNumberGenerator`-backed `SecureRandomToken` generator, an `HMACSHA256`-based `HmacSigner`, an `AesGcm`-based `AesGcmEnvelope` (self-framing nonce + ciphertext + tag), an `X509Certificate2` `CertificateThumbprintValidator` for cert pinning, and a fluent `ClaimsPrincipalBuilder`. |
| `Catharsis.Events` | A lightweight async `EventBus`, plus `DomainEvent`/`DomainEventDispatcher` scaffolding for DDD-style aggregates. |
| `Catharsis.Configuration` | `OptionsValidator`/`IOptionsValidator`, a precedence-based `LayeredSettingsResolver`, and a percentage-rollout `FeatureFlagEvaluator`. |
| `Catharsis.Serialization` | `DelimitedRecordReader`/`DelimitedRecordWriter` (CSV/TSV POCO mapping), `SpanJsonWriter`, and an attribute-driven `BinaryRecordSerializer`. |
| `Catharsis.IO` | `AtomicFileWriter` (write-then-rename), a debounced `DirectoryWatcherDebounced`, an allocation-light `LineReader`, a `GZip`/`Brotli` `CompressingFileWriter`, a subtree-pruning `FilteredFileEnumerator` (`System.IO.Enumeration`), and a `NamedPipeRequestChannel` request/response IPC wrapper. |
| `Catharsis.Networking` | `RetryableHttpMessageHandler` (wires `Resilience.RetryPolicy` into `HttpClient`), `EndpointHealthTracker`, an ICMP `PingSweepHealthTracker`, an `HttpClientMetricsListener` (taps the BCL's built-in `"System.Net.Http"` `Meter`), `TcpEchoServer`/`TcpLineClient`, a `RetryPolicy`-driven `ResilientWebSocketClient`, and `SseEventStreamWriter`/`SseClientSubscriber` (`System.Net.ServerSentEvents`). |
| `Catharsis.Xml` | `XDocument`/`XElement`-based `XmlRecordReader`/`XmlRecordWriter` (POCO mapping, mirroring `Serialization`'s CSV/binary readers), and an `XElementPathResolver` for simplified path lookups (e.g. `"Order/Customer/@id"`). |
| `Catharsis.Numerics` | Generic-math extensions (`Clamp`, `Lerp`, `NormalizeTo`, `IsBetween` over any `INumber<T>`), an exact `BigInteger`-backed `Rational` type, and a portable-SIMD `SimdAggregator` (`Vector<T>` sum/min/max/dot-product with automatic scalar fallback). |
| `Catharsis.Mathematics` | Math constants and symbols, including the Greek alphabet. |
| `Catharsis.Physics` | Fundamental physical constants and reference data. |
| `Catharsis.Units` | SI units, metric prefixes, and unit-conversion factors. |
| `Catharsis.Geometry` | Geometry formulas. |
| `Catharsis.RuleEngine` | `ManagedRuleEngine<T>` — a stateful, observable orchestrator around `Linq`'s `Rule<T>`/`RuleSet<T>`/`RuleEvaluator`: a live, runtime-updatable registry of named rule sets, per-rule-set evaluation metrics via a `MetricsRecorder`, per-match notifications via an `EventBus`, and optional `FeatureFlagEvaluator`-gated gradual rollout of a rule set. |
| `Catharsis.Monitoring` | `NetworkMonitor` — a background service that sweeps a fixed set of endpoints on an interval via `Networking.PingSweepHealthTracker`, drives a per-endpoint `Resilience.CircuitBreaker` from each sweep, and publishes an `EndpointStatusChangedEvent` on an `EventBus` whenever an endpoint's `EndpointStatus` (Healthy/Degraded/Down) changes. |
| `Catharsis.Reliability` | `ResiliencyPipelineRegistry` — a name-keyed registry of `Resilience.IAsyncPolicy` pipelines with a stale-value fallback (serves the last successful result if every policy in a named pipeline fails) and `CircuitStateChangedEvent` notifications on an `EventBus` when an associated `CircuitBreaker` trips or recovers. |
| `Catharsis.Dynamic` | `FlexibleEntity` — a schema-less, self-validating, undoable dynamic object combining an `ExpandoBackedBag<T>`, per-property `ValidationPipeline` rules, `ChangeSet`-based undo/redo, dotted nested-path access (`"Address.City"`), snapshot/restore, and `DynamicTypeDescriptor`/`DynamicPropertyDescriptor`-based exposure for WinForms/WPF-style binding. |
| `Catharsis.Workflow` | `StepWorkflowOrchestrator` — executes named async steps in dependency order using `ComponentModel.Lifecycle`'s `ComponentGraph`/`ComponentGraphBuilder`, running independent steps concurrently in dependency-depth waves, with optional per-step retry via an `IAsyncPolicy` and `WorkflowStepEvent` progress notifications on an `EventBus`; a faulted step fails only its (transitive) dependents. |
| `Catharsis.Operators` | Value types with operator overloading and indexers: `Money` (currency-checked arithmetic, comparison, `Allocate`), `Vector2D`/`Vector3D` (dot `*` and cross `^` products), `Percentage`, `SemanticVersion` (parse, semver ordering, bumping) and an immutable `Matrix` with `+`, `-`, `*` and a `[row, column]` indexer. |
| `Catharsis.Events` (additions) | `AsyncEvent<TArgs>` — a multicast event with async handlers, disposable subscriptions, filters and `+=`/`-=`; `WeakEvent<TArgs>` — holds subscribers weakly; `EventThrottler<T>` — leading-edge throttle on a `TimeProvider`; `DelegateChain<T>` — an immutable `Func<T, T>` pipeline; and the `AsyncEventHandler<TArgs>` and `EventFilter<T>` delegates. |
| `Catharsis.Contracts` | Attribute-driven wiring: `ServiceAttribute`, `DecoratorForAttribute` and `PluginAttribute`; `AttributeServiceScanner` and `PluginLoader` (reflection discovery); `TypeMetadataReader` (cached attribute reads); and `IServiceCollection` extensions `AddAttributedServices`, `AddDecorator` and `AddFactory<T>`. |
| `Catharsis.Generics` | Constraint-driven generic types: `Result<T, TError>`, `Maybe<T>`, `NumericRange<T>` (`INumber<T>`), `TypedRegistry<TKey, TValue>` (`notnull`), `EnumMap<TEnum, TValue>` (`struct, Enum`), an expression-tree compiled `ExpressionMapper<TSource, TDest>` and an `ExpressionCache` of compiled delegates and property getters. |
| `Catharsis.Patterns.Composed` | Usable pattern implementations built on the rest of the library: `TimeProviderSystemClock`/`SystemClockTimeProvider` (adapters), `LoggingPolicyDecorator`/`MetricsPolicyDecorator` (decorators over `IAsyncPolicy`), `IPricingStrategy<T>` with flat, tiered and discount strategies and a `PricingContext<T>` (strategy), `KeyedFactory<TKey, T>`/`PolicyFactory` (factories) and `TreeIterator`/`RoundRobinIterator` (iterators). |
| `Catharsis.Domain` | Core object-oriented building blocks: `Entity<TId>`, `ValueObject`, `AggregateRoot<TId>` (collects `DomainEvent`s), a polymorphic `Shape` hierarchy (`Circle`, `Rectangle`, `Square`, `Triangle`) and a sample `Order` (a `partial` class) with `OrderLine` and an all-or-nothing `Inventory`. |

## Documentation

Every public type and member ships with XML documentation, so IntelliSense and
generated API docs describe each API in place.

## License & authorship

Authored by **Damien M Gilbert**. Current version: **0.0.1**.
