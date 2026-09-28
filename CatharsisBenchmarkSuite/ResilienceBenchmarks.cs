using BenchmarkDotNet.Attributes;
using Catharsis.Resilience;

namespace CatharsisBenchmarkSuite;

///<summary>
///Measures the overhead <see cref="RetryPolicy"/> adds on the happy path (the operation always succeeds on the
///first attempt), against calling the same delegate directly.
///</summary>
[MemoryDiagnoser]
public class RetryPolicyBenchmarks
{
    const int CallCount = 10_000;
    static readonly RetryPolicy Policy = new RetryPolicy().MaxAttempts(3);

    [Benchmark(Baseline = true)]
    public int DirectInvocation()
    {
        int sum = 0;

        for(int i = 0; i < CallCount; i++)
        {
            sum += Compute(i);
        }

        return sum;
    }

    [Benchmark]
    public int RetryPolicy_ExecuteOnSuccessPath()
    {
        int sum = 0;

        for(int i = 0; i < CallCount; i++)
        {
            int captured = i;
            sum += Policy.Execute(() => Compute(captured));
        }

        return sum;
    }

    static int Compute(int value) => value * 2;
}
