using BenchmarkDotNet.Attributes;
using Catharsis.Dynamic;

namespace CatharsisBenchmarkSuite;

///<summary>
///Measures the overhead <see cref="FlexibleEntity"/> adds over a plain <see cref="Dictionary{TKey, TValue}"/> for
///simple set-then-get access — the cost of the underlying <c>ExpandoObject</c> storage, change tracking, and
///validation-pipeline lookup.
///</summary>
[MemoryDiagnoser]
public class FlexibleEntityBenchmarks
{
    const int CallCount = 10_000;

    [Benchmark(Baseline = true)]
    public int Dictionary_SetThenGet()
    {
        Dictionary<string, object?> storage = new();
        int sum = 0;

        for (int i = 0; i < CallCount; i++)
        {
            storage["Value"] = i;
            sum += (int)storage["Value"]!;
        }

        return sum;
    }

    [Benchmark]
    public int FlexibleEntity_SetThenGet()
    {
        FlexibleEntity entity = new();
        int sum = 0;

        for (int i = 0; i < CallCount; i++)
        {
            entity.Set("Value", i);
            sum += entity.Get<int>("Value");
        }

        return sum;
    }
}
