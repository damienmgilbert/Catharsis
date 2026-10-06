using BenchmarkDotNet.Attributes;
using Catharsis.Collections;
using Catharsis.Randomization;

namespace CatharsisBenchmarkSuite;

///<summary>
///Compares <see cref="WeightedRandomPicker{T}"/> (linear scan over cumulative weights) against
///<see cref="WeightedList{T}"/> (binary search over a cached cumulative-weight table) for repeated picks from the
///same fixed set of weighted items.
///</summary>
[MemoryDiagnoser]
public class WeightedPickBenchmarks
{
    const int ItemCount = 1_000;
    const int PickCount = 5_000;

    WeightedRandomPicker<int> _picker = null!;
    WeightedList<int> _list = null!;
    Random _random = null!;

    [GlobalSetup]
    public void Setup()
    {
        _picker = new WeightedRandomPicker<int>();
        _list = [];

        for (int i = 0; i < ItemCount; i++)
        {
            _picker.Add(i, i + 1);
            _list.Add(i, i + 1);
        }

        _random = new Random(11);
    }

    [Benchmark(Baseline = true)]
    public int WeightedRandomPicker_Pick()
    {
        int sum = 0;

        for (int i = 0; i < PickCount; i++)
        {
            sum += _picker.Pick(_random);
        }

        return sum;
    }

    [Benchmark]
    public int WeightedList_PickRandom()
    {
        int sum = 0;

        for (int i = 0; i < PickCount; i++)
        {
            sum += _list.PickRandom(_random);
        }

        return sum;
    }
}
