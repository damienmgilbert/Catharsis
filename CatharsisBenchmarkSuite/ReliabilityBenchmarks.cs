using BenchmarkDotNet.Attributes;
using Catharsis.Events;
using Catharsis.Reliability;
using Catharsis.Resilience;

namespace CatharsisBenchmarkSuite;

///<summary>
///Measures the overhead <see cref="ResiliencyPipelineRegistry"/> adds on the happy path over calling the wrapped
///<see cref="RetryPolicy"/> directly — the cost of the name lookup, last-known-good caching, and circuit-state
///comparison on every call.
///</summary>
[MemoryDiagnoser]
public class ResiliencyPipelineRegistryBenchmarks
{
    const int CallCount = 10_000;
    static readonly RetryPolicy Policy = new RetryPolicy().MaxAttempts(3);
    static readonly ResiliencyPipelineRegistry Registry = CreateRegistry();

    static ResiliencyPipelineRegistry CreateRegistry()
    {
        ResiliencyPipelineRegistry registry = new(new EventBus());
        registry.Register("operation", Policy);
        return registry;
    }

    static int Compute(int value) => value * 2;

    [Benchmark(Baseline = true)]
    public async Task<int> DirectPolicy_ExecuteAsync()
    {
        int sum = 0;

        for (int i = 0; i < CallCount; i++)
        {
            int captured = i;
            sum += await Policy.ExecuteAsync(_ => Task.FromResult(Compute(captured)));
        }

        return sum;
    }

    [Benchmark]
    public async Task<int> ResiliencyPipelineRegistry_ExecuteAsync()
    {
        int sum = 0;

        for (int i = 0; i < CallCount; i++)
        {
            int captured = i;
            sum += await Registry.ExecuteAsync("operation", _ => Task.FromResult(Compute(captured)));
        }

        return sum;
    }
}
