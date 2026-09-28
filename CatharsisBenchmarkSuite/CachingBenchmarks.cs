using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using Catharsis.Caching;

namespace CatharsisBenchmarkSuite;

///<summary>
///Compares <see cref="MemoCache{TKey, TValue}"/>'s <c>Lazy&lt;T&gt;</c>-wrapped <c>GetOrAdd</c> against a raw
///<see cref="ConcurrentDictionary{TKey, TValue}"/>, to measure the cost of guaranteeing the factory runs at most
///once per key under contention.
///</summary>
[MemoryDiagnoser]
public class MemoCacheBenchmarks
{
    const int OperationCount = 10_000;
    const int KeySpace = 100;

    [Benchmark(Baseline = true)]
    public int ConcurrentDictionary_GetOrAdd()
    {
        ConcurrentDictionary<int, int> dictionary = new();

        for(int i = 0; i < OperationCount; i++)
        {
            dictionary.GetOrAdd(i % KeySpace, static k => k * 2);
        }

        return dictionary.Count;
    }

    [Benchmark]
    public int MemoCache_GetOrAdd()
    {
        MemoCache<int, int> cache = new();

        for(int i = 0; i < OperationCount; i++)
        {
            cache.GetOrAdd(i % KeySpace, static k => k * 2);
        }

        return cache.Count;
    }
}
