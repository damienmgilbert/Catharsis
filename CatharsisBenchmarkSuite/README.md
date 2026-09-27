# CatharsisBenchmarkSuite

A [BenchmarkDotNet](https://benchmarkdotnet.org/) suite that measures selected
[Catharsis](../Catharsis/README.md) types against the closest BCL (or
in-library) baseline, so performance characteristics and regressions can be
tracked with real numbers instead of intuition.

## Running the benchmarks

Benchmarks must be run in **Release** configuration — BenchmarkDotNet will
refuse a `Debug` build.

```bash
dotnet run -c Release --project CatharsisBenchmarkSuite
```

This launches BenchmarkDotNet's interactive menu for picking which benchmark
class(es) to run. Results — including a memory-allocation report for classes
marked `[MemoryDiagnoser]` — are written under
`CatharsisBenchmarkSuite/BenchmarkDotNet.Artifacts/`.

To run every benchmark without the interactive prompt:

```bash
dotnet run -c Release --project CatharsisBenchmarkSuite -- --filter '*'
```

Requires the **.NET 10 SDK**.

## What's benchmarked

Each file targets one area of the library and pits a Catharsis type against
the nearest baseline, using `[Benchmark(Baseline = true)]` on the reference
implementation:

| File | Compares |
| --- | --- |
| `BuffersBenchmarks.cs` | `PooledBuffer<T>` vs. `List<T>`; `PooledStringBuilder` vs. `StringBuilder` |
| `CachingBenchmarks.cs` | `MemoCache<TKey, TValue>` vs. `ConcurrentDictionary<TKey, TValue>` |
| `CollectionsBenchmarks.cs` | `Deque<T>` / `CircularBuffer<T>` vs. the closest BCL equivalents |
| `DataStructuresBenchmarks.cs` | `LruCache<TKey, TValue>` vs. `Dictionary<TKey, TValue>`; `MinMaxHeap<T>` vs. a naive `List<T>` baseline |
| `RandomizationBenchmarks.cs` | `WeightedRandomPicker<T>` vs. `WeightedList<T>` |
| `ResilienceBenchmarks.cs` | `RetryPolicy`'s happy-path overhead vs. calling the delegate directly |

## Contributing a benchmark

1. Add a new `<Area>Benchmarks.cs` file (or a class inside an existing one)
   following the pattern above: an XML `<summary>` explaining what's being
   compared and why, a `[Benchmark(Baseline = true)]` method for the reference
   implementation, and one or more `[Benchmark]` methods for the Catharsis type.
2. Add `[MemoryDiagnoser]` to the class when allocations are part of the story.
3. Run it in `Release` and sanity-check the results before opening a PR.

## License & authorship

Authored by **Damien M Gilbert**.
