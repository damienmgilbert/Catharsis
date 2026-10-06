using BenchmarkDotNet.Attributes;
using Catharsis.DataStructures;

namespace CatharsisBenchmarkSuite;

///<summary>
///Compares <see cref="LruCache{TKey, TValue}"/> against a plain <see cref="Dictionary{TKey, TValue}"/> for a
///bounded-key add/lookup churn workload (the dictionary never evicts, so this also shows the eviction-bookkeeping
///cost).
///</summary>
[MemoryDiagnoser]
public class LruCacheBenchmarks
{
    const int OperationCount = 5_000;
    const int KeySpace = 200;

    [Benchmark(Baseline = true)]
    public int Dictionary_AddAndLookup()
    {
        Dictionary<int, int> dictionary = [];

        for (int i = 0; i < OperationCount; i++)
        {
            dictionary[i % KeySpace] = i;
            dictionary.TryGetValue(i % KeySpace, out _);
        }

        return dictionary.Count;
    }

    [Benchmark]
    public int LruCache_AddAndLookup()
    {
        LruCache<int, int> cache = new(KeySpace);

        for (int i = 0; i < OperationCount; i++)
        {
            cache.AddOrUpdate(i % KeySpace, i);
            cache.TryGetValue(i % KeySpace, out _);
        }

        return cache.Count;
    }
}

///<summary>
///Compares <see cref="MinMaxHeap{T}"/>'s O(log n) double-ended extraction against a naive
///<see cref="List{T}"/> + linear <c>Min()</c>/<c>Max()</c>/<c>Remove()</c> baseline, draining every element from
///both ends.
///</summary>
[MemoryDiagnoser]
public class MinMaxHeapBenchmarks
{
    const int ItemCount = 2_000;
    int[] _data = [];

    [GlobalSetup]
    public void Setup()
    {
        Random random = new(42);
        _data = new int[ItemCount];

        for (int i = 0; i < ItemCount; i++)
        {
            _data[i] = random.Next();
        }
    }

    [Benchmark(Baseline = true)]
    public (int Min, int Max) List_LinearMinMaxExtraction()
    {
        List<int> list = [.. _data];
        int min = 0;
        int max = 0;

        while (list.Count > 0)
        {
            min = list.Min();
            list.Remove(min);

            if (list.Count > 0)
            {
                max = list.Max();
                list.Remove(max);
            }
        }

        return (min, max);
    }

    [Benchmark]
    public (int Min, int Max) MinMaxHeap_ExtractBoth()
    {
        MinMaxHeap<int> heap = new(_data);
        int min = 0;
        int max = 0;

        while (heap.Count > 0)
        {
            min = heap.ExtractMin();

            if (heap.Count > 0)
            {
                max = heap.ExtractMax();
            }
        }

        return (min, max);
    }
}

///<summary>
///Compares <see cref="SkipList{T}"/> against <see cref="SortedSet{T}"/> for an ordered insert-then-enumerate
///workload over unique values.
///</summary>
[MemoryDiagnoser]
public class SkipListBenchmarks
{
    const int ItemCount = 5_000;
    int[] _data = [];

    [GlobalSetup]
    public void Setup()
    {
        Random random = new(7);
        _data = [.. Enumerable.Range(0, ItemCount).OrderBy(_ => random.Next())];
    }

    [Benchmark(Baseline = true)]
    public long SortedSet_InsertAndEnumerate()
    {
        SortedSet<int> set = [];

        foreach (int value in _data)
        {
            set.Add(value);
        }

        long sum = 0;

        foreach (int value in set)
        {
            sum += value;
        }

        return sum;
    }

    [Benchmark]
    public long SkipList_InsertAndEnumerate()
    {
        SkipList<int> list = new();

        foreach (int value in _data)
        {
            list.Add(value);
        }

        long sum = 0;

        foreach (int value in list)
        {
            sum += value;
        }

        return sum;
    }
}

///<summary>
///Compares <see cref="BloomFilter{T}"/>'s probabilistic membership test against an exact
///<see cref="HashSet{T}"/> lookup, at a 1% target false-positive rate.
///</summary>
[MemoryDiagnoser]
public class BloomFilterBenchmarks
{
    const int ItemCount = 10_000;
    const int LookupCount = 10_000;

    BloomFilter<int> _bloomFilter = null!;
    HashSet<int> _hashSet = null!;
    int[] _lookups = [];

    [GlobalSetup]
    public void Setup()
    {
        _bloomFilter = new BloomFilter<int>(ItemCount);
        _hashSet = [];

        for (int i = 0; i < ItemCount; i++)
        {
            _bloomFilter.Add(i);
            _hashSet.Add(i);
        }

        Random random = new(3);
        _lookups = new int[LookupCount];

        for (int i = 0; i < _lookups.Length; i++)
        {
            _lookups[i] = random.Next(0, ItemCount * 2);
        }
    }

    [Benchmark(Baseline = true)]
    public int HashSet_Contains()
    {
        int hits = 0;

        foreach (int value in _lookups)
        {
            if (_hashSet.Contains(value))
            {
                hits++;
            }
        }

        return hits;
    }

    [Benchmark]
    public int BloomFilter_MightContain()
    {
        int hits = 0;

        foreach (int value in _lookups)
        {
            if (_bloomFilter.MightContain(value))
            {
                hits++;
            }
        }

        return hits;
    }
}
